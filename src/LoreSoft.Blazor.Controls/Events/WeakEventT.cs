namespace LoreSoft.Blazor.Controls.Events;

/// <summary>
/// Represents a weak event that supports a single typed parameter with automatic cleanup of garbage collected subscribers.
/// </summary>
/// <typeparam name="TEvent">The type of the event data passed to handlers.</typeparam>
/// <remarks>
/// This class provides a type-safe wrapper around <see cref="WeakEventBase"/> for events that pass a single parameter.
/// Handlers can be synchronous actions, asynchronous functions, or asynchronous functions that support cancellation.
/// </remarks>
public sealed class WeakEvent<TEvent> : WeakEventBase
{
    /// <summary>
    /// Subscribes a synchronous action as an event handler.
    /// </summary>
    /// <param name="handler">The synchronous action that accepts <typeparamref name="TEvent"/> to subscribe.</param>
    /// <remarks>
    /// The handler is stored as a weak reference, allowing the target object to be garbage collected
    /// without explicitly unsubscribing.
    /// </remarks>
    public void Subscribe(Action<TEvent> handler)
        => SubscribeCore(handler);

    /// <summary>
    /// Subscribes an asynchronous function as an event handler.
    /// </summary>
    /// <param name="handler">The asynchronous function that accepts <typeparamref name="TEvent"/> to subscribe.</param>
    /// <remarks>
    /// The handler is stored as a weak reference, allowing the target object to be garbage collected
    /// without explicitly unsubscribing.
    /// </remarks>
    public void Subscribe(Func<TEvent, ValueTask> handler)
        => SubscribeCore(handler);

    /// <summary>
    /// Subscribes an asynchronous function that supports cancellation as an event handler.
    /// </summary>
    /// <param name="handler">The asynchronous function that accepts <typeparamref name="TEvent"/> and a <see cref="CancellationToken"/> to subscribe.</param>
    /// <remarks>
    /// The handler is stored as a weak reference, allowing the target object to be garbage collected
    /// without explicitly unsubscribing. The cancellation token passed to <see cref="PublishAsync"/> will be
    /// forwarded to this handler when invoked.
    /// </remarks>
    public void Subscribe(Func<TEvent, CancellationToken, ValueTask> handler)
        => SubscribeCore(handler);

    /// <summary>
    /// Unsubscribes a synchronous action from the event.
    /// </summary>
    /// <param name="handler">The synchronous action to unsubscribe.</param>
    /// <returns>The remaining number of handlers after removal.</returns>
    public int Unsubscribe(Action<TEvent> handler)
        => UnsubscribeCore(handler);

    /// <summary>
    /// Unsubscribes an asynchronous function from the event.
    /// </summary>
    /// <param name="handler">The asynchronous function to unsubscribe.</param>
    /// <returns>The remaining number of handlers after removal.</returns>
    public int Unsubscribe(Func<TEvent, ValueTask> handler)
        => UnsubscribeCore(handler);

    /// <summary>
    /// Unsubscribes an asynchronous function that supports cancellation from the event.
    /// </summary>
    /// <param name="handler">The asynchronous function that accepts a <see cref="CancellationToken"/> to unsubscribe.</param>
    /// <returns>The remaining number of handlers after removal.</returns>
    public int Unsubscribe(Func<TEvent, CancellationToken, ValueTask> handler)
        => UnsubscribeCore(handler);

    /// <summary>
    /// Publishes the event to all active subscribers by invoking their handlers asynchronously and concurrently.
    /// </summary>
    /// <param name="eventData">The event data of type <typeparamref name="TEvent"/> to pass to all handlers.</param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the operation.
    /// This token is passed to handlers that support cancellation.
    /// </param>
    /// <returns>A <see cref="ValueTask"/> that completes when all handlers have been invoked.</returns>
    /// <remarks>
    /// <para>
    /// Dead handlers (whose targets have been garbage collected) are automatically removed before invocation.
    /// </para>
    /// <para>
    /// All handlers are invoked concurrently, and this method awaits all of them to complete.
    /// </para>
    /// </remarks>
    public ValueTask PublishAsync(TEvent eventData, CancellationToken cancellationToken = default)
        => PublishCoreAsync(eventData, cancellationToken);
}
