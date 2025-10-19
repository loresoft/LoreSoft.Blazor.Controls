using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Base class for modal dialog components, providing common functionality and parameters.
/// </summary>
public abstract class ModalComponentBase : ComponentBase
{
    /// <summary>
    /// Additional attributes to be applied to the button element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = [];

    /// <summary>
    /// Gets or sets the modal reference used to control the modal dialog.
    /// </summary>
    /// <value>The modal reference instance.</value>
    [Parameter, EditorRequired]
    public IModalReference Modal { get; set; } = null!;

    /// <summary>
    /// Gets or sets the title of the modal dialog.
    /// </summary>
    /// <value>The title text. Defaults to "Alert".</value>
    [Parameter]
    public string Title { get; set; } = "Alert";

    /// <summary>
    /// Gets or sets the message content of the modal dialog.
    /// </summary>
    /// <value>The message text. Defaults to an empty string.</value>
    [Parameter]
    public string Message { get; set; } = "";

    /// <summary>
    /// Gets or sets the text for the primary action button. The primary action typically represents a confirmation or acceptance action.
    /// </summary>
    /// <value>The primary action button text. Defaults to "OK".</value>
    [Parameter]
    public string PrimaryAction { get; set; } = "OK";

    /// <summary>
    /// Gets or sets the text for the secondary action button. The secondary action is typically used for canceling or dismissing the modal.
    /// </summary>
    /// <value>The secondary action button text. Defaults to "Cancel".</value>
    [Parameter]
    public string SecondaryAction { get; set; } = "Cancel";

    /// <summary>
    /// Gets or sets the visual variant of the modal dialog.
    /// </summary>
    /// <value>The modal variant. Defaults to <see cref="ModalVariant.Primary"/>.</value>
    [Parameter]
    public ModalVariant Variant { get; set; } = ModalVariant.Primary;

    /// <summary>
    /// Gets the CSS class name corresponding to the current variant.
    /// </summary>
    /// <value>The variant CSS class name.</value>
    protected string VariantClass { get; private set; } = "dialog-primary";

    /// <summary>
    /// Called when component parameters are set. Updates the variant CSS class based on the <see cref="Variant"/> parameter.
    /// </summary>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        VariantClass = Variant switch
        {
            ModalVariant.Success => "dialog-success",
            ModalVariant.Information => "dialog-information",
            ModalVariant.Warning => "dialog-warning",
            ModalVariant.Danger => "dialog-danger",
            ModalVariant.Primary => "dialog-primary",
            _ => "dialog-primary"
        };
    }

    /// <summary>
    /// Closes the modal dialog with a successful result.
    /// </summary>
    /// <param name="result">The result data to return.</param>
    /// <returns>A task that represents the asynchronous close operation.</returns>
    protected async Task CloseAsync(object? result = null)
    {
        // Ignore mouse event args which are passed when using @onclick
        if (result is MouseEventArgs)
            result = null;

        await Modal.CloseAsync(ModalResult.Success(result));
    }

    /// <summary>
    /// Cancels the modal dialog with a cancelled result.
    /// </summary>
    /// <returns>A task that represents the asynchronous cancel operation.</returns>
    protected async Task CancelAsync()
    {
        await Modal.CloseAsync(ModalResult.Cancel());
    }
}
