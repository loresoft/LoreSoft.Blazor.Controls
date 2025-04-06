using LoreSoft.Blazor.Controls.Extensions;
using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

public class LoadingBlock : ComponentBase
{
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public bool ShowSpinner { get; set; } = true;

    [Parameter]
    public string? LoadingText { get; set; }

    [Parameter]
    public RenderFragment? ChildTemplate { get; set; }

    protected string? ClassName { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!IsLoading)
            return;

        builder.OpenElement(0, "div");
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttribute(2, "class", ClassName);

        if (ShowSpinner)
        {
            builder.OpenElement(3, "div");
            builder.AddAttribute(4, "class", "loading-block-spinner");

            builder.OpenElement(5, "div");
            builder.AddAttribute(6, "class", "loading-block-spinner-icon");
            builder.CloseElement(); // div

            builder.CloseElement(); // div
        }

        if (LoadingText.HasValue())
        {
            builder.OpenElement(7, "div");
            builder.AddAttribute(8, "class", "loading-block-text");

            builder.OpenElement(9, "h4");
            builder.AddContent(10, LoadingText);
            builder.CloseElement(); // h4

            builder.CloseElement(); // div
        }

        if (ChildTemplate != null)
        {
            builder.AddContent(11, ChildTemplate);
        }

        builder.CloseElement(); // div

    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        ClassName = CssBuilder
            .Default("loading-block-overlay")
            .MergeClass(AdditionalAttributes)
            .ToString();
    }
}
