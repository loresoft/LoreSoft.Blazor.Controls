using System.ComponentModel;

using LoreSoft.Blazor.Controls.Extensions;
using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

public class Skeleton : ComponentBase
{
    [Parameter]
    public string Width { set; get; }

    [Parameter]
    public string Height { set; get; }

    [Parameter]
    public SkeletonType Type { set; get; } = SkeletonType.Text;

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> Attributes { get; set; } = [];

    protected string ClassName { get; set; }

    protected string Style { get; set; }

    protected override void OnParametersSet()
    {
        var type = Type.ToString().ToLowerInvariant();

        // update only after parameters are set
        ClassName = new CssBuilder("skeleton")
            .AddClass("skeleton-wave")
            .AddClass($"skeleton-{type}")
            .MergeClass(Attributes)
            .ToString();

        Style = new StyleBuilder()
            .MergeStyle(Attributes)
            .AddStyle("width", Width, (v) => v.HasValue())
            .AddStyle("height", Height, (v) => v.HasValue())
            .ToString();
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", ClassName);
        builder.AddAttribute(2, "style", Style);
        builder.AddMultipleAttributes(3, Attributes);
        builder.CloseElement(); // span
    }
}

public enum SkeletonType
{
    [Description("text")]
    Text,
    [Description("circle")]
    Circle,
    [Description("rectangle")]
    Rectangle
}
