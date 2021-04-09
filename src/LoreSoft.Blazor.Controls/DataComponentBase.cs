using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls
{
    public abstract class DataComponentBase<TItem> : ComponentBase, IDisposable
    {
        private DataProviderDelegate<TItem> _dataProvider;
        private CancellationTokenSource _refreshCancellation;

        [Parameter]
        public ICollection<TItem> Data { get; set; }

        [Parameter]
        public DataProviderDelegate<TItem> DataProvider { get; set; }


        [Parameter]
        public RenderFragment DataToolbar { get; set; }

        [Parameter]
        public RenderFragment<DataComponentBase<TItem>> DataPagination { get; set; }

        [Parameter]
        public RenderFragment LoadingTemplate { get; set; }

        [Parameter]
        public RenderFragment<Exception> ErrorTemplate { get; set; }

        [Parameter]
        public RenderFragment EmptyTemplate { get; set; }


        [Parameter]
        public bool Virtualize { get; set; } = false;

        [Parameter]
        public float VirtualItemSize { get; set; } = 50f;

        [Parameter]
        public int VirtualOverscan { get; set; } = 3;


        [Parameter]
        public Func<TItem, bool> Filter { get; set; } = null;


        public bool IsLoading { get; set; }

        public DataPagerState Pager { get; } = new();

        protected ICollection<TItem> View { get; private set; }

        protected Exception DataError { get; private set; }


        public virtual async Task RefreshAsync(bool resetPager = false)
        {
            // reset page
            if (resetPager)
                Pager.Reset();

            await RefreshCoreAsync();
        }

        public void Dispose()
        {
            _refreshCancellation?.Dispose();
            Pager.PropertyChanged -= OnStatePropertyChange;
        }


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
                        $"Component can only accept one item source from its parameters. " +
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
                    $"Component requires either the '{nameof(Data)}' or '{nameof(DataProvider)}' parameters to be specified and non-null.");
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            // re-render due to columns being added
            await RefreshAsync();
        }


        protected virtual async ValueTask RefreshCoreAsync()
        {
            // cancel pending refresh
            _refreshCancellation?.Cancel();

            // clean up
            CancellationToken cancellationToken = default;
            DataError = null;

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

                var request = CreateDataRequest(cancellationToken);

                var result = await _dataProvider(request);

                if (!cancellationToken.IsCancellationRequested)
                {
                    Pager.Total = result.Total;
                    View = result.Items.ToList();
                }
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException oce && oce.CancellationToken == cancellationToken)
                {
                    // canceled the operation, suppress exception.
                }
                else
                {
                    DataError = ex;
                }
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }


        protected virtual DataRequest CreateDataRequest(CancellationToken cancellationToken)
        {
            var request = new DataRequest(Pager.Page, Pager.PageSize, null, cancellationToken);
            return request;
        }

        // used when Data is set directly
        protected virtual ValueTask<DataResult<TItem>> DefaultProvider(DataRequest request)
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

        protected virtual IQueryable<TItem> PageData(IQueryable<TItem> queryable, DataRequest request)
        {
            if (Pager == null || request.PageSize == 0)
                return queryable;

            var size = request.PageSize;
            int skip = Math.Max(size * (request.Page - 1), 0);

            return queryable
                .Skip(skip)
                .Take(size);
        }

        protected virtual IQueryable<TItem> SortData(IQueryable<TItem> queryable, DataRequest request)
        {
            return queryable;
        }


        private void OnStatePropertyChange(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DataPagerState.Page):
                case nameof(DataPagerState.PageSize):
                    InvokeAsync(() => RefreshAsync());
                    break;
            }
        }
    }

    public record DataSort(string Property, bool Descending);

    public record DataRequest(int Page, int PageSize, DataSort[] Sorts, CancellationToken CancellationToken);

    public record DataResult<TItem>(int Total, IEnumerable<TItem> Items);

    public delegate ValueTask<DataResult<TItem>> DataProviderDelegate<TItem>(DataRequest request);
}