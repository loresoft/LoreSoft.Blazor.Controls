using LoreSoft.Blazor.Controls.Extensions;
using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Displays a loading indicator and overlays content while an operation is in progress.
/// </summary>
public class LoadingBlock : ComponentBase
{
    /// <summary>
    /// Additional attributes to be splatted onto the root div element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Indicates whether the loading indicator should be displayed.
    /// </summary>
    [Parameter]
    public bool IsLoading { get; set; }

    /// <summary>
    /// If <see langword="true"/>, displays the loading indicator as an overlay above the child content.
    /// If <see langword="false"/>, the loading indicator will hide the child content.
    /// </summary>
    [Parameter]
    public bool ShowOverlay { get; set; }

    /// <summary>
    /// If true, displays a spinner in the loading indicator.
    /// </summary>
    [Parameter]
    public bool ShowSpinner { get; set; } = true;

    /// <summary>
    /// Optional text to display with the loading indicator.
    /// </summary>
    [Parameter]
    public string? LoadingText { get; set; }

    /// <summary>
    /// Custom template to display while loading.
    /// </summary>
    [Parameter]
    public RenderFragment? LoadingTemplate { get; set; }

    /// <summary>
    /// The content to display inside the loading block.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The CSS class name for the root element. Computed based on AdditionalAttributes and default class.
    /// </summary>
    protected string? ClassName { get; set; }

    /// <summary>
    /// Builds the render tree for the loading block component.
    /// </summary>
    /// <param name="builder">The <see cref="RenderTreeBuilder"/> used to build the component's render tree.</param>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttribute(2, "class", ClassName);

        if (ChildContent != null)
            builder.AddAttribute(3, "style", "position:relative;");

        // Render the child content if provided and not loading or if ShowOverlay is true
        if (ChildContent != null && (!IsLoading || ShowOverlay))
        {
            builder.AddContent(4, ChildContent);
        }

        // Render loading overlay if IsLoading is true
        if (IsLoading)
        {
            builder.OpenElement(5, "div");
            builder.AddAttribute(6, "class", ShowOverlay ? "loading-mode-overlay" : "loading-mode-block");

            if (ShowSpinner)
            {
                builder.OpenElement(7, "div");
                builder.AddAttribute(8, "class", "loading-block-spinner");

                builder.OpenElement(9, "div");
                builder.AddAttribute(10, "class", "loading-block-spinner-icon");
                builder.CloseElement(); // div (spinner-icon)

                builder.CloseElement(); // div (spinner)
            }

            if (LoadingText.HasValue())
            {
                builder.OpenElement(11, "div");
                builder.AddAttribute(12, "class", "loading-block-text");

                builder.AddContent(13, LoadingText);

                builder.CloseElement(); // div (loading-block-text)
            }

            if (LoadingTemplate != null)
            {
                builder.AddContent(14, LoadingTemplate);
            }

            builder.CloseElement(); // div (overlay/block)
        }

        builder.CloseElement(); // div (root)
    }

    /// <summary>
    /// Sets parameters and updates the CSS class name for the component.
    /// </summary>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        ClassName = CssBuilder
            .Default("loading-block")
            .MergeClass(AdditionalAttributes)
            .ToString();
    }
}
