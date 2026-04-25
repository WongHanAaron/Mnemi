using System;
using Mnemi.Domain.Enums;
using Mnemi.Ui.Components.Services;

namespace Mnemi.Ui.Maui.Services;

public class MauiViewStateService : IViewStateService
{
    private ViewState _current = ViewState.Phone;

    public ViewState Current => _current;
    public event Action<ViewState>? OnChanged;

    public void SetViewState(ViewState newState)
    {
        if (_current != newState)
        {
            _current = newState;
            OnChanged?.Invoke(_current);
        }
    }
}
