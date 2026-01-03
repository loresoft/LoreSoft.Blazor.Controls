using System.Linq.Expressions;

using LoreSoft.Blazor.Controls.Extensions;
using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// A toggle switch component for boolean values, supporting two-way binding and validation.
/// </summary>
/// <typeparam name="TValue">The value type, must be <c>bool</c> or <c>bool?</c>.</typeparam>
public partial class ToggleSwitch<TValue> : ComponentBase
{
    /// <summary>
    /// Static constructor to validate the supported type.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if <typeparamref name="TValue"/> is not <c>bool</c> or <c>bool?</c>.</exception>
    static ToggleSwitch()
    {
        var targetType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
        if (targetType != typeof(bool))
            throw new InvalidOperationException($"The type '{targetType}' is not supported by ToggleSwitch.");
    }

    /// <summary>
    /// Additional attributes to be applied to the input element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> Attributes { get; set; } = [];

    /// <summary>
    /// Gets or sets the current value of the toggle switch.
    /// </summary>
    [Parameter]
    public TValue? Value { get; set; }

    /// <summary>
    /// Event callback triggered when the value changes.
    /// </summary>
    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the value expression for validation.
    /// </summary>
    [Parameter]
    public Expression<Func<TValue>>? ValueExpression { get; set; }

    /// <summary>
    /// The <see cref="EditContext"/> for validation, provided via cascading parameter.
    /// </summary>
    [CascadingParameter]
    public EditContext? EditContext { get; set; }

    /// <summary>
    /// The field identifier for validation.
    /// </summary>
    [Parameter]
    public FieldIdentifier FieldIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the current value of the toggle switch, and notifies value changes and validation.
    /// </summary>
    protected TValue? CurrentValue
    {
        get => Value;
        set
        {
            var isEqual = EqualityComparer<TValue>.Default.Equals(value, Value);

            if (isEqual)
                return;

            Value = value;
            ValueChanged.InvokeAsync(Value);
            EditContext?.NotifyFieldChanged(FieldIdentifier);
        }
    }

    /// <summary>
    /// Initializes the component and sets the field identifier if not already set.
    /// </summary>
    protected override void OnInitialized()
    {
        if (FieldIdentifier.Equals(default) && ValueExpression != null)
            FieldIdentifier = FieldIdentifier.Create(ValueExpression);
    }

    /// <summary>
    /// Builds the render tree for the toggle switch component.
    /// </summary>
    /// <param name="builder">The <see cref="RenderTreeBuilder"/> used to build the component's render tree.</param>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var fieldClass = EditContext != null
            ? EditContext.FieldCssClass(FieldIdentifier)
            : string.Empty;

        builder.OpenElement(0, "label");
        builder.AddAttribute(1, "class", "toggle-switch");

        builder.OpenElement(2, "input");
        builder.AddMultipleAttributes(3, Attributes);
        builder.AddAttribute(4, "type", "checkbox");
        builder.AddAttribute(5, "checked", BindConverter.FormatValue(CurrentValue));
        builder.AddAttribute(6, "onchange", EventCallback.Factory.CreateBinder(this, v => CurrentValue = v, CurrentValue));
        builder.SetUpdatesAttributeName("checked");
        builder.CloseElement(); // input

        var sliderClass = CssBuilder.Pool.Use(b =>
        {
            return b
                .AddClass("toggle-slider")
                .AddClass(fieldClass, fieldClass.HasValue())
                .ToString();
        });

        builder.OpenElement(7, "span");
        builder.AddAttribute(8, "class", sliderClass);
        builder.CloseComponent(); // slider

        builder.CloseElement(); //label
    }
}
