using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

public class ModalService(Messenger messenger)
{
    private readonly Messenger _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));

    public async Task<IModalReference> Show<TComponent>(Dictionary<string, object>? parameters = null)
        where TComponent : ComponentBase
    {
        var modalReference = new ModalReference(_messenger, typeof(TComponent), parameters);

        var message = new ModalShow(modalReference);
        await _messenger.PublishAsync(message);

        return modalReference;
    }
}
