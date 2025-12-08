using System.Collections.Concurrent;

namespace LoreSoft.Blazor.Controls.Utilities;

/// <summary>
/// A high-performance, lock-free container for managing multiple <see cref="IDisposable"/> instances.
/// Thread-safe using <see cref="ConcurrentStack{T}"/> for optimal performance.
/// Disposes all contained disposables in LIFO order (stack order) when disposed.
/// </summary>
public sealed class DisposableBag : IDisposable
{
    private ConcurrentStack<IDisposable>? _disposables;
    private volatile bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisposableBag"/> class.
    /// </summary>
    public DisposableBag()
    {
        _disposables = new ConcurrentStack<IDisposable>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DisposableBag"/> class with the specified collection of disposables.
    /// </summary>
    /// <param name="collection">The collection of disposables to add to the bag.</param>
    public DisposableBag(IEnumerable<IDisposable> collection)
    {
        _disposables = new ConcurrentStack<IDisposable>(collection);
    }

    /// <summary>
    /// Adds a disposable instance to the bag.
    /// </summary>
    /// <typeparam name="T">The type of disposable to add.</typeparam>
    /// <param name="disposable">The disposable to add.</param>
    /// <returns>The same disposable instance for fluent chaining.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the bag has been disposed.</exception>
    public DisposableBag Add<T>(T disposable) where T : IDisposable
    {
        if (disposable == null)
            return this;

        ThrowIfDisposed();

        var stack = _disposables;
        if (stack == null)
            throw new ObjectDisposedException(nameof(DisposableBag));

        stack.Push(disposable);
        return this;
    }

    /// <summary>
    /// Creates a disposable instance using its parameterless constructor and adds it to the bag.
    /// </summary>
    /// <typeparam name="T">The type of disposable to create. Must have a parameterless constructor.</typeparam>
    /// <returns>The created disposable instance.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the bag has been disposed.</exception>
    public T Create<T>() where T : IDisposable, new()
    {
        ThrowIfDisposed();

        var instance = new T();
        Add(instance);

        return instance;
    }

    /// <summary>
    /// Creates a disposable instance using the specified factory function and adds it to the bag.
    /// </summary>
    /// <typeparam name="T">The type of disposable to create.</typeparam>
    /// <param name="factory">Factory function to create the disposable.</param>
    /// <returns>The created disposable instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="factory"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the bag has been disposed.</exception>
    public T Create<T>(Func<T> factory) where T : IDisposable
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        ThrowIfDisposed();

        var instance = factory();
        Add(instance);

        return instance;
    }

    /// <summary>
    /// Gets a value indicating whether the bag has been disposed.
    /// </summary>
    /// <value><see langword="true"/> if the bag has been disposed; otherwise, <see langword="false"/>.</value>
    public bool IsDisposed => _isDisposed;

    /// <summary>
    /// Disposes all contained disposables in LIFO order (stack order).
    /// Exceptions thrown during disposal are swallowed to ensure all items are disposed.
    /// This method is idempotent and can be called multiple times safely.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
            return;

        // Atomically swap out the stack
        var stack = Interlocked.Exchange(ref _disposables, null);
        if (stack == null)
            return;

        _isDisposed = true;

        // Dispose all items - ConcurrentStack naturally provides LIFO order
        while (stack.TryPop(out var disposable))
        {
            try
            {
                disposable?.Dispose();
            }
            catch
            {
                // Swallow exceptions to ensure all items are disposed
            }
        }
    }

    /// <summary>
    /// Throws an <see cref="ObjectDisposedException"/> if the bag has been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the bag has been disposed.</exception>
    private void ThrowIfDisposed()
        => ObjectDisposedException.ThrowIf(_isDisposed, nameof(DisposableBag));
}
