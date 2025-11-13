using LoreSoft.Blazor.Controls.Events;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls.Abstracts;

/// <summary>
/// Base component class that integrates with the <see cref="Events.EventBus"/> for event-driven communication.
/// Extends <see cref="StandardComponent"/> to provide automatic subscription and unsubscription to the event bus.
/// </summary>
/// <remarks>
/// <para>
/// This component automatically subscribes to the event bus during initialization and unsubscribes
/// during disposal, preventing memory leaks and ensuring proper cleanup.
/// </para>
/// <para>
/// Derived classes should override the <see cref="Subscribe"/> method to register their event handlers
/// with the event bus using the appropriate event types.
/// </para>
/// </remarks>
public class EventBusComponent : StandardComponent
{
    /// <summary>
    /// Gets or sets the event bus instance used for publishing and subscribing to events.
    /// This property is automatically injected by the dependency injection container.
    /// </summary>
    /// <remarks>
    /// The event bus provides a centralized publish-subscribe mechanism for loosely coupled
    /// communication between components using the weak event pattern.
    /// </remarks>
    [Inject]
    public required EventBus EventBus { get; set; }


    /// <summary>
    /// Called when the component is initialized.
    /// Automatically invokes the <see cref="Subscribe"/> method to register event handlers.
    /// </summary>
    /// <remarks>
    /// This ensures that event subscriptions are established before the component is first rendered.
    /// </remarks>
    protected override void OnInitialized()
    {
        Subscribe(EventBus);
        base.OnInitialized();
    }

    /// <summary>
    /// Override this method in derived classes to subscribe to specific events from the event bus.
    /// </summary>
    /// <param name="eventBus">The event bus instance to subscribe to.</param>
    /// <remarks>
    /// <para>
    /// Use the <paramref name="eventBus"/> parameter to call <c>Subscribe&lt;TEvent&gt;</c> methods
    /// for each event type the component needs to handle.
    /// </para>
    /// <para>
    /// Event handlers are automatically stored as weak references by the event bus, but explicit
    /// unsubscription is still performed during component disposal for immediate cleanup.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// protected override void Subscribe(EventBus eventBus)
    /// {
    ///     eventBus.Subscribe&lt;MyEvent&gt;(HandleMyEventAsync);
    ///     eventBus.Subscribe&lt;AnotherEvent&gt;(HandleAnotherEventAsync);
    /// }
    /// </code>
    /// </example>
    protected virtual void Subscribe(EventBus eventBus)
    {

    }

    /// <summary>
    /// Disposes managed resources by unsubscribing all event handlers from the event bus.
    /// </summary>
    /// <remarks>
    /// This method is automatically called during component disposal and ensures that all
    /// event subscriptions are cleaned up immediately rather than waiting for garbage collection.
    /// </remarks>
    protected override void DisposeManagedResources()
    {
        base.DisposeManagedResources();
        EventBus.Unsubscribe(this);
    }
}
