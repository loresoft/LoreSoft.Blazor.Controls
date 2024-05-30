using System.ComponentModel;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

public abstract class DataComponentBase<TItem> : ComponentBase, IDisposable
{
    private DataProviderDelegate<TItem> _dataProvider;
    private CancellationTokenSource _refreshCancellation;
    private IEnumerable<TItem> _data;

    [Parameter]
    public IEnumerable<TItem> Data { get; set; }

    [Parameter]
    public DataProviderDelegate<TItem> DataProvider { get; set; }


    [Parameter]
    public RenderFragment<DataComponentBase<TItem>> DataToolbar { get; set; }

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

    protected override async Task OnParametersSetAsync()
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

            // if Data was replaces, refresh
            if (_data != Data)
            {
                _data = Data;
                await RefreshAsync();
            }
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
        var request = new DataRequest(Pager.Page, Pager.PageSize, null, null, cancellationToken);
        return request;
    }

    // used when Data is set directly
    protected virtual ValueTask<DataResult<TItem>> DefaultProvider(DataRequest request)
    {
        if (Data == null || !Data.Any())
            return ValueTask.FromResult(new DataResult<TItem>(0, Enumerable.Empty<TItem>()));

        var query = Data.AsQueryable();

        query = FilterData(query, request);

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

    protected virtual IQueryable<TItem> FilterData(IQueryable<TItem> queryable, DataRequest request)
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

