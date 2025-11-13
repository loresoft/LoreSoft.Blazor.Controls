using System.Collections.Concurrent;

namespace LoreSoft.Blazor.Controls.Utilities;

/// <summary>
/// Provides a thread-safe, zero-dependency object pool implementation for reusing objects to reduce garbage collection pressure.
/// </summary>
/// <typeparam name="T">The type of objects to pool. Must be a reference type.</typeparam>
/// <remarks>
/// <para>
/// This object pool implementation uses a <see cref="ConcurrentQueue{T}"/> for thread-safe storage with an additional
/// single-item cache for improved performance. It provides automatic return-to-pool functionality via the 
/// <see cref="PooledObject"/> disposable wrapper.
/// </para>
/// <para>
/// The pool enforces maximum size limits using atomic operations (<see cref="Interlocked"/>) to prevent race conditions.
/// Under extreme concurrency scenarios, the actual pool size may briefly exceed the configured limit before stabilizing.
/// </para>
/// <para>
/// <strong>Performance Characteristics:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Fast-path optimization: Uses a single-item cache (<c>_item</c>) for extremely fast Get/Return operations</description></item>
/// <item><description>Lock-free design: All operations use atomic operations instead of locks</description></item>
/// <item><description>Memory efficient: Respects maximum size limits to prevent unbounded growth</description></item>
/// <item><description>Zero allocations: The <see cref="PooledObject"/> wrapper is a struct to avoid heap allocations</description></item>
/// </list>
/// <para>
/// <strong>When to Use Object Pooling:</strong>
/// </para>
/// <para>
/// Object pools are most beneficial when:
/// </para>
/// <list type="bullet">
/// <item><description>Object creation is expensive (e.g., large allocations, complex initialization, network resources)</description></item>
/// <item><description>Objects are frequently created and discarded in high-throughput scenarios</description></item>
/// <item><description>Objects can be reset to a clean state for reuse without side effects</description></item>
/// <item><description>The cost of pooling overhead is less than the cost of object creation</description></item>
/// </list>
/// <para>
/// <strong>Thread Safety:</strong>
/// </para>
/// <para>
/// All public methods are thread-safe and can be safely called from multiple threads concurrently.
/// The pool uses lock-free synchronization primitives for optimal performance under contention.
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Example 1: Basic usage with StringBuilder pooling</strong></para>
/// <code>
/// // Create a pool for StringBuilder instances
/// var pool = new ObjectPool&lt;StringBuilder&gt;(
///     objectFactory: () => new StringBuilder(capacity: 256),
///     resetAction: sb => sb.Clear(),
///     maxSize: 100,
///     preallocate: 10
/// );
///
/// // Use with automatic return (recommended)
/// using (var pooled = pool.GetPooled())
/// {
///     pooled.Instance.Append("Hello, World!");
///     Console.WriteLine(pooled.Instance.ToString());
/// }
/// // Object is automatically returned to pool when disposed
/// </code>
///
/// <para><strong>Example 2: Manual get/return pattern</strong></para>
/// <code>
/// // Manual get/return (use only when 'using' pattern isn't suitable)
/// var sb = pool.Get();
/// try
/// {
///     sb.Append("Manual usage");
///     ProcessStringBuilder(sb);
/// }
/// finally
/// {
///     pool.Return(sb);
/// }
/// </code>
/// </example>
public class ObjectPool<T> where T : class
{
    private readonly ConcurrentQueue<T> _objects;
    private readonly Func<T> _objectFactory;
    private readonly Action<T>? _resetAction;
    private readonly int _maxSize;

