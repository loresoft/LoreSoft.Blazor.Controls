namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents a reference to a modal dialog instance.
/// </summary>
public interface IModalReference
{
    /// <summary>
    /// Gets the unique identifier for the modal dialog.
    /// </summary>
    /// <value>The modal identifier.</value>
    string ModalId { get; }

    /// <summary>
    /// Gets a task that completes when the modal dialog is closed, providing the result of the dialog.
    /// </summary>
    /// <value>A task that returns the <see cref="ModalResult"/> when the modal is closed.</value>
    Task<ModalResult> Result { get; }

    /// <summary>
    /// Cancels the modal dialog asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous cancel operation.</returns>
    Task CancelAsync();

    /// <summary>
    /// Closes the modal dialog asynchronously with the specified result.
    /// </summary>
    /// <param name="modalResult">The result to return when closing the modal.</param>
    /// <returns>A task that represents the asynchronous close operation.</returns>
    Task CloseAsync(ModalResult modalResult);
}

