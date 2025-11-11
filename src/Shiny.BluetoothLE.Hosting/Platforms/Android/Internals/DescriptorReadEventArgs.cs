using Android.Bluetooth;


namespace Shiny.BluetoothLE.Hosting.Internals;

public class DescriptorReadEventArgs(
    BluetoothGattDescriptor descriptor,
    BluetoothDevice device,
    int requestId,
    int offset
) : GattRequestEventArgs(device, requestId, offset)
{
    public BluetoothGattDescriptor Descriptor => descriptor;
}