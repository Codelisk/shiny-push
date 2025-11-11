using System;
using Android.Bluetooth;


namespace Shiny.BluetoothLE.Hosting.Internals;

public class MtuChangedEventArgs(BluetoothDevice device, int mtu) : EventArgs
{
    public BluetoothDevice Device => device;
    public int Mtu => mtu;
}
