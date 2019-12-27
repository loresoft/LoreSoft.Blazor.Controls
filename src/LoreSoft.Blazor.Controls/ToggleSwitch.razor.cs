using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LoreSoft.Blazor.Controls
{
    public class ToggleSwitchBase<TValue> : ComponentBase
    {
        static ToggleSwitchBase()
        {
            var targetType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
            if (targetType != typeof(bool))
                throw new InvalidOperationException($"The type '{targetType}' is not supported by ToggleSwitch.");
        }

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

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


        public bool CurrentValue
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

        public string CssClass
        {
            get
            {
                var fieldClass = EditContext != null
                    ? EditContext.FieldCssClass(FieldIdentifier)
                    : string.Empty;

                if (AdditionalAttributes != null &&
                    AdditionalAttributes.TryGetValue("class", out var cssClass) &&
                    !string.IsNullOrEmpty(Convert.ToString(cssClass)))
                {
                    return $"toggle-switch {cssClass} {fieldClass}";
                }

                return $"toggle-switch {fieldClass}";
            }
        }


        protected override void OnInitialized()
        {
            if (FieldIdentifier.Equals(default))
                FieldIdentifier = FieldIdentifier.Create(ValueExpression);
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

}
