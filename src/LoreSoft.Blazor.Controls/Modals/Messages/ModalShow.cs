namespace LoreSoft.Blazor.Controls;


/// <summary>
/// Message indicating that a modal should be shown.
/// Used with the EventBus service for modal lifecycle management.
/// </summary>
/// <param name="Modal">The reference to the modal instance to show.</param>
public record ModalShow(ModalReference Modal);
