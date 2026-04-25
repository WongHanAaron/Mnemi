namespace Mnemi.Ui.Components.Services;

public interface IDeviceInfoService
{
    string Platform { get; }
    string OSVersion { get; }
}
