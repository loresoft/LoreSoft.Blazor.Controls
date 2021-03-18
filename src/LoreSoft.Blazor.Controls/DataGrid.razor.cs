using System;
using System.Collections.Generic;
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
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public RenderFragment LoadingTemplate { get; set; }


        [Parameter]
        public bool Sortable { get; set; } = true;


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


        public List<DataColumn<TItem>> Columns { get; } = new();

        public DataPager<TItem> Pager { get; private set; }


        public async Task RefreshAsync()
        {
            Console.WriteLine("DataGrid.RefreshAsync()");

            await RefreshCoreAsync(renderOnSuccess: true);
        }


        public async Task SortByAsync(DataColumn<TItem> column)
        {
            Console.WriteLine("DataGrid.SortByAsync()");

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


        protected override void OnParametersSet()
        {
            Console.WriteLine("DataGrid.OnParametersSet()");

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
                _dataProvider = DefaultDataProvider;

                var refreshTask = RefreshCoreAsync(renderOnSuccess: false);
            }
            else
            {
                throw new InvalidOperationException(
                    $"DataGrid requires either the '{nameof(Data)}' or '{nameof(DataProvider)}' parameters to be specified and non-null.");
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
                StateHasChanged(); // re-render due to columns being added
        }


        protected ICollection<TItem> PageData(ICollection<TItem> queryable)
        {
            if (Pager == null || Pager.PageSize == 0)
                return queryable;

            var size = Pager.PageSize;
            int skip = Math.Max(size * (Pager.Page - 1), 0);

            return queryable
                .Skip(skip)
                .Take(size)
                .ToList();
        }

        protected ICollection<TItem> SortData(IQueryable<TItem> queryable)
        {
            if (!Sortable)
                return queryable.ToList();

            var sorted = Columns
                .Where(c => c.SortIndex >= 0)
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


        internal void AddColumn(DataColumn<TItem> column)
        {
            Columns.Add(column);
        }

        internal void SetPager(DataPager<TItem> pager)
        {
            Pager = pager;
        }


        private async ValueTask RefreshCoreAsync(bool renderOnSuccess)
        {
            Console.WriteLine("DataGrid.RefreshCoreAsync()");

            _refreshCancellation?.Cancel();
            CancellationToken cancellationToken;

            if (_dataProvider == DefaultDataProvider)
            {
                _refreshCancellation = null;
                cancellationToken = CancellationToken.None;
            }
            else
            {
                _refreshCancellation = new CancellationTokenSource();
                cancellationToken = _refreshCancellation.Token;
            }

            var request = Pager == null
                ? new DataRequest(0, 0, cancellationToken)
                : new DataRequest(Pager.StartItem, Pager.PageSize, cancellationToken);

            try
            {
                var result = await _dataProvider(request);

                if (!cancellationToken.IsCancellationRequested)
                {
                    if (Pager != null)
                        Pager.TotalItems = result.Total;

                    View = result.Items.ToList();

                    if (renderOnSuccess)
                        StateHasChanged();
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
                    StateHasChanged();
                }
            }
        }

        private ValueTask<DataResult<TItem>> DefaultDataProvider(DataRequest request)
        {
            Console.WriteLine("DataGrid.DefaultDataProvider()");

            var query = Data.AsQueryable();

            if (Filter != null)
                query = query.Where(i => Filter(i));

            var total = query.Count();

            var sorted = SortData(query);
            sorted = PageData(sorted);


            return ValueTask.FromResult(new DataResult<TItem>(total, sorted));
        }

    }

    public record DataRequest(int Start, int Count, CancellationToken CancellationToken);

    public record DataResult<TItem>(int Total, IEnumerable<TItem> Items);

    public delegate ValueTask<DataResult<TItem>> DataProviderDelegate<TItem>(DataRequest request);

}
