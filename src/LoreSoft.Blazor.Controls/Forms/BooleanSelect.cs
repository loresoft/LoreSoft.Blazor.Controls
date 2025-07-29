using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// An input select component for boolean values, supporting <c>bool</c> and <c>bool?</c> types.
/// Renders options for true, false, and optionally null.
/// </summary>
/// <typeparam name="TValue">The value type, must be <c>bool</c> or <c>bool?</c>.</typeparam>
public class BooleanSelect<TValue> : InputSelect<TValue>
{
    /// <summary>
    /// The label to display for the null option (only shown for <c>bool?</c>). Defaults to "- select -".
    /// </summary>
    [Parameter]
    public string NullLabel { get; set; } = "- select -";

    /// <summary>
    /// The label to display for the true option. Defaults to "True".
    /// </summary>
    [Parameter]
    public string TrueLabel { get; set; } = "True";

    /// <summary>
    /// The label to display for the false option. Defaults to "False".
    /// </summary>
    [Parameter]
    public string FalseLabel { get; set; } = "False";

    /// <summary>
    /// Called when component parameters are set. Ensures only <c>bool</c> or <c>bool?</c> are allowed and sets the default child content.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if <typeparamref name="TValue"/> is not <c>bool</c> or <c>bool?</c>.</exception>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (typeof(TValue) != typeof(bool) && typeof(TValue) != typeof(bool?))
            throw new InvalidOperationException($"{nameof(BooleanSelect<TValue>)} only supports bool or bool? types.");

        ChildContent ??= RenderBooleanOptions;
    }

    /// <summary>
    /// Converts the value to a string for rendering in the select element.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>
    /// "true" or "false" for <c>bool</c> and <c>bool?</c> values, <c>null</c> for a null <c>bool?</c>, or the base implementation for other types.
    /// </returns>
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

    /// <summary>
    /// Renders the boolean options for the select element.
    /// Includes a null option for <c>bool?</c> types.
    /// </summary>
    /// <param name="builder">The <see cref="RenderTreeBuilder"/> used to build the options.</param>
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
