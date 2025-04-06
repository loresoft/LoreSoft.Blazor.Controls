using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

[CascadingTypeParameter(nameof(TItem))]
public partial class DataList<TItem> : DataComponentBase<TItem>
{
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    [Parameter, EditorRequired]
    public required RenderFragment<TItem> RowTemplate { get; set; }

    [Parameter]
    public RenderFragment? HeaderTemplate { get; set; }

    [Parameter]
    public RenderFragment? FooterTemplate { get; set; }


    protected string? ClassName { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        ClassName = new CssBuilder("data-list")
                    .MergeClass(AdditionalAttributes)
                    .ToString();
    }
}
