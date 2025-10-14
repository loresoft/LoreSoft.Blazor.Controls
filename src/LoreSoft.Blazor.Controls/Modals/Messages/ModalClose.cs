
namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Message indicating that a modal should be closed.
/// Used with the Messenger service for modal lifecycle management.
/// </summary>
/// <param name="Modal">The reference to the modal instance to close.</param>
public record ModalClose(ModalReference Modal);
