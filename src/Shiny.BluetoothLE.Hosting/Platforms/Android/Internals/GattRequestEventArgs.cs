using Android.Bluetooth;


namespace Shiny.BluetoothLE.Hosting.Internals;

public class GattRequestEventArgs(BluetoothDevice device, int requestId, int offset) : GattEventArgs(device)
{
    public int RequestId => requestId;
    public int Offset => offset;
}
