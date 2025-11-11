using System;
using Android.Bluetooth;


namespace Shiny.BluetoothLE.Hosting.Internals;

public class GattEventArgs(BluetoothDevice device) : EventArgs
{
    public BluetoothDevice Device => device;
}
