using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// A button component that displays a busy indicator and disables itself while an operation is in progress.
/// </summary>
public class BusyButton : ComponentBase
{
    /// <summary>
    /// Indicates whether the button is busy. When <c>true</c>, the busy indicator is shown and the button is disabled.
    /// </summary>
    [Parameter]
    public bool Busy { get; set; }

    /// <summary>
    /// Indicates whether the button is disabled. When <c>true</c>, the button cannot be clicked.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// The text to display when the button is busy. This text is shown in the busy template if no custom template is provided. Defaults to "Processing".
    /// </summary>
    [Parameter]
    public string BusyText { get; set; } = "Processing";

    /// <summary>
    /// Custom template to display when the button is busy. If not set, a default template is used.
    /// </summary>
    [Parameter]
    public RenderFragment? BusyTemplate { get; set; }

    /// <summary>
    /// The content to display inside the button when it is not busy.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional attributes to be applied to the button element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = [];

    /// <summary>
    /// The event callback to trigger when the button is clicked. Automatically sets the button to busy while executing.
    /// </summary>
    [Parameter]
    public EventCallback Trigger { get; set; }

    /// <summary>
    /// Indicates whether the button is currently executing the trigger callback.
    /// </summary>
    private bool Executing { get; set; }

    /// <summary>
    /// The CSS class name for the button element. Computed based on AdditionalAttributes and default class.
    /// </summary>
    protected string? ClassName { get; set; }

    /// <summary>
    /// Builds the render tree for the BusyButton component.
    /// </summary>
    /// <param name="builder">The <see cref="RenderTreeBuilder"/> used to build the component's render tree.</param>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "button");
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttribute(2, "class", ClassName);
        builder.AddAttribute(3, "disabled", Disabled || IsBusy);

        if (Trigger.HasDelegate)
        {
            builder.AddAttribute(4, "type", "button");
            builder.AddAttribute(5, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, ExecuteTrigger));
        }

        if (IsBusy)
        {
            builder.AddContent(6, BusyTemplate);
        }
        else
        {
            builder.AddContent(7, ChildContent);
        }

        builder.CloseElement(); // button
    }

    /// <summary>
    /// Sets parameters and updates the CSS class name and default busy template.
    /// </summary>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        ClassName = new CssBuilder("busy-button")
            .MergeClass(AdditionalAttributes)
            .ToString();

        BusyTemplate ??= builder => {
            builder.AddContent(0, BusyText);

            builder.OpenElement(1, "span");
            builder.AddAttribute(2, "aria-hidden", "true");
            builder.AddAttribute(3, "class", "busy-loading-indicator");

            builder.OpenElement(4, "span");
            builder.AddAttribute(5, "class", "busy-loading-dot-1");
            builder.CloseElement();

            builder.OpenElement(6, "span");
            builder.AddAttribute(7, "class", "busy-loading-dot-2");
            builder.CloseElement();

            builder.OpenElement(8, "span");
            builder.AddAttribute(9, "class", "busy-loading-dot-3");
            builder.CloseElement();

            builder.CloseElement(); //span
        };
    }

    /// <summary>
    /// Gets a value indicating whether the button is busy, either by the <see cref="Busy"/> parameter or while executing the trigger.
    /// </summary>
    protected bool IsBusy => Busy || Executing;

    /// <summary>
    /// Executes the <see cref="Trigger"/> callback and manages the busy state.
    /// </summary>
    private async Task ExecuteTrigger()
    {
        if (!Trigger.HasDelegate || Executing)
            return;

        try
        {
            Executing = true;
            await Trigger.InvokeAsync();
        }
        finally
        {
            Executing = false;
        }
    }
}
