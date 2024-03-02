using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

public class Repeater<TItem> : ComponentBase
{
    private int _sequence = 0;

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
                builder.AddContent(Next(), Row, item);
            }
        }
        else if (Empty != null)
        {
            builder.AddContent(Next(), Empty);
        }
    }

    protected int Next() => _sequence++;

}
