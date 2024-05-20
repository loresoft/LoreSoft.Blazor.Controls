using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

public class LoadingBlock : ComponentBase
{
    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public bool ShowSpinner { get; set; } = true;

    [Parameter]
    public string LoadingText { get; set; }

    [Parameter]
    public RenderFragment ChildTemplate { get; set; }


    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!IsLoading)
            return;

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "loading-block-overlay");

        if (ShowSpinner)
        {
            builder.OpenElement(2, "div");
            builder.AddAttribute(3, "class", "loading-block-spinner");

            builder.OpenElement(4, "div");
            builder.AddAttribute(5, "class", "loading-block-spinner-icon");
            builder.CloseElement(); // div

            builder.CloseElement(); // div
        }

        if (!string.IsNullOrEmpty(LoadingText))
        {
            builder.OpenElement(6, "div");
            builder.AddAttribute(7, "class", "loading-block-text");

            builder.OpenElement(8, "h4");
            builder.AddContent(9, LoadingText);
            builder.CloseElement(); // h4

            builder.CloseElement(); // div
        }

        if (ChildTemplate != null)
        {
            builder.AddContent(10, ChildTemplate);
        }

        builder.CloseElement(); // div

    }
}
