using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

public class BusyButton : ComponentBase
{
    [Parameter]
    public bool Busy { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public RenderFragment BusyTemplate { get; set; }

    [Parameter]
    public RenderFragment ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "button");
        builder.AddMultipleAttributes(1, Attributes);
        builder.AddAttribute(2, "disabled", Disabled || Busy);

        if (Busy)
        {
            builder.AddContent(3, BusyTemplate);
        }
        else
        {
            builder.AddContent(3, ChildContent);
        }

        builder.CloseElement(); // button
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        BusyTemplate ??= builder => builder.AddContent(0, "Busy...");
    }
}
