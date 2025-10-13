using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

public abstract class ModalComponentBase : ComponentBase
{
    [Parameter, EditorRequired]
    public IModalReference Modal { get; set; } = null!;

    [Parameter]
    public string Title { get; set; } = "Alert";

    [Parameter]
    public string Message { get; set; } = "";

    [Parameter]
    public string PrimaryAction { get; set; } = "OK";

    [Parameter]
    public string SecondaryAction { get; set; } = "Cancel";

    [Parameter]
    public ModalVariant Variant { get; set; } = ModalVariant.Primary;

    protected string VariantClass { get; private set; } = "dialog-primary";

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        VariantClass = Variant switch
        {
            ModalVariant.Success => "dialog-success",
            ModalVariant.Warning => "dialog-warning",
            ModalVariant.Danger => "dialog-danger",
            ModalVariant.Primary => "dialog-primary",
            _ => "dialog-primary"
        };
    }

    protected async Task Close(object? result = null)
    {
        await Modal.CloseAsync(ModalResult.Ok(result));
    }

    protected async Task Cancel()
    {
        await Modal.CloseAsync(ModalResult.Cancel());
    }
}
