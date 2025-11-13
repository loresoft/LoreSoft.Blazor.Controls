using System.Buffers;

namespace LoreSoft.Blazor.Controls.Events;

/// <summary>
/// Provides a base implementation for weak event patterns that automatically clean up garbage collected subscribers.
/// </summary>
/// <remarks>
/// <para>
/// This class uses weak references to store event handlers, allowing subscribers to be garbage collected
/// without explicitly unsubscribing. This prevents common memory leaks associated with event handlers.
/// </para>
/// <para>
/// Thread-safe operations are ensured through a lock object. All public and protected methods
/// can be safely called from multiple threads concurrently.
/// </para>
/// <para>
/// Handlers are invoked asynchronously and concurrently. Dead handlers (whose targets have been garbage collected)
/// are automatically cleaned up during event publication.
/// </para>
/// </remarks>
public class WeakEventBase
{
    private readonly List<WeakDelegate> _handlers = [];

#if NET9_0_OR_GREATER
    private readonly Lock _handlerLock = new();
#else
    private readonly object _handlerLock = new();
#endif

    /// <summary>
    /// Gets the count of active subscribers that have not been garbage collected.
    /// </summary>
    /// <returns>The number of alive subscribers.</returns>
    /// <remarks>
    /// This method acquires a lock to ensure thread-safe counting. The count reflects the number
    /// of handlers whose target objects are still alive at the moment of the call.
    /// </remarks>
    public int SubscriberCount()
    {
        lock (_handlerLock)
        {
            int count = 0;
            for (int i = 0; i < _handlers.Count; i++)
            {
                if (_handlers[i].IsAlive)
                    count++;
            }
            return count;
        }
    }

    /// <summary>
    /// Removes all event handlers associated with the specified subscriber object.
    /// </summary>
    /// <param name="subscriber">The subscriber object whose handlers should be removed. If <see langword="null"/>, no handlers are removed.</param>
    /// <returns>The remaining number of handlers after removal.</returns>
    /// <remarks>
    /// <para>
    /// This method is typically called when the subscriber is being disposed to ensure immediate cleanup
    /// rather than waiting for garbage collection.
    /// </para>
    /// <para>
    /// Uses reference equality to match handlers by their target object. All handlers with a matching target
    /// are removed in a single atomic operation.
    /// </para>
    /// </remarks>
    public int Unsubscribe(object subscriber)
    {
        if (subscriber is null)
            return _handlers.Count;

        lock (_handlerLock)
        {
            for (int i = _handlers.Count - 1; i >= 0; i--)
            {
                if (_handlers[i].IsTargetMatch(subscriber))
                    _handlers.RemoveAt(i);
            }

            return _handlers.Count;
        }
    }


    /// <summary>
    /// Subscribes a delegate as a weak event handler.
    /// </summary>
    /// <param name="handler">The delegate to subscribe. Cannot be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// The handler is stored as a weak reference, allowing the target object to be garbage collected
    /// without explicitly unsubscribing. This prevents memory leaks from forgotten event subscriptions.
    /// </para>
    /// <para>
    /// Duplicate subscriptions are not prevented - the same delegate can be subscribed multiple times
    /// and will be invoked once for each subscription.
    /// </para>
    /// </remarks>
    protected void SubscribeCore(Delegate handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (_handlerLock)
        {
            WeakDelegate weakDelegate = new(handler);
            _handlers.Add(weakDelegate);
        }
    }

    /// <summary>
    /// Unsubscribes a specific delegate from the event.
    /// </summary>
    /// <param name="handler">The delegate to unsubscribe. Cannot be <see langword="null"/>.</param>
    /// <returns>The remaining number of handlers after removal.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method removes all instances of the specified delegate. If the delegate was subscribed
    /// multiple times, all subscriptions are removed.
    /// </para>
    /// <para>
    /// The match is based on both the target object and the method. Two delegates with the same method
    /// but different targets are not considered equal.
    /// </para>
    /// </remarks>
    protected int UnsubscribeCore(Delegate handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (_handlerLock)
        {
            for (int i = _handlers.Count - 1; i >= 0; i--)
            {
                if (_handlers[i].IsDelegateMatch(handler))
                    _handlers.RemoveAt(i);
            }

            return _handlers.Count;
        }
    }

    /// <summary>
    /// Publishes an event to all active subscribers by invoking their handlers asynchronously and concurrently.
    /// </summary>
    /// <param name="eventData">The event data to pass to each event handler. Can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the operation.
    /// This token is passed to handlers that support cancellation in their method signature.
    /// </param>
    /// <returns>A <see cref="ValueTask"/> that completes when all handlers have been invoked.</returns>
    /// <remarks>
    /// <para>
    /// Dead handlers (whose targets have been garbage collected) are automatically removed before invocation.
    /// This cleanup happens in a thread-safe manner under lock.
    /// </para>
    /// <para>
    /// All handlers are invoked concurrently using fire-and-forget semantics with <see cref="ValueTask"/>.
    /// This method awaits all of them to complete before returning.
    /// </para>
    /// <para>
    /// Uses <see cref="ArrayPool{T}"/> to avoid allocating arrays for task tracking on each invocation,
    /// improving performance for frequently-fired events.
    /// </para>
    /// <para>
    /// If a handler throws an exception, it will propagate to the caller after all other handlers complete.
    /// The first exception encountered will be thrown.
    /// </para>
    /// </remarks>
    protected async ValueTask PublishCoreAsync(object? eventData, CancellationToken cancellationToken = default)
    {
        WeakDelegate[]? handlers = null;
        ValueTask[]? tasks = null;

        int activeCount = 0;

        try
        {
            // Capture alive handlers and compact list under lock
            lock (_handlerLock)
            {
                int currentCount = _handlers.Count;
                if (currentCount == 0)
                    return;

                handlers = ArrayPool<WeakDelegate>.Shared.Rent(currentCount);

                // Single-pass compaction: copy alive handlers and compact list
                int writeIndex = 0;
                for (int readIndex = 0; readIndex < currentCount; readIndex++)
                {
                    if (!_handlers[readIndex].IsAlive)
                        continue;

                    // Copy alive handler to rented array
                    handlers[activeCount++] = _handlers[readIndex];

                    // Move alive handler to front of list if needed
                    if (writeIndex != readIndex)
                        _handlers[writeIndex] = _handlers[readIndex];

                    writeIndex++;
                }

                // Remove dead handlers from end (single operation)
                if (writeIndex < currentCount)
                    _handlers.RemoveRange(writeIndex, currentCount - writeIndex);
            }

            if (activeCount == 0)
                return;

            tasks = ArrayPool<ValueTask>.Shared.Rent(activeCount);

            // Invoke all handlers concurrently
            for (int i = 0; i < activeCount; i++)
                tasks[i] = handlers[i].InvokeAsync(eventData, cancellationToken);

            // Await all handler invocations
            for (int i = 0; i < activeCount; i++)
                await tasks[i].ConfigureAwait(false);
        }
        finally
        {
            if (handlers != null)
                ArrayPool<WeakDelegate>.Shared.Return(handlers, clearArray: true);

            if (tasks != null)
                ArrayPool<ValueTask>.Shared.Return(tasks, clearArray: true);
        }
    }
}
