using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Ui.Shared.Components.Helpers;

/// <summary>
/// Base class for components that load data asynchronously.
/// Provides shared loading/error state management and CancellationToken
/// propagation to reduce boilerplate across card components.
/// </summary>
public abstract class AsyncDataComponent : ComponentBase, IDisposable
{
    private CancellationTokenSource? _cts;

    /// <summary>
    /// A CancellationToken that is cancelled when the component is disposed.
    /// Pass this to async service calls to cancel in-flight operations when
    /// the user navigates away.
    /// </summary>
    protected CancellationToken ComponentCancellationToken
    {
        get
        {
            _cts ??= new CancellationTokenSource();
            return _cts.Token;
        }
    }

    [Inject]
    private ILogger<AsyncDataComponent> Logger { get; set; } = null!;

    protected bool IsLoading { get; set; } = true;
    protected string? ErrorMessage { get; set; }

    /// <summary>
    /// Wraps an async data-loading operation with loading/error state.
    /// Sets IsLoading = true before, catches and logs exceptions, and sets
    /// IsLoading = false in finally.
    /// </summary>
    protected async Task LoadAsync(Func<Task> loadTask, string fallbackErrorMessage)
    {
        IsLoading = true;
        ErrorMessage = null;
        StateHasChanged();

        try
        {
            await loadTask();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Data load failed in {ComponentType}", GetType().Name);
            ErrorMessage = fallbackErrorMessage;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Cancels pending async operations when the component is removed.
    /// </summary>
    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }
}
