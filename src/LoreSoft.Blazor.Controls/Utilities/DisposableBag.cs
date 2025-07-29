namespace LoreSoft.Blazor.Controls.Utilities;

/// <summary>
/// Provides a stack-based container for managing and disposing multiple <see cref="IDisposable"/> objects.
/// Ensures all included disposables are disposed in reverse order when the bag is disposed.
/// </summary>
public readonly struct DisposableBag() : IDisposable
{
    private readonly Stack<IDisposable> _disposeStack = new();

    /// <summary>
    /// Creates a new instance of <typeparamref name="T"/> and adds it to the bag for disposal.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="IDisposable"/> to create and manage.</typeparam>
    /// <returns>The created disposable instance.</returns>
    public T Create<T>() where T : IDisposable, new()
    {
        var disposable = new T();
        _disposeStack.Push(disposable);

        return disposable;
    }

    /// <summary>
    /// Creates a new instance of <typeparamref name="T"/> using the specified factory and adds it to the bag for disposal.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="IDisposable"/> to create and manage.</typeparam>
    /// <param name="creator">A factory function to create the disposable instance.</param>
    /// <returns>The created disposable instance.</returns>
    public T Create<T>(Func<T> creator) where T : IDisposable
    {
        var disposable = creator();
        _disposeStack.Push(disposable);

        return disposable;
    }

    /// <summary>
    /// Adds an existing <see cref="IDisposable"/> instance to the bag for disposal.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="IDisposable"/> to include.</typeparam>
    /// <param name="disposable">The disposable instance to include.</param>
    /// <returns>The included disposable instance.</returns>
    public T Include<T>(T disposable) where T : IDisposable
    {
        _disposeStack.Push(disposable);

        return disposable;
    }

    /// <summary>
    /// Disposes all <see cref="IDisposable"/> objects in the bag in reverse order of inclusion.
    /// </summary>
    public void Dispose()
    {
        while (_disposeStack.Count > 0)
        {
            var disposable = _disposeStack.Pop();
            disposable.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}
