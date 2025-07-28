using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

public class BooleanSelect<TValue> : InputSelect<TValue>
{
    [Parameter]
    public string NullLabel { get; set; } = "- select -";

    [Parameter]
    public string TrueLabel { get; set; } = "True";

    [Parameter]
    public string FalseLabel { get; set; } = "False";


    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (typeof(TValue) != typeof(bool) && typeof(TValue) != typeof(bool?))
            throw new InvalidOperationException($"{nameof(BooleanSelect<TValue>)} only supports bool or bool? types.");

        ChildContent ??= RenderBooleanOptions;
    }

    protected override string? FormatValueAsString(TValue? value)
    {
        if (typeof(TValue) == typeof(bool))
        {
            if (value is bool b)
                return b ? "true" : "false";

            return "false";
        }
        else if (typeof(TValue) == typeof(bool?))
        {
            if (value is null)
                return null;

            if (value is bool b)
                return b ? "true" : "false";

            return null;
        }

        return base.FormatValueAsString(value);
    }

    protected void RenderBooleanOptions(RenderTreeBuilder builder)
    {
        if (typeof(TValue) == typeof(bool?))
        {
            builder.OpenElement(0, "option");
            builder.AddAttribute(1, "value", "");
            builder.AddContent(2, NullLabel);
            builder.CloseElement();
        }

        builder.OpenElement(3, "option");
        builder.AddAttribute(4, "value", "true");
        builder.AddContent(5, TrueLabel);
        builder.CloseElement();

        builder.OpenElement(6, "option");
        builder.AddAttribute(7, "value", "false");
        builder.AddContent(8, FalseLabel);
        builder.CloseElement();
    }

}
