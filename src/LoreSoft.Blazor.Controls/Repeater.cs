using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

public class Repeater<TItem> : ComponentBase
{
    [Parameter, EditorRequired]
    public IEnumerable<TItem> Items { get; set; }

    [Parameter, EditorRequired]
    public RenderFragment<TItem> Row { get; set; }

    [Parameter]
    public RenderFragment Empty { get; set; }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Items?.Any() == true)
        {
            foreach (var item in Items)
            {
                builder.AddContent(1, Row, item);
                builder.SetKey(item);
            }
        }
        else if (Empty != null)
        {
            builder.AddContent(2, Empty);
        }
    }
}
