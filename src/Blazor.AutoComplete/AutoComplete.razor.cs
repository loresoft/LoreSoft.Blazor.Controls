using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace Blazor.AutoComplete
{
    public class AutoCompleteBase<TItem> : ComponentBase, IDisposable
    {
        private Timer _debounceTimer;

        [Inject]
        private IJSRuntime JSRuntime { get; set; }


        [Parameter]
        protected TItem Value { get; set; }

        [Parameter]
        protected EventCallback<TItem> ValueChanged { get; set; }

        [Parameter]
        protected string Placeholder { get; set; }


        [Parameter]
        protected Func<string, Task<List<TItem>>> SearchMethod { get; set; }


        [Parameter]
        protected RenderFragment NoRecordsTemplate { get; set; }

        [Parameter]
        protected RenderFragment<TItem> ResultTemplate { get; set; }

        [Parameter]
        protected RenderFragment<TItem> SelectedTemplate { get; set; }

        [Parameter]
        protected RenderFragment FooterTemplate { get; set; }


        [Parameter]
        protected int MinimumLength { get; set; } = 1;

        [Parameter]
        protected int Debounce { get; set; } = 300;


        protected bool Searching { get; set; } = false;

        protected bool SearchMode { get; set; } = false;

        protected List<TItem> SearchResults { get; set; } = new List<TItem>();

        protected ElementRef SearchInput { get; set; }

        private string _searchText;
        protected string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;

                if (value.Length == 0)
                {
                    _debounceTimer.Stop();
                    SearchResults.Clear();
                }
                else if (value.Length >= MinimumLength)
                {
                    _debounceTimer.Stop();
                    _debounceTimer.Start();
                }
            }
        }

        protected int SelectedIndex { get; set; } = 0;

        protected override void OnInit()
        {
            if (SearchMethod == null)
            {
                throw new InvalidOperationException($"{GetType()} requires a {nameof(SearchMethod)} parameter.");
            }

            if (SelectedTemplate == null)
            {
                throw new InvalidOperationException($"{GetType()} requires a {nameof(SelectedTemplate)} parameter.");
            }

            if (ResultTemplate == null)
            {
                throw new InvalidOperationException($"{GetType()} requires a {nameof(ResultTemplate)} parameter.");
            }

            _debounceTimer = new Timer();
            _debounceTimer.Interval = Debounce;
            _debounceTimer.AutoReset = false;
            _debounceTimer.Elapsed += Search;
        }

        protected async void Search(object source, ElapsedEventArgs e)
        {
            Searching = true;
            await Invoke(StateHasChanged);

            var result = await SearchMethod?.Invoke(_searchText);

            SearchResults = result ?? new List<TItem>();

            Searching = false;
            await Invoke(StateHasChanged);
        }

        protected async Task SelectResult(TItem item)
        {
            await ValueChanged.InvokeAsync(item);

            SearchMode = false;
        }


        protected async Task HandleFocus(UIFocusEventArgs args)
        {
            SearchText = "";
            SearchMode = true;
            await Task.Delay(250);

            await JSRuntime.InvokeAsync<object>("AutoComplete.SetFocus", SearchInput);
        }

        protected async Task HandleBlur()
        {
            // delay close to allow other events to finish
            await Task.Delay(250);

            SearchMode = false;
        }

        protected async Task HandleKey(UIKeyboardEventArgs args)
        {
            if (SearchResults.Count == 0)
            {
                SelectedIndex = -1;
                return;
            }

            switch (args.Key)
            {
                case "ArrowDown":
                    var down = SelectedIndex + 1;
                    if (down >= SearchResults.Count)
                        down = 0;

                    if (down < 0)
                        down = 0;

                    SelectedIndex = down;
                    break;

                case "ArrowUp":
                    var up = SelectedIndex - 1;

                    if (up >= SearchResults.Count)
                        up = 0;

                    if (up < 0)
                        up = SearchResults.Count - 1;

                    SelectedIndex = up;
                    break;
                case "Enter":
                    if (SelectedIndex >= 0 && SelectedIndex < SearchResults.Count)
                    {
                        var item = SearchResults[SelectedIndex];
                        await SelectResult(item);
                    }

                    break;
            }

        }


        protected string ArrowClass()
        {
            return SearchMode ? "dropdown-arrow-open" : "dropdown-arrow-close";
        }

        protected bool ShowNoRecords()
        {
            return SearchMode
                && !Searching
                && !SearchResults.Any();
        }

        protected string ResultClass(TItem item, int index)
        {
            return index == SelectedIndex
                ? "autocomplete-result-selected"
                : "";
        }

        public void Dispose()
        {
            _debounceTimer.Dispose();
        }
    }
}
