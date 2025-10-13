using System.Collections.Concurrent;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// A lightweight in-process message service that uses weak references to prevent memory leaks.
/// </summary>
/// <remarks>
/// The <see cref="Messenger"/> class provides a publish-subscribe pattern for loosely coupled communication
/// between components in an application. It uses weak references to subscribers to prevent memory leaks
/// when subscribers are no longer needed. Register as a singleton in your DI container for application-wide messaging.
/// </remarks>
public class Messenger : IDisposable
{
    private readonly ConcurrentDictionary<Type, ConcurrentBag<WeakSubscription>> _subscriptions = new();

#if NET9_0_OR_GREATER
    private readonly Lock _cleanupLock = new();
#else
    private readonly object _cleanupLock = new();
#endif

    private bool _disposed;
    private int _cleanupRunning = 0;

    /// <summary>
    /// Subscribes to messages of type <typeparamref name="TMessage"/>.
    /// </summary>
    /// <typeparam name="TMessage">The message type to subscribe to.</typeparam>
    /// <param name="subscriber">The subscriber object used for weak reference tracking. When this object is garbage collected, the subscription will be automatically removed.</param>
    /// <param name="handler">The async handler function to invoke when a message of type <typeparamref name="TMessage"/> is published.</param>
    /// <returns>An <see cref="IDisposable"/> that when disposed will unsubscribe the handler from receiving further messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="subscriber"/> or <paramref name="handler"/> is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the messenger has been disposed.</exception>
    /// <remarks>
    /// The subscription uses a weak reference to the subscriber object, allowing it to be garbage collected
    /// without explicitly unsubscribing. However, it's recommended to dispose the returned subscription when done
    /// to immediately free resources.
    /// </remarks>
    public IDisposable Subscribe<TMessage>(object subscriber, Func<TMessage, Task> handler)
    {
        ArgumentNullException.ThrowIfNull(subscriber);
        ArgumentNullException.ThrowIfNull(handler);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var messageType = typeof(TMessage);
        var subscription = new WeakSubscription(subscriber, async (msg) => await handler((TMessage)msg));

        var bag = _subscriptions.GetOrAdd(messageType, _ => []);
        bag.Add(subscription);

        // start a cleanup task to remove dead subscriptions
        ScheduleCleanup(messageType);

        // return a disposable to remove the subscription
        return new Subscription(() => RemoveSubscription(messageType, subscription));
    }

