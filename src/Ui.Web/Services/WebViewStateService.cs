using System;
using Microsoft.JSInterop;
using Mnemi.Domain.Enums;
using Mnemi.Ui.Components.Services;

namespace Mnemi.Ui.Web.Services;

public class WebViewStateService : IViewStateService, IDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private ViewState _current = ViewState.Desktop;

    public ViewState Current => _current;
    public event Action<ViewState>? OnChanged;

    public WebViewStateService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        // In a real app, you'd use JS interop to listen for window resize
    }

    public void SetViewState(ViewState newState)
    {
        if (_current != newState)
        {
            _current = newState;
            OnChanged?.Invoke(_current);
        }
    }

    public void Dispose()
    {
        // Cleanup JS event listeners
    }
}
