using Microsoft.JSInterop;
using Ui.Shared.Models;
using Ui.Shared.Ports;

namespace Ui.Web.Services;

/// <summary>
/// Web implementation of IViewStateService that tracks viewport size via JavaScript interop.
/// Detects Phone (&lt;768px), Tablet (768-1023px), or Desktop (&ge;1024px).
/// </summary>
public class WebViewStateService : IViewStateService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<WebViewStateService>? _dotNetHelper;
    private ViewState _current = ViewState.Desktop;
    private bool _disposed;
    private bool _initialized;

    public ViewState Current => _current;

    public event Action<ViewState>? OnViewStateChanged;

    public WebViewStateService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Must be called from OnAfterRenderAsync to safely invoke JavaScript interop.
    /// </summary>
    public async Task EnsureInitializedAsync()
    {
        if (_initialized) return;
        _initialized = true;

        try
        {
            _dotNetHelper = DotNetObjectReference.Create(this);

            // Get initial viewport size
            var width = await _jsRuntime.InvokeAsync<int>(
                "eval",
                "() => window.innerWidth");

            UpdateViewState(width);

            // Register resize listener
            await _jsRuntime.InvokeVoidAsync(
                "addOnResizeHandler",
                _dotNetHelper);
        }
        catch (Exception ex)
        {
            // Fallback to Desktop if JS fails
            Console.WriteLine($"JS interop failed in WebViewStateService: {ex.Message}");
            _current = ViewState.Desktop;
        }
    }

    [JSInvokable]
    public void OnResize(int width)
    {
        UpdateViewState(width);
    }

    private void UpdateViewState(int width)
    {
        var newState = width < 768 ? ViewState.Phone
            : width < 1024 ? ViewState.Tablet
            : ViewState.Desktop;

        if (newState != _current)
        {
            _current = newState;
            OnViewStateChanged?.Invoke(_current);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        try
        {
            if (_initialized)
            {
                await _jsRuntime.InvokeVoidAsync("eval", "() => { /* cleanup resize listener if implemented */ }");
            }
        }
        catch
        {
            // Ignore JS errors during disposal
        }

        _dotNetHelper?.Dispose();
        _disposed = true;
    }
}
