using Ui.Shared.Ports;
using SharedViewState = Ui.Shared.Models.ViewState;

namespace Ui.Services;

/// <summary>
/// MAUI implementation of IViewStateService.
/// Detects device idiom (Phone/Tablet/Desktop) for responsive sidebar layout.
/// </summary>
public class MauiViewStateService : IViewStateService
{
    public SharedViewState Current
    {
        get
        {
            var idiom = DeviceInfo.Idiom;
            if (idiom == DeviceIdiom.Phone) return SharedViewState.Phone;
            if (idiom == DeviceIdiom.Tablet) return SharedViewState.Tablet;
            return SharedViewState.Desktop;
        }
    }

    public event Action<SharedViewState>? OnViewStateChanged;
}
