using Android.Bluetooth;


namespace Shiny.BluetoothLE.Hosting.Internals;

public class ConnectionStateChangeEventArgs(BluetoothDevice device, ProfileState oldState, ProfileState newState) : GattEventArgs(device)
{
    public ProfileState OldState => oldState;
    public ProfileState NewState => newState;
}
