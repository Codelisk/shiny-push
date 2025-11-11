using System;
using Android.Bluetooth.LE;


namespace Shiny.BluetoothLE.Hosting.Internals;

public class AdvertisementCallbacks(Action onStart, Action<Exception> onError) : AdvertiseCallback
{
    public override void OnStartSuccess(AdvertiseSettings settingsInEffect)
    {
        base.OnStartSuccess(settingsInEffect);
        onStart.Invoke();
    }


    public override void OnStartFailure(AdvertiseFailure errorCode)
    {
        base.OnStartFailure(errorCode);
        onError.Invoke(new ArgumentException($"Failed to start BLE advertising - {errorCode}"));
    }
}