    private int _count;
    private T? _item;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectPool{T}"/> class with the specified configuration.
    /// </summary>
    /// <param name="objectFactory">
    /// A factory function that creates new objects when the pool is empty. This function is called each time 
    /// a new instance is needed and should return a properly initialized object ready for use.
    /// The factory should be thread-safe if the pool will be used concurrently.
    /// </param>
    /// <param name="resetAction">
    /// An optional action that resets objects to a clean state before returning them to the pool. 
    /// Use this to clear state, reset properties, release resources, or prepare the object for reuse.
    /// </param>
    /// <param name="maxSize">
    /// The maximum number of objects to retain in the pool. Objects exceeding this limit will not be returned 
    /// to the pool and will be eligible for garbage collection. A value of 0 (default) uses a default maximum 
    /// of <c>Environment.ProcessorCount * 2</c>, which provides a good balance for most scenarios.
    /// </param>
    /// <param name="preallocate">
    /// The number of objects to create and add to the pool during initialization. This can improve performance 
    /// by avoiding allocation during initial usage at the cost of increased startup time and memory usage.
    /// Default is 0 (no preallocation).
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="objectFactory"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxSize"/> or <paramref name="preallocate"/> is negative.</exception>
    public ObjectPool(
        Func<T> objectFactory,
        Action<T>? resetAction = null,
        int maxSize = 0,
        int preallocate = 0)
    {
        ArgumentNullException.ThrowIfNull(objectFactory);
        ArgumentOutOfRangeException.ThrowIfNegative(maxSize);
        ArgumentOutOfRangeException.ThrowIfNegative(preallocate);

        _objectFactory = objectFactory;
        _resetAction = resetAction;
        _maxSize = maxSize;
        _objects = [];

        // Set default max size if unlimited
        if (_maxSize == 0)
            _maxSize = Environment.ProcessorCount * 2;

        if (preallocate <= 0)
            return;

        // Preallocate objects
        for (int i = 0; i < preallocate; i++)
        {
            _objects.Enqueue(objectFactory());
        }
    }

    /// <summary>
    /// Gets an object from the pool, or creates a new one if the pool is empty.
    /// </summary>
    /// <returns>
    /// An object from the pool if available; otherwise, a newly created instance using the object factory.
    /// The returned object is ready for use and should be returned to the pool via <see cref="Return"/> 
    /// when no longer needed, or obtained via <see cref="GetPooled"/> for automatic return.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Performance Optimization:</strong>
    /// </para>
    /// <para>
    /// This method uses a two-tier retrieval strategy for optimal performance:
    /// </para>
    /// <list type="number">
    /// <item><description><strong>Fast path:</strong> Attempts to retrieve from the single-item cache (<c>_item</c>) using a lock-free compare-and-exchange operation</description></item>
    /// <item><description><strong>Queue path:</strong> If the cache is empty, attempts to dequeue from the concurrent queue</description></item>
    /// <item><description><strong>Factory path:</strong> If the pool is empty, creates a new object using the factory function</description></item>
    /// </list>
    /// <para>
    /// <strong>Thread Safety:</strong>
    /// </para>
    /// <para>
    /// This method is thread-safe and can be called concurrently from multiple threads without external synchronization.
    /// The lock-free design ensures minimal contention and high throughput under concurrent access.
    /// </para>
    /// <para>
    /// <strong>Usage Pattern:</strong>
    /// </para>
    /// <para>
    /// When you are finished using the object, you must call <see cref="Return"/> to return it to the pool,
    /// or use <see cref="GetPooled"/> for automatic return via disposal (recommended).
    /// </para>
    /// <para>
    /// Failing to return objects to the pool will not cause errors or memory leaks (objects will be garbage collected),
    /// but it reduces the effectiveness of pooling and may lead to increased memory allocations and GC pressure.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Manual pattern (must remember to return)
    /// var obj = pool.Get();
    /// try
    /// {
    ///     // Use the object
    ///     obj.DoWork();
    /// }
    /// finally
    /// {
    ///     pool.Return(obj);  // Always return, even if exception occurs
    /// }
    ///
    /// // Automatic pattern (preferred - uses 'using' statement)
    /// using var pooled = pool.GetPooled();
    /// pooled.Instance.DoWork();  // Automatically returned at end of scope
    /// </code>
    /// </example>
    public T Get()
    {
        var item = _item;

        // Try to get from the single-item cache first
        if (item != null && Interlocked.CompareExchange(ref _item, null, item) == item)
            return item;

        // no object available, so go get a brand new one
        if (!_objects.TryDequeue(out item))
            return _objectFactory();

        // Successfully dequeued an item, decrement the count
        Interlocked.Decrement(ref _count);
        return item;
    }

