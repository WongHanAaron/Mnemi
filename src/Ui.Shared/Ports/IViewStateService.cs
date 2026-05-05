using Mnemi.Ui.Shared.Models;

namespace Mnemi.Ui.Shared.Ports;

/// <summary>
/// Provides the current layout viewing state based on host viewport size.
/// Implemented by host-specific services (Web: browser resize, MAUI: window size).
/// Shared components consume this interface — never the host implementations directly.
/// </summary>
public interface IViewStateService
{
    /// <summary>Gets the current ViewState (Phone, Tablet, or Desktop).</summary>
    ViewState Current { get; }

    /// <summary>Raised when the ViewState changes (e.g., window resize or orientation change).</summary>
    event Action<ViewState> OnViewStateChanged;
}
