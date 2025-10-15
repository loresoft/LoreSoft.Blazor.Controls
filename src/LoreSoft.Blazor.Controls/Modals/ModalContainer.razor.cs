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
    private readonly Messenger _messenger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModalContainer"/> class.
    /// </summary>
    /// <param name="messenger">The messenger service used to subscribe to modal show and close messages.</param>
    public ModalContainer(Messenger messenger)
    {
        _messenger = messenger;
        _messenger.Subscribe<ModalShow>(this, HandleModalShow);
        _messenger.Subscribe<ModalClose>(this, HandleModalClose);
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
    private async Task HandleModalShow(ModalShow message)
    {
        if (message is null || message.Modal is null)
            return;

        Modals.Add(message.Modal);
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Handles the <see cref="ModalClose"/> message by removing a modal from the active collection.
    /// </summary>
    /// <param name="message">The modal close message containing the modal reference to remove.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Includes a 250ms delay to allow closing animations to complete before removing the modal from the DOM.
    /// </remarks>
    private async Task HandleModalClose(ModalClose message)
    {
        if (message is null || message.Modal is null)
            return;

        // Allow time for closing animations
        await Task.Delay(250);

        Modals.Remove(message.Modal);
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Disposes the component and unsubscribes from all messenger subscriptions.
    /// </summary>
    public void Dispose()
    {
        _messenger.Unsubscribe(this);
        GC.SuppressFinalize(this);
    }
}
