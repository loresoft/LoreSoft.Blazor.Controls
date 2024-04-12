using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

public class Conditional : ComponentBase
{
    [Parameter]
    public bool Condition { get; set; }

    [Parameter]
    public RenderFragment ChildContent { get; set; }

    [Parameter]
    public RenderFragment Passed { get; set; }

    [Parameter]
    public RenderFragment Failed { get; set; }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Condition)
        {
            if (Passed != null)
            {
                builder.AddContent(1, Passed);
            }
            else if (ChildContent != null)
            {
                builder.AddContent(2, ChildContent);
            }
        }
        else if (Failed != null)
        {
            builder.AddContent(3, Failed);
        }
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (ChildContent != null && Passed != null)
        {
            throw new InvalidOperationException($"Do not specify both '{nameof(Passed)}' and '{nameof(ChildContent)}'.");
        }
    }
}
