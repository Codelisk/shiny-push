using Android.Bluetooth;


namespace Shiny.BluetoothLE.Hosting.Internals;

public class CharacteristicWriteEventArgs(
    BluetoothGattCharacteristic characteristic,
    BluetoothDevice device,
    int requestId,
    int offset,
    bool preparedWrite,
    bool responseNeeded,
    byte[] value
) : WriteRequestEventArgs(device, requestId, offset, preparedWrite, responseNeeded, value)
{
    public BluetoothGattCharacteristic Characteristic => characteristic;
}
