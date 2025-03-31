using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using LoreSoft.Blazor.Controls.Extensions;
using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

public partial class ToggleSwitch<TValue> : ComponentBase
{
    static ToggleSwitch()
    {
        var targetType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
        if (targetType != typeof(bool))
            throw new InvalidOperationException($"The type '{targetType}' is not supported by ToggleSwitch.");
    }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> Attributes { get; set; } = [];

    [Parameter]
    public TValue Value { get; set; }

    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    [Parameter]
    public Expression<Func<TValue>> ValueExpression { get; set; }

    [CascadingParameter]
    public EditContext EditContext { get; set; }

    [Parameter]
    public FieldIdentifier FieldIdentifier { get; set; }


    protected bool CurrentValue
    {
        get => ConvertToBoolean(Value);
        set
        {
            TValue current = ConvertFromBoolean(value);
            var isEqual = EqualityComparer<TValue>.Default.Equals(current, Value);

            if (isEqual)
                return;

            Value = current;
            ValueChanged.InvokeAsync(current);
            EditContext?.NotifyFieldChanged(FieldIdentifier);
        }
    }


    protected override void OnInitialized()
    {
        if (FieldIdentifier.Equals(default))
            FieldIdentifier = FieldIdentifier.Create(ValueExpression);
    }

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

        var sliderClass = CssBuilder.Default("toggle-slider")
            .AddClass(fieldClass, fieldClass.HasValue())
            .ToString();

        builder.OpenElement(7, "span");
        builder.AddAttribute(8, "class", sliderClass);
        builder.CloseComponent(); // slider

        builder.CloseElement(); //label
    }


    private bool ConvertToBoolean(TValue value)
    {
        if (value is bool boolValue)
            return boolValue;

        return false;
    }

    private TValue ConvertFromBoolean(bool value)
    {
        var targetType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);

        if (targetType == typeof(bool))
            return (TValue)(object)value;

        return default;
    }

}