    /// <summary>
    /// Returns an object to the pool for reuse by future callers.
    /// </summary>
    /// <param name="item">
    /// The object to return to the pool. Must not be <see langword="null"/>.
    /// Should be an object previously obtained from this pool via <see cref="Get"/> or <see cref="GetPooled"/>.
    /// </param>
    /// <remarks>
    /// <para>
    /// <strong>Reset Action Behavior:</strong>
    /// </para>
    /// <para>
    /// If a reset action was provided in the constructor, it will be invoked before the object is added back to the pool.
    /// If the reset action throws an exception, the object will not be returned to the pool and will be eligible 
    /// for garbage collection. This prevents corrupted or improperly reset objects from being reused and causing
    /// unexpected behavior in subsequent operations.
    /// </para>
    /// <para>
    /// <strong>Maximum Size Enforcement:</strong>
    /// </para>
    /// <para>
    /// If the pool has reached its maximum size, the object will not be added to the pool and will be eligible 
    /// for garbage collection. The maximum size is enforced using atomic operations (<see cref="Interlocked"/>),
    /// but under extreme concurrency the pool may briefly exceed the limit before stabilizing. This is by design
    /// to avoid expensive locking and is considered acceptable for most use cases.
    /// </para>
    /// <para>
    /// <strong>Two-Tier Storage:</strong>
    /// </para>
    /// <para>
    /// The pool uses a two-tier storage strategy:
    /// </para>
    /// <list type="number">
    /// <item><description><strong>Fast path:</strong> Attempts to store in the single-item cache (<c>_item</c>) for extremely fast subsequent retrievals</description></item>
    /// <item><description><strong>Queue path:</strong> If the cache is full, enqueues to the concurrent queue (if under maximum size)</description></item>
    /// </list>
    /// <para>
    /// <strong>Thread Safety:</strong>
    /// </para>
    /// <para>
    /// This method is thread-safe and silently ignores <see langword="null"/> items without throwing an exception.
    /// Multiple threads can safely return objects concurrently.
    /// </para>
    /// <para>
    /// <strong>Important Usage Notes:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Do not return the same object instance to the pool multiple times</description></item>
    /// <item><description>Do not continue using an object after returning it to the pool</description></item>
    /// <item><description>Do not return objects from a different pool</description></item>
    /// <item><description>The object may be retrieved and modified by other callers immediately after return</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var obj = pool.Get();
    /// try
    /// {
    ///     obj.DoWork();
    /// }
    /// finally
    /// {
    ///     pool.Return(obj);  // Always return in finally block
    /// }
    /// 
    /// // Don't do this:
    /// pool.Return(obj);
    /// obj.DoWork();      // ERROR: Object may be reused by another thread!
    /// pool.Return(obj);  // ERROR: Returning same object twice!
    /// </code>
    /// </example>
    public void Return(T item)
    {
        // Ignore null items
        if (item == null)
            return;

        try
        {
            // Reset the object before returning to pool
            _resetAction?.Invoke(item);
        }
        catch
        {
            // Don't return corrupted objects to pool
            return;
        }

        // Try to store in the single-item cache first
        if (_item == null && Interlocked.CompareExchange(ref _item, item, null) == null)
            return;

        // If the cache is full, we need to drop the item
        if (Interlocked.Increment(ref _count) > _maxSize)
            Interlocked.Decrement(ref _count);
        else
            _objects.Enqueue(item);
    }

