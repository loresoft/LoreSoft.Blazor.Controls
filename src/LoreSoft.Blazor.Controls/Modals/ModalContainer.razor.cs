using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

public partial class ModalContainer : ComponentBase, IDisposable
{
    private readonly Messenger _messenger;

    public ModalContainer(Messenger messenger)
    {
        _messenger = messenger;
        _messenger.Subscribe<ModalShow>(this, ShowModal);
        _messenger.Subscribe<ModalClose>(this, CloseModal);
    }


    protected List<ModalReference> Modals { get; } = [];


    private async Task ShowModal(ModalShow message)
    {
        if (message is null || message.Modal is null)
            return;

        Modals.Add(message.Modal);
        await InvokeAsync(StateHasChanged);
    }

    private async Task CloseModal(ModalClose close)
    {
        if (close is null || close.Modal is null)
            return;

        Modals.Remove(close.Modal);
        await InvokeAsync(StateHasChanged);
    }


    public void Dispose()
    {
        _messenger.Unsubscribe(this);
        GC.SuppressFinalize(this);
    }
}
