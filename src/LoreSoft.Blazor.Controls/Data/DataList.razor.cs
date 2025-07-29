using LoreSoft.Blazor.Controls.Utilities;
using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Displays a list of data items using a customizable row template.
/// Supports optional header and footer templates, and additional HTML attributes.
/// </summary>
/// <typeparam name="TItem">The type of the data item.</typeparam>
[CascadingTypeParameter(nameof(TItem))]
public partial class DataList<TItem> : DataComponentBase<TItem>
{
    /// <summary>
    /// Gets or sets additional attributes to be applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets the template used to render each row in the data list.
    /// </summary>
    [Parameter, EditorRequired]
    public required RenderFragment<TItem> RowTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template used to render the header of the data list.
    /// </summary>
    [Parameter]
    public RenderFragment? HeaderTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template used to render the footer of the data list.
    /// </summary>
    [Parameter]
    public RenderFragment? FooterTemplate { get; set; }

    /// <summary>
    /// Gets or sets the CSS class name for the root element. Computed from <see cref="AdditionalAttributes"/>.
    /// </summary>
    protected string? ClassName { get; set; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        ClassName = new CssBuilder("data-list")
                    .MergeClass(AdditionalAttributes)
                    .ToString();
    }
}