    /// <summary>
    /// Gets a disposable wrapper that automatically returns the object to the pool when disposed.
    /// </summary>
    /// <returns>
    /// A <see cref="PooledObject"/> that wraps the pooled instance and implements <see cref="IDisposable"/>. 
    /// The object is automatically returned to the pool when the wrapper is disposed (e.g., at the end of a 
    /// <c>using</c> block or statement).
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Recommended Usage Pattern:</strong>
    /// </para>
    /// <para>
    /// This is the <strong>recommended</strong> way to use the object pool as it ensures objects are always 
    /// returned to the pool, even if an exception occurs during processing. The returned wrapper can be used 
    /// in a <c>using</c> statement or declaration to guarantee automatic return of the object.
    /// </para>
    /// <para>
    /// <strong>Struct Design for Performance:</strong>
    /// </para>
    /// <para>
    /// The wrapper is a <see langword="struct"/> (value type) to avoid additional heap allocations, making the 
    /// pooling mechanism truly zero-allocation. This is particularly important in high-throughput scenarios 
    /// where even small allocations can add up.
    /// </para>
    /// <para>
    /// <strong>Implicit Conversion:</strong>
    /// </para>
    /// <para>
    /// The wrapper supports implicit conversion to <typeparamref name="T"/>, allowing it to be used directly
    /// in most contexts where the underlying type is expected. However, accessing the <see cref="PooledObject.Instance"/>
    /// property explicitly is recommended for clarity.
    /// </para>
    /// <para>
    /// <strong>Disposal Behavior:</strong>
    /// </para>
    /// <para>
    /// The wrapper automatically calls <see cref="Return"/> when disposed. After disposal, the wrapped object
    /// may be retrieved and used by other callers, so you must not continue using it.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><strong>Example 1: Using statement (block scope)</strong></para>
    /// <code>
    /// using (var pooled = pool.GetPooled())
    /// {
    ///     pooled.Instance.DoSomething();
    ///     ProcessData(pooled.Instance);
    /// }  // Object automatically returned here
    /// </code>
    ///
    /// <para><strong>Example 2: Using declaration (method scope)</strong></para>
    /// <code>
    /// using var pooled = pool.GetPooled();
    /// pooled.Instance.DoSomething();
    /// // Object automatically returned at end of method
    /// </code>
    ///
    /// <para><strong>Example 3: Implicit conversion</strong></para>
    /// <code>
    /// using var pooled = pool.GetPooled();
    /// ProcessData(pooled);  // Automatically converts to T
    /// </code>
    ///
    /// <para><strong>Example 4: Exception safety</strong></para>
    /// <code>
    /// using var pooled = pool.GetPooled();
    /// pooled.Instance.DoRiskyOperation();  // Even if this throws, object is returned
    /// </code>
    /// </example>
    public PooledObject GetPooled() => new(this, Get());

    /// <summary>
    /// Provides a disposable wrapper that automatically returns an object to the pool when disposed.
    /// </summary>
    public readonly struct PooledObject : IDisposable
    {
        private readonly ObjectPool<T> _pool;

        internal PooledObject(ObjectPool<T> pool, T instance)
        {
            _pool = pool;
            Instance = instance;
        }

        /// <summary>
        /// Gets the pooled instance wrapped by this disposable object.
        /// </summary>
        /// <value>The object retrieved from the pool.</value>
        public T Instance { get; }

        /// <summary>
        /// Returns the pooled object back to the pool for reuse.
        /// </summary>
        public void Dispose()
        {
            _pool.Return(Instance);
        }

        /// <summary>
        /// Implicitly converts a <see cref="PooledObject"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <param name="pooledObject">The pooled object wrapper to convert.</param>
        /// <returns>The underlying pooled instance of type <typeparamref name="T"/>.</returns>
        public static implicit operator T(PooledObject pooledObject)
            => pooledObject.Instance;
    }
}
