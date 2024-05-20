using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace LoreSoft.Blazor.Controls;

public class BusyButton : ComponentBase
{
    [Parameter]
    public bool Busy { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string BusyText { get; set; } = "Processing";

    [Parameter]
    public RenderFragment BusyTemplate { get; set; }

    [Parameter]
    public RenderFragment ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> Attributes { get; set; } = [];

    [Parameter]
    public EventCallback Trigger { get; set; }

    private bool Executing { get; set; }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "button");
        builder.AddMultipleAttributes(1, Attributes);
        builder.AddAttribute(2, "disabled", Disabled || IsBusy);

        if (Trigger.HasDelegate)
        {
            builder.AddAttribute(3, "type", "button");
            builder.AddAttribute(4, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, ExecuteTrigger));
        }

        if (IsBusy)
        {
            builder.AddContent(5, BusyTemplate);
        }
        else
        {
            builder.AddContent(6, ChildContent);
        }

        builder.CloseElement(); // button
    }


    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

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

    protected bool IsBusy => Busy || Executing;

    private async Task ExecuteTrigger()
    {
        if (!Trigger.HasDelegate)
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
