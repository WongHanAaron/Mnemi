using Mnemi.Ui.Components.Services;

namespace Mnemi.Ui.Maui.Services;

public class MauiDeviceInfoService : IDeviceInfoService
{
    public string Platform => Microsoft.Maui.Devices.DeviceInfo.Current.Platform.ToString();
    public string OSVersion => Microsoft.Maui.Devices.DeviceInfo.Current.VersionString;
}
