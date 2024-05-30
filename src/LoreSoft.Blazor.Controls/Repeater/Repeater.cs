#nullable enable

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

public class Repeater<TItem> : ComponentBase
{
    [Parameter]
    public IEnumerable<TItem>? Items { get; set; }

    [Parameter]
    public RenderFragment<TItem>? ChildContent { get; set; }

    [Parameter]
    public RenderFragment? Empty { get; set; }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (ChildContent != null && Items?.Any() == true)
        {
            foreach (var item in Items)
            {
                builder.AddContent(0, ChildContent, item);
            }
        }
        else if (Empty != null)
        {
            builder.AddContent(1, Empty);
        }
    }
}
