using Ui.Shared.Models;
using Ui.Shared.Ports;

namespace Ui.Web.Services;

/// <summary>
/// Stub implementation of IViewStateService that always returns Desktop.
/// Replaced by WebViewStateService in Phase 5.
/// </summary>
public class StubViewStateService : IViewStateService
{
    public ViewState Current => ViewState.Desktop;

    public event Action<ViewState>? OnViewStateChanged;
}
