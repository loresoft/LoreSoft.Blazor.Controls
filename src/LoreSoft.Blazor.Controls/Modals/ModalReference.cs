namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents a reference to a modal dialog instance, managing its lifecycle and result.
/// </summary>
/// <remarks>
/// This class implements <see cref="IModalReference"/> and provides the coordination between
/// the modal dialog component, the messenger service, and the code that initiated the modal.
/// It automatically injects itself as a parameter to the modal component.
/// </remarks>
public class ModalReference : IModalReference
{
    private readonly Messenger _messenger;
    private readonly TaskCompletionSource<ModalResult> _resultCompletion;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModalReference"/> class.
    /// </summary>
    /// <param name="messenger">The messenger service used to publish modal close messages.</param>
    /// <param name="componentType">The type of the component to render in the modal dialog.</param>
    /// <param name="parameters">The parameters to pass to the modal component.</param>
    /// <param name="id">The unique identifier for the modal. If <c>null</c>, a random identifier is generated.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="messenger"/>, <paramref name="componentType"/>, or <paramref name="parameters"/> is null.
    /// </exception>
    /// <remarks>
    /// The constructor automatically adds this <see cref="ModalReference"/> instance to the parameters dictionary
    /// under the key <see cref="ModalComponentBase.Modal"/>, making it available to the modal component.
    /// </remarks>
    public ModalReference(
        Messenger messenger,
        Type componentType,
        Dictionary<string, object> parameters,
        string? id = null)
    {
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));

        ComponentType = componentType ?? throw new ArgumentNullException(nameof(componentType));
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));

        ModalId = id ?? Identifier.Random();

        _resultCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);

        // Ensure the Modal parameter is set for the component
        Parameters[nameof(ModalComponentBase.Modal)] = this;
    }

    /// <summary>
    /// Gets the unique identifier for the modal dialog.
    /// </summary>
    /// <value>The modal identifier string.</value>
    public string ModalId { get; }

    /// <summary>
    /// Gets the type of the component to render in the modal dialog.
    /// </summary>
    /// <value>The component <see cref="Type"/>.</value>
    public Type ComponentType { get; }

    /// <summary>
    /// Gets the parameters to pass to the modal component.
    /// </summary>
    /// <value>A dictionary of parameter names and values.</value>
    public Dictionary<string, object> Parameters { get; }

    /// <summary>
    /// Gets a task that completes when the modal dialog is closed, providing the result of the dialog.
    /// </summary>
    /// <value>A task that returns the <see cref="ModalResult"/> when the modal is closed.</value>
    public Task<ModalResult> Result
        => _resultCompletion.Task;

    /// <summary>
    /// Cancels the modal dialog asynchronously with a cancelled result.
    /// </summary>
    /// <returns>A task that represents the asynchronous cancel operation.</returns>
    public Task CancelAsync()
        => CloseAsync(ModalResult.Cancel());

    /// <summary>
    /// Closes the modal dialog asynchronously with the specified result.
    /// </summary>
    /// <param name="modalResult">The result to return when closing the modal.</param>
    /// <returns>A task that represents the asynchronous close operation.</returns>
    /// <remarks>
    /// This method publishes a <see cref="ModalClose"/> message via the messenger service to notify
    /// the modal dialog to close, then completes the <see cref="Result"/> task with the provided result.
    /// </remarks>
    public async Task CloseAsync(ModalResult modalResult)
    {
        // signal others to close the modal
        var message = new ModalClose(this);
        await _messenger.PublishAsync(message);

        // complete the result task
        Complete(modalResult);
    }

    /// <summary>
    /// Completes the result task with the specified modal result.
    /// </summary>
    /// <param name="result">The modal result to set.</param>
    /// <remarks>
    /// This method is called internally to complete the <see cref="Result"/> task.
    /// It uses <see cref="TaskCompletionSource{TResult}.TrySetResult"/> to safely handle multiple completion attempts.
    /// </remarks>
    internal void Complete(ModalResult result)
    {
        _resultCompletion.TrySetResult(result);
    }
}

