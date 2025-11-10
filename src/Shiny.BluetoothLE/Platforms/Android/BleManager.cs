using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using Android;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Microsoft.Extensions.Logging;
using Shiny.BluetoothLE.Intrastructure;
using SR = Android.Bluetooth.LE.ScanResult;

namespace Shiny.BluetoothLE;


public partial class BleManager(
    IServiceProvider services,
    IOperationQueue operations,
    ILogger<IBleManager> logger,
    ILogger<IPeripheral> peripheralLogger
) : ScanCallback, IBleManager, IShinyStartupTask
{
    public const string BroadcastReceiverName = "org.shiny.bluetoothle.ShinyBleCentralBroadcastReceiver";

    public AccessState CurrentAccess
    {
        get
        {
            var perms = GetPlatformPermissions();
            var states = perms.Select(AndroidShinyHost.GetCurrentPermissionStatus).ToList();
            
            if (states.Any(x => x == AccessState.Denied))
                return AccessState.Denied;

            if (states.Any(x => x == AccessState.Unknown))
                return AccessState.Unknown;

            return AccessState.Available;
        }
    }

    public bool IsScanning { get; private set; }

    BluetoothManager? native;
    public BluetoothManager Native => this.native ??= AndroidShinyHost.GetSystemService<BluetoothManager>(Context.BluetoothService);


    public IObservable<(Peripheral Peripheral, Intent Intent)> PeripheralIntents => this.peripheralEventSubj;
    readonly Subject<(Peripheral Peripheral, Intent Intent)> peripheralEventSubj = new();


    static T? GetParcel<T>(Intent intent, string name) where T : Java.Lang.Object
    {
        Java.Lang.Object? result;
        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            var javaCls = Java.Lang.Class.FromType(typeof(T));
            if (javaCls == null)
                throw new InvalidOperationException("Invalid java type");

            result = intent.GetParcelableExtra(name, javaCls);
            return (T)result;
        }

        result = intent.GetParcelableExtra(name);
        return (T)result;
    }

    public void Start()
    {
        ShinyBleBroadcastReceiver.Process = async intent =>
        {
            if (intent?.Action == Intent.ActionBootCompleted || intent?.Action == null)
                return;

            var device = GetParcel<BluetoothDevice>(intent, BluetoothDevice.ExtraDevice);
            if (device != null)
            {
                var peripheral = this.GetPeripheral(device);

                switch (intent.Action)
                {
                    case BluetoothDevice.ActionAclConnected:
                    case BluetoothDevice.ActionAclDisconnected:
                        // bg state
                        await services
                            .RunDelegates<IBleDelegate>(
                                x => x.OnPeripheralStateChanged(peripheral),
                                logger
                            )
                            .ConfigureAwait(false);
                        break;

                    default:
                        //    public override IObservable<BleException> WhenConnectionFailed() => this.Context.ConnectionFailed;

                        //    public override IObservable<string> WhenNameUpdated() => this.Context
                        //        .ManagerContext
                        //        .ListenForMe(BluetoothDevice.ActionNameChanged, this)
                        //        .Select(_ => this.Name);
                        this.peripheralEventSubj.OnNext((peripheral, intent));
                        break;
                }
            }
        };

        AndroidShinyHost.RegisterBroadcastReceiver<ShinyBleBroadcastReceiver>(
            true,
            BluetoothDevice.ActionNameChanged,
            BluetoothDevice.ActionBondStateChanged,
            BluetoothDevice.ActionPairingRequest,
            BluetoothDevice.ActionAclConnected,
            BluetoothDevice.ActionAclDisconnected,
            Intent.ActionBootCompleted
        );
        ShinyBleAdapterStateBroadcastReceiver.Process = async intent =>
        {
            if (intent?.Action != BluetoothAdapter.ActionStateChanged)
                return;

            var newState = (State)intent.GetIntExtra(BluetoothAdapter.ExtraState, -1);
            if (newState == State.On || newState == State.Off)
            {
                var status = newState == State.On
                    ? AccessState.Available
                    : AccessState.Disabled;

                await services
                    .RunDelegates<IBleDelegate>(
                        del => del.OnAdapterStateChanged(status),
                        logger
                    )
                    .ConfigureAwait(false);
            }
        };

        AndroidShinyHost.RegisterBroadcastReceiver<ShinyBleAdapterStateBroadcastReceiver>(
            true,
            BluetoothAdapter.ActionStateChanged,
            Intent.ActionBootCompleted
        );
    }

    public IObservable<AccessState> RequestAccess() => Observable.FromAsync(async ct =>
    {
        var versionPermissions = GetPlatformPermissions();

        // TODO
        // if (!versionPermissions.All(x => this.platform.IsInManifest(x)))
        //     return AccessState.NotSetup;

        // var results = await AndroidShinyHost
        //     .RequestPermissions(versionPermissions)
        //     .ToTask(ct)
        //     .ConfigureAwait(false);
        //
        // return results.IsSuccess()
        //     ? this.Native.GetAccessState() // now look at the actual device state
        //     : AccessState.Denied;
        return AccessState.Available;
    });


    readonly Subject<(SR? Native, ScanFailure? Failure)> scanSubj = new();
    public IObservable<ScanResult> Scan(ScanConfig? config = null) => this.RequestAccess()
        .Do(x => Assert(x))
        .Select(x => Observable.Create<ScanResult>(ob =>
        {
            if (this.IsScanning)
                throw new InvalidOperationException("There is already an active scan");
        
            this.Clear();

            var disp = this.scanSubj.Subscribe(x =>
            {
                if (x.Failure == null)
                {
                    if (x.Native != null)
                    {
                        ob.OnNext(this.FromNative(x.Native));
                    }
                }
                else
                {
                    ob.OnError(new InvalidOperationException("Scan Error: " + x.Failure));
                }
            });
            this.StartScan(config);

            return () =>
            {
                this.IsScanning = false;
                this.StopScan();
                disp?.Dispose();
            };
        }))
        .Switch();


    public void StopScan()
        => this.Native.Adapter!.BluetoothLeScanner?.StopScan(this);

    public IEnumerable<IPeripheral> GetConnectedPeripherals()
        => this.peripherals.Where(x => x.Value.Status == ConnectionState.Connected).Select(x => x.Value);

    public IPeripheral? GetKnownPeripheral(string peripheralUuid)
        => this.peripherals.Values.FirstOrDefault(x => x.Uuid.Equals(peripheralUuid, StringComparison.InvariantCultureIgnoreCase));

    public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, SR? result)
        => this.scanSubj.OnNext((result, null));

    public override void OnScanFailed([GeneratedEnum] ScanFailure errorCode)
        => this.scanSubj.OnNext((null, errorCode));        

    public override void OnBatchScanResults(IList<SR>? results)
    {
        if (results == null)
            return;

        foreach (var result in results)
            this.scanSubj.OnNext((result, null));
    }


    readonly ConcurrentDictionary<string, Peripheral> peripherals = new();
    Peripheral GetPeripheral(BluetoothDevice device) => this.peripherals.GetOrAdd(
        device.Address!,
        _ => new Peripheral(this, device, operations, peripheralLogger)
    );


    protected ScanResult FromNative(SR native)
    {
        var peripheral = this.GetPeripheral(native.Device!);
        var ad = new AdvertisementData(native);
        return new ScanResult(peripheral, native.Rssi, ad);
    }


    void StartScan(ScanConfig? config)
    {
        AndroidScanConfig cfg;
        if (config == null)
            cfg = new();
        else if (config is AndroidScanConfig cfg1)
            cfg = cfg1;
        else
            cfg = new AndroidScanConfig(ServiceUuids: config.ServiceUuids);

        var builder = new ScanSettings.Builder();
        builder.SetScanMode(cfg.ScanMode);

        var scanFilters = new List<ScanFilter>();
        if (cfg.ServiceUuids.Length > 0)
        {
            foreach (var uuid in cfg.ServiceUuids)
            {
                var fullUuid = Utils.ToUuidType(uuid);
                var parcel = new ParcelUuid(fullUuid);
                scanFilters.Add(new ScanFilter.Builder()
                    .SetServiceUuid(parcel)!
                    .Build()!
                );
            }
        }

        if (cfg.UseScanBatching && this.Native.Adapter!.IsOffloadedScanBatchingSupported)
            builder.SetReportDelay(100);

        if (OperatingSystem.IsAndroidVersionAtLeast(26))
            builder.SetLegacy(false);
        
        this.Native.Adapter!.BluetoothLeScanner!.StartScan(
            scanFilters,
            builder.Build(),
            this
        );
        this.IsScanning = true;
    }


    void Clear() => this.peripherals
        .Where(x => x.Value.Status != ConnectionState.Connected)
        .ToList()
        .ForEach(x => this.peripherals.TryRemove(x.Key, out var device));


    static string[] GetPlatformPermissions()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            return [
                Manifest.Permission.BluetoothScan,
                Manifest.Permission.BluetoothConnect
            ];
        }
        return [
            Manifest.Permission.Bluetooth,
            Manifest.Permission.BluetoothAdmin,
            Manifest.Permission.AccessFineLocation
        ];
    }


    static void Assert(AccessState access)
    {
        if (access == AccessState.NotSetup)
        {
            var permissions = GetPlatformPermissions();
            var msgList = String.Join(", ", permissions);
            throw new InvalidOperationException("Your AndroidManifest.xml is missing 1 or more of the following permissions for this version of Android: " + msgList);
        }
        
        if (access != AccessState.Available)
            throw new PermissionException("BluetoothLE", access);
    }
}

//    public IEnumerable<Peripheral> GetConnectedDevices()
//    {
//        var nativeDevices = this.Manager.GetDevicesMatchingConnectionStates(ProfileType.Gatt, new[]
//        {
//            (int) ProfileState.Connecting,
//            (int) ProfileState.Connected
//        });
//        foreach (var native in nativeDevices)
//            yield return this.GetDevice(native);
//    }
