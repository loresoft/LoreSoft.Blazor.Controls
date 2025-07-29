#nullable enable

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// A component that repeats a template for each item in a collection.
/// Supports custom content for each item and an optional template for empty collections.
/// </summary>
/// <typeparam name="TItem">The type of the data item.</typeparam>
public class Repeater<TItem> : ComponentBase
{
    /// <summary>
    /// Gets or sets the collection of items to render.
    /// </summary>
    [Parameter]
    public IEnumerable<TItem>? Items { get; set; }

    /// <summary>
    /// Gets or sets the template to render for each item in <see cref="Items"/>.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem>? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the template to render when <see cref="Items"/> is null or empty.
    /// </summary>
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
