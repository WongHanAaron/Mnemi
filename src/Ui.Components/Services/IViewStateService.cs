using System;
using Mnemi.Domain.Enums;

namespace Mnemi.Ui.Components.Services;

public interface IViewStateService
{
    ViewState Current { get; }
    event Action<ViewState>? OnChanged;
}
