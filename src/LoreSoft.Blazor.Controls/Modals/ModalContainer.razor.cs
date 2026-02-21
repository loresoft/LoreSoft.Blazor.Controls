using LoreSoft.Blazor.Controls.Events;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Container component that manages and renders active modal dialogs in the application.
/// </summary>
/// <remarks>
/// This component subscribes to <see cref="ModalShow"/> and <see cref="ModalClose"/> messages
/// to dynamically add and remove modals from the UI. It should be placed once at the application root level.
/// </remarks>
public partial class ModalContainer : ComponentBase, IDisposable
{
    private readonly EventBus _eventBus;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModalContainer"/> class.
    /// </summary>
    /// <param name="eventBus">The event bus service used to subscribe to modal show and close messages.</param>
    public ModalContainer(EventBus eventBus)
    {
        _eventBus = eventBus;
        _eventBus.Subscribe<ModalShow>(HandleModalShow);
        _eventBus.Subscribe<ModalClose>(HandleModalClose);
    }

    /// <summary>
    /// Gets the collection of currently active modal dialogs.
    /// </summary>
    /// <value>A list of <see cref="ModalReference"/> instances representing active modals.</value>
    protected List<ModalReference> Modals { get; } = [];

    /// <summary>
    /// Handles the <see cref="ModalShow"/> message by adding a modal to the active collection.
    /// </summary>
    /// <param name="message">The modal show message containing the modal reference to display.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async ValueTask HandleModalShow(ModalShow message)
    {
        if (message is null || message.Modal is null)
            return;

        Modals.Add(message.Modal);
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Handles the <see cref="ModalClose"/> message by scheduling removal of a modal from the active collection.
    /// </summary>
    /// <param name="message">The modal close message containing the modal reference to remove.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Schedules a 250ms delayed removal to allow closing animations to complete before removing the modal from the DOM.
    /// The caller is not blocked by the delay.
    /// </remarks>
    private ValueTask HandleModalClose(ModalClose message)
    {
        if (message is null || message.Modal is null)
            return ValueTask.CompletedTask;

        // Schedule modal removal after a delay to allow closing animations to complete
        _ = RemoveModalAsync(message.Modal);

        // Return immediately to avoid blocking the caller with the delay
        return ValueTask.CompletedTask;
    }

    private async Task RemoveModalAsync(ModalReference modal)
    {
        // Allow time for closing animations
        await Task.Delay(250);

        Modals.Remove(modal);
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Disposes the component and unsubscribes from all eventbus subscriptions.
    /// </summary>
    public void Dispose()
    {
        _eventBus.Unsubscribe(this);
        GC.SuppressFinalize(this);
    }
}
