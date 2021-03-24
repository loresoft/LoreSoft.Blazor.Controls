using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls
{
    public partial class DataGrid<TItem> : ComponentBase
    {
        private HashSet<TItem> _expandedItems = new();
        private DataProviderDelegate<TItem> _dataProvider;
        private CancellationTokenSource _refreshCancellation;
        private Exception _refreshException;

        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> TableAttributes { get; set; }

        [Parameter]
        public ICollection<TItem> Data { get; set; }

        [Parameter]
        public DataProviderDelegate<TItem> DataProvider { get; set; }


        [Parameter]
        public RenderFragment<DataGrid<TItem>> DataToolbar { get; set; }

        [Parameter]
        public RenderFragment DataColumns { get; set; }

        [Parameter]
        public RenderFragment<DataGrid<TItem>> DataPagination { get; set; }


        [Parameter]
        public RenderFragment<TItem> DetailTemplate { get; set; }


        [Parameter]
        public RenderFragment<DataGrid<TItem>> LoadingTemplate { get; set; }

        [Parameter]
        public RenderFragment<DataGrid<TItem>> ErrorTemplate { get; set; }

        [Parameter]
        public RenderFragment<DataGrid<TItem>> EmptyTemplate { get; set; }

        [Parameter]
        public bool Selectable { get; set; }

        [Parameter]
        public bool Sortable { get; set; } = true;

        [Parameter]
        public bool Virtualize { get; set; } = false;


        [Parameter]
        public float VirtualItemSize { get; set; } = 50f;

        [Parameter]
        public int VirtualOverscan { get; set; } = 3;


        [Parameter]
        public string TableClass { get; set; } = "table";

        [Parameter]
        public string RowClass { get; set; }

        [Parameter]
        public Func<TItem, string> RowStyle { get; set; }

        [Parameter]
        public Func<TItem, bool> Filter { get; set; } = null;


        [Parameter]
        public IEnumerable<TItem> SelectedItems { get; set; } = new List<TItem>();

        [Parameter]
        public EventCallback<IEnumerable<TItem>> SelectedItemsChanged { get; set; }

        public bool IsLoading { get; set; }

        public List<DataColumn<TItem>> Columns { get; } = new();

        public DataPagerState Pager { get; } = new();


        public async Task RefreshAsync(bool resetPager = false)
        {
            // clear row flags on refresh
            _expandedItems.Clear();
            SetSelectedItems(new List<TItem>());

            // reset page
            if (resetPager)
                Pager.Reset();

            await RefreshCoreAsync();
        }


        public async Task SortByAsync(DataColumn<TItem> column)
        {
            if (column == null || !Sortable || !column.Sortable)
                return;

            var descending = column.SortIndex >= 0 && !column.SortDescending;

            Columns.ForEach(c => c.UpdateSort(-1, false));

            column.UpdateSort(0, descending);

            await RefreshAsync();
        }

        public async Task SortByAsync(string columnName)
        {
            if (!Sortable)
                return;

            var column = Columns.Find(c => c.Name == columnName);
            await SortByAsync(column);
        }


        protected ICollection<TItem> View { get; private set; }

        protected List<DataColumn<TItem>> VisibleColumns => Columns.Where(c => c.Visible).ToList();

        protected int CellCount => (Columns?.Count ?? 0) + (DetailTemplate != null ? 1 : 0) + (Selectable ? 1 : 0);

        protected override void OnInitialized()
        {
            Pager.PropertyChanged += OnStatePropertyChange;
        }

        protected override void OnParametersSet()
        {
            if (DataProvider != null)
            {
                if (Data != null)
                {
                    throw new InvalidOperationException(
                        $"DataGrid can only accept one item source from its parameters. " +
                        $"Do not supply both '{nameof(Data)}' and '{nameof(DataProvider)}'.");
                }

                _dataProvider = DataProvider;
            }
            else if (Data != null)
            {
                _dataProvider = DefaultProvider;
            }
            else
            {
                throw new InvalidOperationException(
                    $"DataGrid requires either the '{nameof(Data)}' or '{nameof(DataProvider)}' parameters to be specified and non-null.");
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            // verify columns added
            if (Columns == null || Columns.Count == 0)
                throw new InvalidOperationException("DataGrid requires at least one DataColumn child component.");


            // re-render due to columns being added
            await RefreshAsync();
        }


        protected bool IsRowExpanded(TItem item)
        {
            return _expandedItems.Contains(item);
        }

        protected void ToggleDetailRow(TItem item)
        {
            if (_expandedItems.Contains(item))
                _expandedItems.Remove(item);
            else
                _expandedItems.Add(item);

            StateHasChanged();
        }


        protected bool IsAllSelected()
        {
            return View?.All(IsRowSelected) == true;
        }

        protected void ToggleSelectAll()
        {
            if (IsAllSelected())
                DeselectAll();
            else
                SelectAll();
        }

        protected bool IsRowSelected(TItem item)
        {
            return SelectedItems?.Contains(item) ?? false;
        }

        protected void ToggleSelectRow(TItem item)
        {
            var list = new List<TItem>(SelectedItems ?? Enumerable.Empty<TItem>());
            if (IsRowSelected(item))
            {
                list.Remove(item);
                SetSelectedItems(list);
            }
            else
            {
                list.Add(item);
                SetSelectedItems(list);
            }

            StateHasChanged();
        }

        protected void SelectAll()
        {
            var list = View?.ToList() ?? new List<TItem>();
            SetSelectedItems(list);
            StateHasChanged();
        }

        protected void DeselectAll()
        {
            SetSelectedItems(new List<TItem>());
            StateHasChanged();
        }

        protected async void SetSelectedItems(IEnumerable<TItem> items)
        {
            SelectedItems = items;
            await SelectedItemsChanged.InvokeAsync(SelectedItems);
        }


        internal void AddColumn(DataColumn<TItem> column)
        {
            Columns.Add(column);
        }


        private async ValueTask RefreshCoreAsync()
        {
            // cancel pending refresh
            _refreshCancellation?.Cancel();

            // clean up
            CancellationToken cancellationToken = default;
            _refreshException = null;

            IsLoading = true;
            StateHasChanged();

            try
            {
                if (_dataProvider == DefaultProvider)
                {
                    _refreshCancellation = null;
                    cancellationToken = CancellationToken.None;
                }
                else
                {
                    _refreshCancellation = new CancellationTokenSource();
                    cancellationToken = _refreshCancellation.Token;
                }

                var sorts = Columns
                    .Where(c => c.SortIndex >= 0)
                    .OrderBy(c => c.SortIndex)
                    .Select(c => new DataSort(c.Name, c.SortDescending))
                    .ToArray();

                var request = new DataRequest(Pager.Page, Pager.PageSize, sorts, cancellationToken);

                var result = await _dataProvider(request);

                if (!cancellationToken.IsCancellationRequested)
                {
                    Pager.Total = result.Total;
                    View = result.Items.ToList();
                }
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException oce && oce.CancellationToken == cancellationToken)
                {
                    // canceled the operation, suppress exception.
                }
                else
                {
                    _refreshException = e;
                }
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        // used when Data is set directly
        private ValueTask<DataResult<TItem>> DefaultProvider(DataRequest request)
        {
            if (Data == null || Data.Count == 0)
                return ValueTask.FromResult(new DataResult<TItem>(0, Enumerable.Empty<TItem>()));

            var query = Data.AsQueryable();

            if (Filter != null)
                query = query.Where(i => Filter(i));

            var total = query.Count();

            var sorted = SortData(query, request);
            sorted = PageData(sorted, request);

            return ValueTask.FromResult(new DataResult<TItem>(total, sorted));
        }

        private ICollection<TItem> PageData(ICollection<TItem> queryable, DataRequest request)
        {
            if (Pager == null || request.PageSize == 0)
                return queryable;

            var size = request.PageSize;
            int skip = Math.Max(size * (request.Page - 1), 0);

            return queryable
                .Skip(skip)
                .Take(size)
                .ToList();
        }

        private ICollection<TItem> SortData(IQueryable<TItem> queryable, DataRequest request)
        {
            if (!Sortable || request.Sorts == null || request.Sorts.Length == 0)
                return queryable.ToList();

            var columns = request.Sorts.Select(s => s.Property);

            var sorted = Columns
                .Where(c => columns.Contains(c.Name))
                .OrderBy(c => c.SortIndex)
                .ToList();

            if (sorted.Count == 0)
                return queryable.ToList();

            var queue = new Queue<DataColumn<TItem>>(sorted);
            var column = queue.Dequeue();

            var orderedQueryable = column.SortDescending
                ? queryable.OrderByDescending(column.Property)
                : queryable.OrderBy(column.Property);

            while (queue.Count > 0)
            {
                column = queue.Dequeue();

                orderedQueryable = column.SortDescending
                    ? orderedQueryable.ThenByDescending(column.Property)
                    : orderedQueryable.ThenBy(column.Property);
            }

            return orderedQueryable.ToList();
        }

        private void OnStatePropertyChange(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DataPagerState.Page):
                case nameof(DataPagerState.PageSize):
                    Task.Run(() => RefreshAsync(false));
                    break;
            }
        }

    }

    public record DataSort(string Property, bool Descending);

    public record DataRequest(int Page, int PageSize, DataSort[] Sorts, CancellationToken CancellationToken);

    public record DataResult<TItem>(int Total, IEnumerable<TItem> Items);

    public delegate ValueTask<DataResult<TItem>> DataProviderDelegate<TItem>(DataRequest request);
}
