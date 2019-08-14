using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LoreSoft.Blazor.Controls
{
    public class AutoCompleteBase<TItem> : ComponentBase, IDisposable
    {
        private Timer _debounceTimer;

        public AutoCompleteBase()
        {
            Items = new List<TItem>();
            AllowClear = true;
            Searching = false;
            SearchMode = false;
            SelectedIndex = 0;
        }

        [Inject]
        private IJSRuntime JSRuntime { get; set; }


        [Parameter]
        public TItem Value { get; set; }

        [Parameter]
        public EventCallback<TItem> ValueChanged { get; set; }

        [Parameter]
        public string Placeholder { get; set; }

        [Parameter]
        public List<TItem> Items { get; set; }

        [Parameter]
        public Func<string, Task<List<TItem>>> SearchMethod { get; set; }


        [Parameter]
        public RenderFragment NoRecordsTemplate { get; set; }

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


        public bool Searching { get; set; }

        public bool SearchMode { get; set; }

        public List<TItem> SearchResults { get; set; } = new List<TItem>();

        public ElementReference SearchInput { get; set; }


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
                throw new InvalidOperationException($"{GetType()} requires a {nameof(SelectedTemplate)} parameter.");

            if (ResultTemplate == null)
                throw new InvalidOperationException($"{GetType()} requires a {nameof(ResultTemplate)} parameter.");

            _debounceTimer = new Timer();
            _debounceTimer.Interval = Debounce;
            _debounceTimer.AutoReset = false;
            _debounceTimer.Elapsed += Search;
        }

        public async void Search(object source, ElapsedEventArgs e)
        {
            Searching = true;
            await InvokeAsync(StateHasChanged);

            List<TItem> result = null;

            try
            {
                result = await SearchMethod?.Invoke(_searchText);
            }
            catch (Exception ex)
            {
                // log error
            }

            SearchResults = result ?? new List<TItem>();

            Searching = false;
            await InvokeAsync(StateHasChanged);
        }

        public async Task SelectResult(TItem item)
        {
            await ValueChanged.InvokeAsync(item);

            SearchMode = false;
        }

        public async Task Clear()
        {
            await ValueChanged.InvokeAsync(default(TItem));
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
            Searching = false;
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

        public bool ShowNoRecords()
        {
            return SearchMode
                && !Searching
                && !SearchResults.Any();
        }

        public string ResultClass(TItem item, int index)
        {
            return index == SelectedIndex || Equals(item, Value)
                ? "autocomplete-result-selected"
                : "";
        }


        public void Dispose()
        {
            _debounceTimer.Dispose();
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
