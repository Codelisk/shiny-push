using System;
using System.Collections.Generic;
using System.Linq;
using CoreBluetooth;

namespace Shiny.BluetoothLE.Hosting;


public class GattService(CBPeripheralManager manager, string uuid, bool primary) : IGattService, IGattServiceBuilder, IDisposable
{
    readonly List<GattCharacteristic> characteristics = new();
    public CBMutableService Native { get; } = new(CBUUID.FromString(uuid), primary);
    public string Uuid => uuid;
    public bool Primary => primary;
    public IReadOnlyList<IGattCharacteristic> Characteristics => this.characteristics.Cast<IGattCharacteristic>().ToList();


    public IGattCharacteristic AddCharacteristic(string uuid, Action<IGattCharacteristicBuilder> characteristicBuilder)
    {
        var ch = new GattCharacteristic(manager, uuid);
        characteristicBuilder(ch);
        ch.Build(this.Native);

        this.characteristics.Add(ch);
        return ch;
    }


    public void Dispose()
    {
        foreach (var ch in this.characteristics)
            ch.Dispose();
    }
}