    /// <summary>
    /// Unsubscribes all message handlers for the specified subscriber.
    /// </summary>
    /// <param name="subscriber">The subscriber object to unsubscribe from all message types.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="subscriber"/> is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the messenger has been disposed.</exception>
    /// <remarks>
    /// This method removes all subscriptions associated with the specified subscriber object across all message types.
    /// This is useful when a component or service needs to clean up all its subscriptions at once.
    /// </remarks>
    public void Unsubscribe(object subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_cleanupLock)
        {
            var messageTypesToClean = new List<Type>();

            foreach (var kvp in _subscriptions)
            {
                var messageType = kvp.Key;
                var subscriptions = kvp.Value;

                // Filter out subscriptions that match the subscriber
                var remainingSubscriptions = subscriptions
                    .Where(s => !s.IsSubscriber(subscriber) && s.IsAlive)
                    .ToArray();

                if (remainingSubscriptions.Length == 0)
                {
                    // No subscriptions left for this message type
                    messageTypesToClean.Add(messageType);
                }
                else if (remainingSubscriptions.Length != subscriptions.Count)
                {
                    // Some subscriptions were removed, update the bag
                    _subscriptions[messageType] = [.. remainingSubscriptions];
                }
            }

            // Remove message types with no subscriptions
            foreach (var messageType in messageTypesToClean)
            {
                _subscriptions.TryRemove(messageType, out _);
            }
        }
    }

    /// <summary>
    /// Publishes a message to all active subscribers of type <typeparamref name="TMessage"/>.
    /// </summary>
    /// <typeparam name="TMessage">The message type to publish.</typeparam>
    /// <param name="message">The message instance to publish to all subscribers.</param>
    /// <returns>A <see cref="Task"/> that completes when all subscriber handlers have completed execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the messenger has been disposed.</exception>
    /// <remarks>
    /// All active subscriber handlers are invoked concurrently. The method awaits completion of all handlers
    /// before returning. If a handler throws an exception, it is caught and logged to the console, but does not
    /// prevent other handlers from executing. Dead subscriptions (where the subscriber has been garbage collected)
    /// are automatically skipped and cleaned up.
    /// </remarks>
    public async Task PublishAsync<TMessage>(TMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var messageType = typeof(TMessage);

        if (!_subscriptions.TryGetValue(messageType, out var subscriptions))
            return;

        var tasks = new List<Task>();

        foreach (var subscription in subscriptions)
        {
            if (!subscription.IsAlive)
                continue;

            try
            {
                tasks.Add(subscription.Handler(message));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in message handler: {ex.Message}");
            }
        }

        // Await all handlers to complete
        await Task.WhenAll(tasks);

        // start a cleanup task to remove dead subscriptions
        ScheduleCleanup(messageType);
    }

    /// <summary>
    /// Disposes the messenger and clears all subscriptions.
    /// </summary>
    /// <remarks>
    /// After disposal, any attempts to subscribe or publish messages will throw an <see cref="ObjectDisposedException"/>.
    /// This method is safe to call multiple times.
    /// </remarks>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        lock (_cleanupLock)
            _subscriptions.Clear();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Removes a specific subscription and cleans up any dead subscriptions for the given message type.
    /// </summary>
    /// <param name="messageType">The type of message to remove the subscription from.</param>
    /// <param name="subscriptionToRemove">The specific subscription instance to remove.</param>
    private void RemoveSubscription(Type messageType, WeakSubscription subscriptionToRemove)
    {
        if (!_subscriptions.TryGetValue(messageType, out var subscriptions))
            return;

        lock (_cleanupLock)
        {
            // Remove the specific subscription and any dead ones
            var remainingSubscriptions = subscriptions
                .Where(s => s != subscriptionToRemove && s.IsAlive)
                .ToArray();

            if (remainingSubscriptions.Length == 0)
                _subscriptions.TryRemove(messageType, out _);
            else
                _subscriptions[messageType] = [.. remainingSubscriptions];
        }
    }

    /// <summary>
    /// Schedules a background cleanup task to remove dead subscriptions for the specified message type.
    /// </summary>
    /// <param name="messageType">The type of message to clean up subscriptions for.</param>
    /// <remarks>
    /// Uses an interlocked flag to ensure only one cleanup runs at a time. The cleanup runs asynchronously
    /// and does not block the caller.
    /// </remarks>
    private void ScheduleCleanup(Type messageType)
    {
        // Use Interlocked to ensure only one cleanup runs at a time per message type
        if (Interlocked.CompareExchange(ref _cleanupRunning, 1, 0) != 0)
            return;

        // Fire and forget - don't block the caller
        _ = Task.Run(() =>
        {
            try
            {
                CleanupSubscriptions(messageType);
            }
            finally
            {
                Interlocked.Exchange(ref _cleanupRunning, 0);
            }
        });
    }

    /// <summary>
    /// Removes all dead subscriptions for the specified message type.
    /// </summary>
    /// <param name="messageType">The type of message to clean up subscriptions for.</param>
    /// <remarks>
    /// If all subscriptions are dead, the entire message type entry is removed from the dictionary.
    /// </remarks>
    private void CleanupSubscriptions(Type messageType)
    {
        if (!_subscriptions.TryGetValue(messageType, out var subscriptions))
            return;

        lock (_cleanupLock)
        {
            var aliveSubscriptions = subscriptions.Where(s => s.IsAlive).ToArray();

            if (aliveSubscriptions.Length == 0)
                _subscriptions.TryRemove(messageType, out _);
            else if (aliveSubscriptions.Length != subscriptions.Count)
                _subscriptions[messageType] = [.. aliveSubscriptions];
        }
    }

    /// <summary>
    /// Represents a subscription that holds a weak reference to the subscriber object.
    /// </summary>
    /// <remarks>
    /// This class allows subscribers to be garbage collected without explicit unsubscription,
    /// preventing memory leaks in long-lived messenger instances.
    /// </remarks>
    private class WeakSubscription(object subscriber, Func<object, Task> handler)
    {
        private readonly WeakReference _subscriberRef = new(subscriber);

        /// <summary>
        /// Gets the handler function to invoke when a message is published.
        /// </summary>
        public Func<object, Task> Handler { get; } = handler;

        /// <summary>
        /// Gets a value indicating whether the subscriber object is still alive (not garbage collected).
        /// </summary>
        public bool IsAlive => _subscriberRef.IsAlive;

        /// <summary>
        /// Determines whether this subscription belongs to the specified subscriber.
        /// </summary>
        /// <param name="subscriber">The subscriber object to check.</param>
        /// <returns><c>true</c> if this subscription belongs to the specified subscriber; otherwise, <c>false</c>.</returns>
        public bool IsSubscriber(object subscriber)
        {
            if (!_subscriberRef.IsAlive)
                return false;

            var target = _subscriberRef.Target;
            return target != null && ReferenceEquals(target, subscriber);
        }
    }

    /// <summary>
    /// Represents a disposable subscription token that unsubscribes when disposed.
    /// </summary>
    private class Subscription(Action onDispose) : IDisposable
    {
        private readonly Action _onDispose = onDispose;
        private bool _disposed;

        /// <summary>
        /// Disposes the subscription and invokes the unsubscribe action.
        /// </summary>
        /// <remarks>
        /// This method is safe to call multiple times.
        /// </remarks>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _onDispose?.Invoke();
        }
    }
}
