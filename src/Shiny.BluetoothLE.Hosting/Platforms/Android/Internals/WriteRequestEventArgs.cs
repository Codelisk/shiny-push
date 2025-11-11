using System;
using Android.Bluetooth;


namespace Shiny.BluetoothLE.Hosting.Internals;

public abstract class WriteRequestEventArgs(
    BluetoothDevice device,
    int requestId,
    int offset,
    bool preparedWrite,
    bool responseNeeded,
    byte[] value
) : GattRequestEventArgs(device, requestId, offset)
{

    public bool ResponseNeeded => responseNeeded;
    public bool PreparedWrite => preparedWrite;
    public byte[] Value => value;
}
