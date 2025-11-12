namespace Shiny.BluetoothLE.Hosting;


public record GattResult(
    GattState Status,
    byte[]? Data
)
{
    public static GattResult Success(byte[] data) => new(GattState.Success, data);
    public static GattResult Error(GattState status) => new(status, null);
}
