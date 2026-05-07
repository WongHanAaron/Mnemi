using Ui.Shared.Models;
using Ui.Shared.Ports;

namespace Ui.Web.Client.Services;

/// <summary>
/// Client-side stub of IViewStateService. Always returns Desktop.
/// Replaced by browser-based implementation in Phase 5.
/// </summary>
public class StubViewStateService : IViewStateService
{
    public ViewState Current => ViewState.Desktop;

    public event Action<ViewState>? OnViewStateChanged;
}
