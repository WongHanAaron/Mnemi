using Microsoft.JSInterop;
using Mnemi.Ui.Shared.Ports;
using Mnemi.Ui.Shared.Models;

namespace Mnemi.Ui.Web.Services;

/// <summary>
/// Web-specific ViewState service that detects viewport size via JavaScript interop
/// and raises OnViewStateChanged when the browser window is resized.
/// </summary>
public sealed class WebViewStateService : IViewStateService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<WebViewStateService>? _dotNetRef;
    private ViewState _current = ViewState.Desktop;

    /// <inheritdoc />
    public ViewState Current => _current;

    /// <inheritdoc />
    public event Action<ViewState>? OnViewStateChanged;

    public WebViewStateService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Initializes the service by registering a JS resize listener.
    /// Call this during app startup (e.g., OnAfterRenderAsync of the main layout).
    /// </summary>
    public async Task InitializeAsync()
    {
        _dotNetRef = DotNetObjectReference.Create(this);

        // Get initial viewport width
        var initialWidth = await _jsRuntime.InvokeAsync<double>("mnemi.getViewportWidth");

        // Register resize listener
        await _jsRuntime.InvokeVoidAsync("mnemi.registerResizeListener", _dotNetRef);

        UpdateFromWidth(initialWidth);
    }

    /// <summary>
    /// Called by JavaScript when the window is resized.
    /// </summary>
    [JSInvokable]
    public void OnWindowResized(double width)
    {
        UpdateFromWidth(width);
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

    public async ValueTask DisposeAsync()
    {
        _dotNetRef?.Dispose();
        _dotNetRef = null;
    }
}
