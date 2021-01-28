using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace LoreSoft.Blazor.Controls
{
    public class TypeaheadBase<TItem, TValue> : ComponentBase, IDisposable
    {
        private Timer _debounceTimer;

        public TypeaheadBase()
        {
            Items = new List<TItem>();
            AllowClear = true;
            Loading = false;
            SearchMode = false;
            SelectedIndex = 0;
            SearchResults = new List<TItem>();
            SearchPlaceholder = "Search ...";
        }

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [CascadingParameter]
        protected EditContext EditContext { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        [Parameter]
        public TValue Value { get; set; }

        [Parameter]
        public EventCallback<TValue> ValueChanged { get; set; }

        [Parameter]
        public Expression<Func<TValue>> ValueExpression { get; set; }


        [Parameter]
        public IList<TValue> Values { get; set; }

        [Parameter]
        public EventCallback<IList<TValue>> ValuesChanged { get; set; }

        [Parameter]
        public Expression<Func<IList<TValue>>> ValuesExpression { get; set; }


        [Parameter]
        public string Placeholder { get; set; }

        [Parameter]
        public string SearchPlaceholder { get; set; }

        [Parameter]
        public IReadOnlyCollection<TItem> Items { get; set; }

        [Parameter]
        public Func<string, Task<IEnumerable<TItem>>> SearchMethod { get; set; }

        [Parameter]
        public Func<TItem, TValue> ConvertMethod { get; set; }


        [Parameter]
        public RenderFragment NoRecordsTemplate { get; set; }

        [Parameter]
        public RenderFragment LoadingTemplate { get; set; }

        [Parameter]
        public RenderFragment<TItem> ResultTemplate { get; set; }

        [Parameter]
        public RenderFragment<TValue> SelectedTemplate { get; set; }

        [Parameter]
        public RenderFragment FooterTemplate { get; set; }


        [Parameter]
        public int MinimumLength { get; set; } = 1;

        [Parameter]
        public int Debounce { get; set; } = 800;

        [Parameter]
        public bool AllowClear { get; set; }

        [Parameter]
        public bool Disabled { get; set; } = false;

        [Parameter]
        public FieldIdentifier FieldIdentifier { get; set; }


        public bool Loading { get; set; }

        public bool SearchMode { get; set; }

        public IList<TItem> SearchResults { get; set; }

        public ElementReference SearchInput { get; set; }

        public ElementReference TypeaheadControl;


        private string _searchText;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;

                if (value.Length == 0)
                {
                    _debounceTimer.Stop();
                    SearchResults.Clear();
                    SelectedIndex = -1;

                    ShowItems();
                }
                else if (value.Length >= MinimumLength)
                {
                    _debounceTimer.Stop();
                    _debounceTimer.Start();
                }
            }
        }

        public int SelectedIndex { get; set; }


        protected override void OnInitialized()
        {
            if (SearchMethod == null)
                throw new InvalidOperationException($"{GetType()} requires a {nameof(SearchMethod)} parameter.");

            if (ConvertMethod == null)
            {
                if (typeof(TItem) != typeof(TValue))
                    throw new InvalidOperationException($"{GetType()} requires a {nameof(ConvertMethod)} parameter.");

                ConvertMethod = item => item is TValue value ? value : default;
            }

            if (SelectedTemplate == null)
                SelectedTemplate = item => builder => builder.AddContent(0, item?.ToString());

            if (ResultTemplate == null)
                ResultTemplate = item => builder => builder.AddContent(0, item?.ToString());

            if (NoRecordsTemplate == null)
                NoRecordsTemplate = builder => builder.AddContent(0, "No Records Found");

            if (LoadingTemplate == null)
                LoadingTemplate = builder => builder.AddContent(0, "Loading ...");


            if (FieldIdentifier.Equals(default))
                FieldIdentifier = IsMultiselect()
                    ? FieldIdentifier.Create(ValuesExpression)
                    : FieldIdentifier.Create(ValueExpression);

            _debounceTimer = new Timer();
            _debounceTimer.Interval = Debounce;
            _debounceTimer.AutoReset = false;
            _debounceTimer.Elapsed += Search;
        }


        public async void Search(object source, ElapsedEventArgs e)
        {
            Loading = true;
            await InvokeAsync(StateHasChanged);

            var result = await SearchMethod(_searchText);

            SearchResults = result == null 
                ? new List<TItem>() 
                : result.ToList();

            Loading = false;
            await InvokeAsync(StateHasChanged);
        }

        public async Task SelectResult(TItem item)
        {
            var value = ConvertMethod(item);

            if (IsMultiselect())
            {
                var valueList = Values ?? new List<TValue>();
                if (valueList.Contains(value))
                    valueList.Remove(value);
                else
                    valueList.Add(value);

                await ValuesChanged.InvokeAsync(valueList);
            }
            else
            {
                await ValueChanged.InvokeAsync(value);
            }

            EditContext?.NotifyFieldChanged(FieldIdentifier);
            SearchMode = false;
        }

        public async Task RemoveValue(TValue item)
        {
            var valueList = Values ?? new List<TValue>();
            if (valueList.Contains(item))
                valueList.Remove(item);

            await ValuesChanged.InvokeAsync(valueList);
            EditContext?.NotifyFieldChanged(FieldIdentifier);
        }

        public async Task Clear()
        {
            if (IsMultiselect())
                await ValuesChanged.InvokeAsync(new List<TValue>());
            else
                await ValueChanged.InvokeAsync(default);

            EditContext?.NotifyFieldChanged(FieldIdentifier);
        }

        /// <summary>
        /// This will set focus to typeahead and open the search dropdown
        /// </summary>
        public async Task FocusTypeahead()
        {
            await JSRuntime.InvokeAsync<object>("BlazorControls.setFocus", TypeaheadControl);
        }

        public async Task HandleFocus()
        {
            if (Disabled)
                return;

            SearchText = "";
            SearchMode = true;
            await Task.Delay(250);

            await JSRuntime.InvokeAsync<object>("BlazorControls.setFocus", SearchInput);
            await JSRuntime.InvokeAsync<object>("BlazorControls.preventEnter", SearchInput, true);
        }

        public async Task HandleBlur()
        {
            // delay close to allow other events to finish
            await Task.Delay(250);

            SearchMode = false;
            Loading = false;

            // cleanup event handler
            await JSRuntime.InvokeAsync<object>("BlazorControls.preventEnter", SearchInput, false);
        }

        public async Task HandleKeydown(KeyboardEventArgs args)
        {
            if (args.Key == "ArrowDown")
                MoveSelection(1);
            else if (args.Key == "ArrowUp")
                MoveSelection(-1);
            else if ((args.Key == "Enter" || args.Key == "Tab") && SelectedIndex >= 0 && SelectedIndex < SearchResults.Count)
                await SelectResult(SearchResults[SelectedIndex]);
        }


        public bool IsMultiselect()
        {
            return ValuesExpression != null;
        }

        public bool HasSearchResult()
        {
            return SearchMode
                && !Loading
                && SearchResults.Any();
        }

        public bool HasValue()
        {
            return Value != null || (Values != null && Values.Count > 0);
        }

        public string ResultClass(TItem item, int index)
        {
            const string resultClass = "typeahead-option-selected";

            if (index == SelectedIndex)
                return resultClass;

            TValue value = ConvertMethod(item);

            if (!IsMultiselect())
                return Equals(value, Value)
                    ? resultClass
                    : string.Empty;

            if (Values == null || Values.Count == 0)
                return string.Empty;

            return Values.Contains(value)
                ? resultClass
                : string.Empty;
        }

        public string ValidationClass()
        {
            return EditContext != null
                ? EditContext.FieldCssClass(FieldIdentifier)
                : string.Empty;
        }


        public void Dispose()
        {
            _debounceTimer?.Dispose();
        }


        private void MoveSelection(int count)
        {
            var index = SelectedIndex + count;

            if (index >= SearchResults.Count)
                index = 0;

            if (index < 0)
                index = SearchResults.Count - 1;

            SelectedIndex = index;
        }

        private void ShowItems()
        {
            if (Items == null || Items.Count <= 0 || SearchResults.Count != 0)
                return;

            SearchResults = new List<TItem>(Items);
        }

    }
}
