using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Renders content conditionally based on a boolean value.
/// </summary>
public class Conditional : ComponentBase
{
    /// <summary>
    /// The condition to evaluate. If <c>true</c>, <see cref="Passed"/> or <see cref="ChildContent"/> is rendered; otherwise, <see cref="Failed"/> is rendered.
    /// </summary>
    [Parameter]
    public bool Condition { get; set; }

    /// <summary>
    /// The default content to render when <see cref="Condition"/> is <c>true</c> and <see cref="Passed"/> is not set.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The content to render when <see cref="Condition"/> is <c>true</c>. If set, <see cref="ChildContent"/> is ignored.
    /// </summary>
    [Parameter]
    public RenderFragment? Passed { get; set; }

    /// <summary>
    /// The content to render when <see cref="Condition"/> is <c>false</c>.
    /// </summary>
    [Parameter]
    public RenderFragment? Failed { get; set; }

    /// <summary>
    /// Builds the render tree for the <see cref="Conditional"/> component.
    /// </summary>
    /// <param name="builder">The <see cref="RenderTreeBuilder"/> used to build the component's render tree.</param>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Condition)
        {
            if (Passed != null)
            {
                builder.AddContent(0, Passed);
            }
            else if (ChildContent != null)
            {
                builder.AddContent(1, ChildContent);
            }
        }
        else if (Failed != null)
        {
            builder.AddContent(2, Failed);
        }
    }

    /// <summary>
    /// Called when component parameters are set. Throws if both <see cref="Passed"/> and <see cref="ChildContent"/> are specified.
    /// </summary>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (ChildContent != null && Passed != null)
        {
            throw new InvalidOperationException($"Do not specify both '{nameof(Passed)}' and '{nameof(ChildContent)}'.");
        }
    }
}
