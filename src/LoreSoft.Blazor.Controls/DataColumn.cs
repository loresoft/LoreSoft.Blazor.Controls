using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls
{
    public class DataColumn<TItem> : ComponentBase
    {
        private Func<TItem, object> _propertyAccessor;
        private string _propertyName;
        private int? _sortIndex;
        private bool? _sortDescending;

        [CascadingParameter(Name = "Grid")]
        protected DataGrid<TItem> Grid { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> Attributes { get; set; }

        [Parameter]
        public Expression<Func<TItem, object>> Property { get; set; }

        [Parameter]
        public string Title { get; set; }

        [Parameter]
        public string Width { get; set; }

        [Parameter]
        public string Format { get; set; }

        [Parameter]
        public string ClassName { get; set; }

        [Parameter]
        public Func<TItem, string> Style { get; set; }

        [Parameter]
        public string HeaderClass { get; set; }

        [Parameter]
        public string FooterClass { get; set; }


        [Parameter]
        public bool Sortable { get; set; } = true;

        [Parameter]
        public int SortIndex
        {
            get => _sortIndex ?? -1;
            set => SetInitialValue(ref _sortIndex, value);
        }

        [Parameter]
        public bool SortDescending
        {
            get => _sortDescending ?? false;
            set => SetInitialValue(ref _sortDescending, value);
        }


        [Parameter]
        public bool Visible { get; set; } = true;

        public string Name => PropertyName();


        [Parameter]
        public RenderFragment HeaderTemplate { get; set; }

        [Parameter]
        public RenderFragment<TItem> Template { get; set; }

        [Parameter]
        public RenderFragment FooterTemplate { get; set; }


        protected override void OnInitialized()
        {
            if (Grid == null)
                throw new InvalidOperationException("DataColumn must be child of DataGrid");

            if (Property == null)
                throw new InvalidOperationException("DataColumn Property parameter is required");

            // register with parent grid
            Grid.AddColumn(this);
        }

        protected void OnChange()
        {
            InvokeAsync(StateHasChanged);
            Grid?.RefreshAsync();
        }

        // parameter properties with internal changes can only be set once
        protected void SetInitialValue<T>(ref T field, T value)
        {
            if (field != null)
                return;

            field = value;
            OnChange();
        }


        internal string HeaderTitle()
        {
            if (!string.IsNullOrEmpty(Title))
                return Title;

            var name = PropertyName();
            return ToTitle(name);
        }

        internal string CellValue(TItem data)
        {
            if (data == null || Property == null)
                return string.Empty;

            _propertyAccessor ??= Property.Compile();

            object value = null;

            try
            {
                value = _propertyAccessor.Invoke(data);
            }
            catch (NullReferenceException)
            {

            }

            if (value == null)
                return string.Empty;

            return string.IsNullOrEmpty(Format)
                ? value.ToString()
                : string.Format(CultureInfo.CurrentCulture, $"{{0:{Format}}}", value);
        }

        internal void UpdateSort(int index, bool descending)
        {
            _sortIndex = index;
            _sortDescending = descending;
        }

        private string PropertyName()
        {
            if (Property == null)
                return string.Empty;

            if (!string.IsNullOrEmpty(_propertyName))
                return _propertyName;

            _propertyName = Property?.Body switch
            {
                MemberExpression memberExpression => memberExpression.Member.Name,
                UnaryExpression { Operand: MemberExpression memberOperand } => memberOperand.Member.Name,
                _ => string.Empty
            };

            return _propertyName;
        }

        private string ToTitle(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var words = Regex.Matches(value, @"([A-Z][a-z]*)|([0-9]+)");

            var spacedName = new StringBuilder();
            foreach (Match word in words)
            {
                if (spacedName.Length > 0)
                    spacedName.Append(' ');

                spacedName.Append(word.Value);
            }

            return spacedName.ToString();
        }
    }
}
