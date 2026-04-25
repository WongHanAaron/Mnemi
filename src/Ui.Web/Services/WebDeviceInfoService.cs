using Mnemi.Ui.Components.Services;

namespace Mnemi.Ui.Web.Services;

public class WebDeviceInfoService : IDeviceInfoService
{
    public string Platform => "Browser";
    public string OSVersion => "Unknown";
}
