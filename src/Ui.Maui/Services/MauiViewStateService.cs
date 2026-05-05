using Microsoft.Maui.Controls;
using Mnemi.Ui.Shared.Ports;
using Mnemi.Ui.Shared.Models;

namespace Mnemi.Ui.Maui.Services;

/// <summary>
/// MAUI-specific ViewState service that detects window size changes
/// from the MAUI Window object and raises OnViewStateChanged.
/// </summary>
public sealed class MauiViewStateService : IViewStateService, IDisposable
{
    private ViewState _current = ViewState.Desktop;

    /// <inheritdoc />
    public ViewState Current => _current;

    /// <inheritdoc />
    public event Action<ViewState>? OnViewStateChanged;

    /// <summary>
    /// Initializes the service by subscribing to the MAUI window size changed event.
    /// Call this during app startup.
    /// </summary>
    public void Initialize()
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is not null)
        {
            window.SizeChanged += OnWindowSizeChanged;
            UpdateFromWidth(window.Width);
        }
    }

    private void OnWindowSizeChanged(object? sender, EventArgs e)
    {
        if (sender is Window window)
        {
            UpdateFromWidth(window.Width);
        }
    }

    private void UpdateFromWidth(double width)
    {
        var newState = ViewBreakpoints.GetViewState(width);
        if (newState != _current)
        {
            _current = newState;
            OnViewStateChanged?.Invoke(_current);
        }
    }

    public void Dispose()
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is not null)
        {
            window.SizeChanged -= OnWindowSizeChanged;
        }
    }
}
