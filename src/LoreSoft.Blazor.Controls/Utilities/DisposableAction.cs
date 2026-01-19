namespace LoreSoft.Blazor.Controls;

/// <summary>
/// An allocation-free disposable struct that executes an action when disposed. The action will only be invoked once, even if Dispose is called multiple times.
/// </summary>
/// <param name="action">The action to execute on disposal.</param>
public struct DisposableAction(Action? action) : IDisposable
{
    private Action? _action = action;

    /// <summary>
    /// Executes the action provided during construction. Subsequent calls to Dispose will have no effect.
    /// </summary>
    public void Dispose()
    {
        var actionToInvoke = Interlocked.Exchange(ref _action, null);
        actionToInvoke?.Invoke();
    }
}

