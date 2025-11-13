using System.Collections.Concurrent;

namespace LoreSoft.Blazor.Controls.Events;

/// <summary>
/// Provides a centralized event bus for publishing and subscribing to typed events using the weak event pattern.
/// </summary>
/// <remarks>
/// <para>
/// This class implements a publish-subscribe pattern where subscribers are held with weak references,
/// allowing them to be garbage collected without explicitly unsubscribing.
/// </para>
/// <para>
/// Events are dispatched based on their type, and all handlers for a given event type are invoked
/// concurrently when the event is published. Empty subscriptions (with no remaining handlers) are
/// automatically removed from the event bus.
/// </para>
/// <para>
/// All operations are thread-safe and use concurrent collections internally.
/// </para>
/// </remarks>
public class EventBus
{
    private readonly ConcurrentDictionary<Type, WeakEventBase> _subscriptions = new();

    /// <summary>
    /// Subscribes an asynchronous event handler for events of type <typeparamref name="TEvent"/>.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to subscribe to.</typeparam>
    /// <param name="handler">The asynchronous function that accepts <typeparamref name="TEvent"/> to subscribe. Cannot be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// The handler is stored as a weak reference, allowing the target object to be garbage collected
    /// without explicitly unsubscribing.
    /// </remarks>
    public void Subscribe<TEvent>(Func<TEvent, ValueTask> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var messageType = typeof(TEvent);

        var subscription = _subscriptions.GetOrAdd(messageType, _ => new WeakEvent<TEvent>()) as WeakEvent<TEvent>;
        subscription?.Subscribe(handler);
    }

    /// <summary>
    /// Subscribes an asynchronous event handler that supports cancellation for events of type <typeparamref name="TEvent"/>.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to subscribe to.</typeparam>
    /// <param name="handler">The asynchronous function that accepts <typeparamref name="TEvent"/> and a <see cref="CancellationToken"/> to subscribe. Cannot be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// The handler is stored as a weak reference, allowing the target object to be garbage collected
    /// without explicitly unsubscribing. The cancellation token passed to <see cref="PublishAsync{TEvent}"/> will be
    /// forwarded to this handler when invoked.
    /// </remarks>
    public void Subscribe<TEvent>(Func<TEvent, CancellationToken, ValueTask> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var messageType = typeof(TEvent);

        var subscription = _subscriptions.GetOrAdd(messageType, _ => new WeakEvent<TEvent>()) as WeakEvent<TEvent>;
        subscription?.Subscribe(handler);
    }

    /// <summary>
    /// Unsubscribes an asynchronous event handler from events of type <typeparamref name="TEvent"/>.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to unsubscribe from.</typeparam>
    /// <param name="handler">The asynchronous function to unsubscribe. Cannot be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// If this was the last handler for the event type, the subscription is removed from the event bus.
    /// </remarks>
    public void Unsubscribe<TEvent>(Func<TEvent, ValueTask> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var messageType = typeof(TEvent);
        if (_subscriptions.GetOrAdd(messageType, _ => new WeakEvent<TEvent>()) is not WeakEvent<TEvent> subscription)
            return;

        var remaining = subscription.Unsubscribe(handler);
        if (remaining > 0)
            return;

        _subscriptions.TryRemove(messageType, out _);
    }

    /// <summary>
    /// Unsubscribes an asynchronous event handler that supports cancellation from events of type <typeparamref name="TEvent"/>.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to unsubscribe from.</typeparam>
    /// <param name="handler">The asynchronous function that accepts a <see cref="CancellationToken"/> to unsubscribe. Cannot be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// If this was the last handler for the event type, the subscription is removed from the event bus.
    /// </remarks>
    public void Unsubscribe<TEvent>(Func<TEvent, CancellationToken, ValueTask> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var messageType = typeof(TEvent);
        if (_subscriptions.GetOrAdd(messageType, _ => new WeakEvent<TEvent>()) is not WeakEvent<TEvent> subscription)
            return;

        var remaining = subscription.Unsubscribe(handler);
        if (remaining > 0)
            return;

        _subscriptions.TryRemove(messageType, out _);
    }

    /// <summary>
    /// Unsubscribes all event handlers associated with the specified subscriber object from all event types. This is typically called when the subscriber is being disposed.
    /// </summary>
    /// <param name="subscriber">The subscriber object whose handlers should be removed. Cannot be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="subscriber"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method removes all handlers across all event types that belong to the specified subscriber.
    /// Empty subscriptions (with no remaining handlers) are automatically removed from the event bus.
    /// </remarks>
    public void Unsubscribe(object subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        List<Type>? emptySubscriptions = null;
        foreach (var pair in _subscriptions)
        {
            var subscriberType = pair.Key;
            var subscription = pair.Value;

            var remaining = subscription.Unsubscribe(subscriber);
            if (remaining > 0)
                continue;

            emptySubscriptions ??= [];
            emptySubscriptions.Add(subscriberType);
        }

        if (emptySubscriptions == null || emptySubscriptions.Count == 0)
            return;

        foreach (var type in emptySubscriptions)
            _subscriptions.TryRemove(type, out _);
    }

    /// <summary>
    /// Publishes an event to all active subscribers by invoking their handlers asynchronously and concurrently.
    /// </summary>
    /// <typeparam name="TEvent">The type of event being published.</typeparam>
    /// <param name="eventData">The event data to pass to all handlers. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the operation. 
    /// This token is passed to handlers that support cancellation.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask"/> that completes when all handlers have been invoked.
    /// If no handlers are subscribed to this event type, the task completes immediately.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventData"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// Dead handlers (whose targets have been garbage collected) are automatically removed before invocation.
    /// </para>
    /// <para>
    /// All handlers are invoked concurrently, and this method awaits all of them to complete.
    /// </para>
    /// </remarks>
    public async ValueTask PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        var messageType = typeof(TEvent);

        if (!_subscriptions.TryGetValue(messageType, out var subscription))
            return;

        if (subscription is not WeakEvent<TEvent> weakEvent)
            return;

        await weakEvent.PublishAsync(eventData, cancellationToken).ConfigureAwait(false);
    }
}
