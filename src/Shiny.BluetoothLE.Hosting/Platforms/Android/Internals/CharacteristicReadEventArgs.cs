using Android.Bluetooth;


namespace Shiny.BluetoothLE.Hosting.Internals;

public class CharacteristicReadEventArgs(
    BluetoothDevice device,
    BluetoothGattCharacteristic characteristic,
    int requestId,
    int offset
) : GattRequestEventArgs(device, requestId, offset)
{

    public BluetoothGattCharacteristic Characteristic => characteristic;
}