using CoreBluetooth;

namespace Shiny.BluetoothLE.Hosting;


public class Peripheral(CBCentral central) : IPeripheral
{
    public string Uuid { get; } = central.Identifier.ToString();
    public CBCentral Central { get; }
    public object? Context { get; set; }
    public int Mtu => (int)this.Central.MaximumUpdateValueLength;
}