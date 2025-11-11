using Android.Bluetooth;


namespace Shiny.BluetoothLE.Hosting.Internals;

public class DescriptorWriteEventArgs(
    BluetoothGattDescriptor descriptor,
    BluetoothDevice device,
    int requestId,
    int offset,
    bool preparedWrite,
    bool responseNeeded,
    byte[] value
) : WriteRequestEventArgs(device, requestId, offset, preparedWrite, responseNeeded, value)
{
    public BluetoothGattDescriptor Descriptor => descriptor;
}