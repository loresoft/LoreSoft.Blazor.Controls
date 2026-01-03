using System.ComponentModel;

using LoreSoft.Blazor.Controls.Extensions;
using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// A component that displays a skeleton placeholder for loading content.
/// Supports customizable width, height, and shape.
/// </summary>
public class Skeleton : ComponentBase
{
    /// <summary>
    /// Gets or sets the CSS width of the skeleton placeholder.
    /// </summary>
    [Parameter]
    public string? Width { set; get; }

    /// <summary>
    /// Gets or sets the CSS height of the skeleton placeholder.
    /// </summary>
    [Parameter]
    public string? Height { set; get; }

    /// <summary>
    /// Gets or sets the type of skeleton shape to display. Default is <see cref="SkeletonType.Text"/>.
    /// </summary>
    [Parameter]
    public SkeletonType Type { set; get; } = SkeletonType.Text;

    /// <summary>
    /// Gets or sets additional attributes to be applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the computed CSS class for the skeleton element.
    /// </summary>
    protected string? ClassName { get; set; }

    /// <summary>
    /// Gets the computed CSS style for the skeleton element.
    /// </summary>
    protected string? Style { get; set; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        var type = Type.ToString().ToLowerInvariant();

        // update only after parameters are set
        ClassName = CssBuilder.Pool.Use(builder => builder
            .AddClass("skeleton")
            .AddClass("skeleton-wave")
            .AddClass($"skeleton-{type}")
            .MergeClass(AdditionalAttributes)
            .ToString()
        );

        Style = StyleBuilder.Pool.Use(builder => builder
            .MergeStyle(AdditionalAttributes)
            .AddStyle("width", Width, (v) => v.HasValue())
            .AddStyle("height", Height, (v) => v.HasValue())
            .ToString()
        );
    }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", ClassName);
        builder.AddAttribute(2, "style", Style);
        builder.AddMultipleAttributes(3, AdditionalAttributes);
        builder.CloseElement(); // span
    }
}

/// <summary>
/// Specifies the shape of the skeleton placeholder.
/// </summary>
public enum SkeletonType
{
    /// <summary>
    /// Displays a text-like skeleton.
    /// </summary>
    [Description("text")]
    Text,
    /// <summary>
    /// Displays a circular skeleton.
    /// </summary>
    [Description("circle")]
    Circle,
    /// <summary>
    /// Displays a rectangular skeleton.
    /// </summary>
    [Description("rectangle")]
    Rectangle
}
