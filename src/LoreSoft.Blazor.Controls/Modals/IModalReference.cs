#nullable enable

namespace LoreSoft.Blazor.Controls;

public interface IModalReference
{
    string ModalId { get; }

    Task<ModalResult> Result { get; }

    Task CancelAsync();
    Task CloseAsync(ModalResult modalResult);
}

