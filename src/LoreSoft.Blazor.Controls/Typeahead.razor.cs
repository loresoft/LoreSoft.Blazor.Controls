using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LoreSoft.Blazor.Controls
{
    public class TypeaheadBase<TItem> : ComponentBase, IDisposable
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
        }

        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        [CascadingParameter] 
        private EditContext CascadedEditContext { get; set; }

        [Parameter]
        public TItem Value { get; set; }

        [Parameter]
        public EventCallback<TItem> ValueChanged { get; set; }

        [Parameter] 
        public Expression<Func<TItem>> ValueExpression { get; set; }


        [Parameter]
        public IList<TItem> Values { get; set; }

        [Parameter]
        public EventCallback<IList<TItem>> ValuesChanged { get; set; }

        [Parameter]
        public Expression<Func<IList<TItem>>> ValuesExpression { get; set; }


        [Parameter]
        public string Placeholder { get; set; }

        [Parameter]
        public IReadOnlyCollection<TItem> Items { get; set; }

        [Parameter]
        public Func<string, Task<IList<TItem>>> SearchMethod { get; set; }


        [Parameter]
        public RenderFragment NoRecordsTemplate { get; set; }

        [Parameter]
        public RenderFragment LoadingTemplate { get; set; }

        [Parameter]
        public RenderFragment<TItem> ResultTemplate { get; set; }

        [Parameter]
        public RenderFragment<TItem> SelectedTemplate { get; set; }

        [Parameter]
        public RenderFragment FooterTemplate { get; set; }


        [Parameter]
        public int MinimumLength { get; set; } = 1;

        [Parameter]
        public int Debounce { get; set; } = 800;

        [Parameter]
        public bool AllowClear { get; set; }


        public bool Loading { get; set; }

        public bool SearchMode { get; set; }

        public IList<TItem> SearchResults { get; set; }

        public ElementReference SearchInput { get; set; }

        public EditContext EditContext { get; set; }

        public FieldIdentifier FieldIdentifier { get; set; }
        
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

            if (SelectedTemplate == null)
                SelectedTemplate = item => builder => builder.AddContent(0, item?.ToString());

            if (ResultTemplate == null)
                SelectedTemplate = item => builder => builder.AddContent(0, item?.ToString());

            if (NoRecordsTemplate == null)
                NoRecordsTemplate = builder => builder.AddContent(0, "No Records Found");

            if (LoadingTemplate == null)
                LoadingTemplate = builder => builder.AddContent(0, "Loading ...");
            

            EditContext = CascadedEditContext;

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

            IList<TItem> result = null;

            try
            {
                if (SearchMethod != null)
                    result = await SearchMethod(_searchText);
            }
            catch (Exception ex)
            {
                // log error
            }

            SearchResults = result ?? new List<TItem>();

            Loading = false;
            await InvokeAsync(StateHasChanged);
        }

        public async Task SelectResult(TItem item)
        {
            if (IsMultiselect())
            {
                var valueList = Values ?? new List<TItem>();
                if (valueList.Contains(item))
                    valueList.Remove(item);
                else
                    valueList.Add(item);

                await ValuesChanged.InvokeAsync(valueList);
            }
            else
            {
                await ValueChanged.InvokeAsync(item);
            }

            EditContext?.NotifyFieldChanged(FieldIdentifier);
            SearchMode = false;
        }

        public async Task RemoveValue(TItem item)
        {
            var valueList = Values ?? new List<TItem>();
            if (valueList.Contains(item))
                valueList.Remove(item);

            await ValuesChanged.InvokeAsync(valueList);
            EditContext?.NotifyFieldChanged(FieldIdentifier);
        }

        public async Task Clear()
        {
            if (IsMultiselect())
                await ValuesChanged.InvokeAsync(new List<TItem>());
            else
                await ValueChanged.InvokeAsync(default);

            EditContext?.NotifyFieldChanged(FieldIdentifier);
        }

        public async Task HandleFocus()
        {
            SearchText = "";
            SearchMode = true;
            await Task.Delay(250);

            await JSRuntime.InvokeAsync<object>("BlazorControls.SetFocus", SearchInput);
        }

        public async Task HandleBlur()
        {
            // delay close to allow other events to finish
            await Task.Delay(250);

            SearchMode = false;
            Loading = false;
        }

        public async Task HandleKeydown(UIKeyboardEventArgs args)
        {
            if (args.Key == "ArrowDown")
                MoveSelection(1);
            else if (args.Key == "ArrowUp")
                MoveSelection(-1);
            else if (args.Key == "Enter" && SelectedIndex >= 0 && SelectedIndex < SearchResults.Count)
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

        public string OptionClass(TItem item, int index)
        {
            const string resultClass = "typeahead-option-selected";

            if (index == SelectedIndex)
                return resultClass;

            if (!IsMultiselect())
                return Equals(item, Value)
                    ? resultClass
                    : string.Empty;

            if (Values == null || Values.Count == 0)
                return string.Empty;

            return Values.Contains(item)
                ? resultClass
                : string.Empty;
        }

        public string ValidationClass()
        {
            return EditContext != null 
                ? EditContext.FieldClass(FieldIdentifier)
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
