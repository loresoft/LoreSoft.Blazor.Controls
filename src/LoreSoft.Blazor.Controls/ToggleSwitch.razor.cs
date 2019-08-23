using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LoreSoft.Blazor.Controls
{
    public class ToggleSwitchBase : ComponentBase
    {
        [CascadingParameter]
        private EditContext CascadedEditContext { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        [Parameter]
        public bool Value { get; set; }

        [Parameter]
        public EventCallback<bool> ValueChanged { get; set; }

        [Parameter]
        public Expression<Func<bool>> ValueExpression { get; set; }

        public EditContext EditContext { get; set; }

        public FieldIdentifier FieldIdentifier { get; set; }

        public bool CurrentValue
        {
            get => Value;
            set
            {
                if (value == Value) 
                    return;

                Value = value;
                ValueChanged.InvokeAsync(value);
                EditContext?.NotifyFieldChanged(FieldIdentifier);
            }
        }

        public string CssClass
        {
            get
            {
                var fieldClass = EditContext != null
                    ? EditContext.FieldClass(FieldIdentifier)
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
            EditContext = CascadedEditContext;
            FieldIdentifier = FieldIdentifier.Create(ValueExpression);
        }
    }
}
