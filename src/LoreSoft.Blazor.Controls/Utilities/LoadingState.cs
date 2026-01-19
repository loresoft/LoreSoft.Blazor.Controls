namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Tracks the loading state of an operation using a reference counter.
/// </summary>
public class LoadingState
{
    private int _count;

    /// <summary>
    /// Gets a value indicating whether the state is currently loading.
    /// </summary>
    public bool IsLoading => _count > 0;

    /// <summary>
    /// Starts a loading operation by incrementing the counter and returns a disposable that will automatically stop the operation when disposed.
    /// </summary>
    /// <returns>An <see cref="DisposableAction"/> that will call <see cref="Stop"/> when disposed.</returns>
    public DisposableAction Start()
    {
        Interlocked.Increment(ref _count);
        return new DisposableAction(Stop);
    }

    /// <summary>
    /// Stops a loading operation by decrementing the counter.
    /// </summary>
    public void Stop()
        => Interlocked.Decrement(ref _count);

    /// <summary>
    /// Resets the loading state counter to zero.
    /// </summary>
    public void Reset()
        => Interlocked.Exchange(ref _count, 0);
}

