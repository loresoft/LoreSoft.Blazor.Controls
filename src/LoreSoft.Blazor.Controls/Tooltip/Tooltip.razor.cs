// Ignore Spelling: Tooltip

using LoreSoft.Blazor.Controls.Abstracts;
using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

public partial class Tooltip : StandardComponent
{
    [Parameter, EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    [Parameter]
    public RenderFragment? TipContent { get; set; }

    [Parameter]
    public string? Tip { get; set; }

    [Parameter]
    public TooltipPosition Position { get; set; } = TooltipPosition.Top;

    [Parameter]
    public TooltipVariant? Variant { get; set; }


    protected override string? ElementClass => "tooltip-container";

    protected override CssBuilder ComputeClasses(CssBuilder cssBuilder)
    {
        return cssBuilder
            .AddClass(PositionClass)
            .AddClass(VariantClass);
    }

    private string PositionClass => Position switch
    {
        TooltipPosition.Top => "tooltip-top",
        TooltipPosition.Bottom => "tooltip-bottom",
        TooltipPosition.Left => "tooltip-left",
        TooltipPosition.Right => "tooltip-right",
        TooltipPosition.TopLeft => "tooltip-top-left",
        TooltipPosition.TopRight => "tooltip-top-right",
        TooltipPosition.BottomLeft => "tooltip-bottom-left",
        TooltipPosition.BottomRight => "tooltip-bottom-right",
        _ => "tooltip-top"
    };

    private string? VariantClass => Variant switch
    {
        TooltipVariant.Success => "tooltip-success",
        TooltipVariant.Danger => "tooltip-danger",
        TooltipVariant.Warning => "tooltip-warning",
        TooltipVariant.Information => "tooltip-info",
        _ => null
    };
}
