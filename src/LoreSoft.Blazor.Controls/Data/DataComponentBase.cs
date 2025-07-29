// Ignore Spelling: Queryable Toolbar Virtualize Overscan

using System.ComponentModel;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Base class for data-bound components, providing data loading, virtualization, paging, sorting, and filtering support.
/// </summary>
/// <typeparam name="TItem">The type of the data item.</typeparam>
public abstract class DataComponentBase<TItem> : ComponentBase, IDisposable
{
    private CancellationTokenSource? _refreshCancellation;
    private IEnumerable<TItem>? _data;
    private bool _isLoading;

    /// <summary>
    /// Gets or sets the data items to display. Mutually exclusive with <see cref="DataProvider"/> and <see cref="DataLoader"/>.
    /// </summary>
    [Parameter]
    public IEnumerable<TItem>? Data { get; set; }

    /// <summary>
    /// Gets or sets the delegate to provide data asynchronously with support for paging, sorting, and filtering. Mutually exclusive with <see cref="Data"/> and <see cref="DataLoader"/>.
    /// </summary>
    [Parameter]
    public DataProviderDelegate<TItem>? DataProvider { get; set; }

    /// <summary>
    /// Gets or sets the function to load data asynchronously. Mutually exclusive with <see cref="Data"/> and <see cref="DataProvider"/>.
    /// </summary>
    [Parameter]
    public Func<Task<IEnumerable<TItem>>>? DataLoader { get; set; }

    /// <summary>
    /// Gets or sets the toolbar template for the data component.
    /// </summary>
    [Parameter]
    public RenderFragment<DataComponentBase<TItem>>? DataToolbar { get; set; }

    /// <summary>
    /// Gets or sets the pagination template for the data component.
    /// </summary>
    [Parameter]
    public RenderFragment<DataComponentBase<TItem>>? DataPagination { get; set; }

    /// <summary>
    /// Gets or sets the template displayed while loading data.
    /// </summary>
    [Parameter]
    public RenderFragment? LoadingTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template displayed when an error occurs during data loading.
    /// </summary>
    [Parameter]
    public RenderFragment<Exception>? ErrorTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template displayed when there is no data.
    /// </summary>
    [Parameter]
    public RenderFragment? EmptyTemplate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether virtualization is enabled for large data sets.
    /// </summary>
    [Parameter]
    public bool Virtualize { get; set; } = false;

    /// <summary>
    /// Gets or sets the estimated size of each virtualized item (in pixels).
    /// </summary>
    [Parameter]
    public float VirtualItemSize { get; set; } = 50f;

    /// <summary>
    /// Gets or sets the number of items to overscan before and after the visible region when virtualizing.
    /// </summary>
    [Parameter]
    public int VirtualOverscan { get; set; } = 3;

    /// <summary>
    /// Gets or sets a value indicating whether the component is currently loading data.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading == value)
                return;

            _isLoading = value;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Gets the pager state for managing paging information.
    /// </summary>
    public DataPagerState Pager { get; } = new();

    /// <summary>
    /// Gets the current view of data items after paging, sorting, and filtering.
    /// </summary>
    protected ICollection<TItem>? View { get; private set; }

    /// <summary>
    /// Gets the exception that occurred during data loading, if any.
    /// </summary>
    protected Exception? DataError { get; private set; }

    /// <summary>
    /// Gets or sets the current data provider delegate.
    /// </summary>
    protected DataProviderDelegate<TItem>? CurrentDataProvider { get; set; }

    /// <summary>
    /// Refreshes the data asynchronously, optionally resetting the pager or forcing a reload.
    /// </summary>
    /// <param name="resetPager">Whether to reset the pager to the first page.</param>
    /// <param name="forceReload">Whether to force reloading the data.</param>
    public virtual async Task RefreshAsync(bool resetPager = false, bool forceReload = false)
    {
        // reset page
        if (resetPager)
            Pager.Reset();

        // clear cached data to force data loader to re-run
        if (forceReload && DataLoader != null)
            _data = null;

        await RefreshCoreAsync();
    }

    /// <summary>
    /// Releases resources used by the component.
    /// </summary>
    public void Dispose()
    {
        _refreshCancellation?.Dispose();
        Pager.PropertyChanged -= OnStatePropertyChange;

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        Pager.PropertyChanged += OnStatePropertyChange;
    }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (DataProvider != null)
        {
            if (Data != null || DataLoader != null)
            {
                throw new InvalidOperationException(
                    $"Component can only accept one item source from its parameters. " +
                    $"Do not supply both '{nameof(Data)}' and '{nameof(DataProvider)}'.");
            }

            CurrentDataProvider = DataProvider;
        }
        else if (Data != null)
        {
            CurrentDataProvider = DefaultProvider;

            // if Data was replaces, refresh
            if (_data != Data)
            {
                _data = Data;
                await RefreshAsync();
            }
        }
        else if (DataLoader != null)
        {
            CurrentDataProvider = DefaultProvider;
        }
        else
        {
            throw new InvalidOperationException(
                $"Component requires either the '{nameof(Data)}' or '{nameof(DataProvider)}' parameters to be specified and non-null.");
        }
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        // re-render due to columns being added
        await RefreshAsync();
    }

    /// <summary>
    /// Core logic for refreshing data, handling cancellation, loading, and error states.
    /// </summary>
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
            if (CurrentDataProvider == null)
                throw new InvalidOperationException("Invalid Data Provider");

            if (CurrentDataProvider == DefaultProvider)
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

            var result = await CurrentDataProvider(request);

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
                Pager.Total = 0;
                View = null;

            }
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Creates a <see cref="DataRequest"/> for the current paging, sorting, and filtering state.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>A new <see cref="DataRequest"/> instance.</returns>
    public virtual DataRequest CreateDataRequest(CancellationToken cancellationToken = default)
    {
        return new DataRequest(Pager.Page, Pager.PageSize, null, null, cancellationToken);
    }

    /// <summary>
    /// Default data provider used when <see cref="Data"/> or <see cref="DataLoader"/> is set directly.
    /// </summary>
    /// <param name="request">The data request.</param>
    /// <returns>A <see cref="DataResult{TItem}"/> containing the filtered, sorted, and paged data.</returns>
    protected virtual async ValueTask<DataResult<TItem>> DefaultProvider(DataRequest request)
    {
        if (_data == null && DataLoader != null)
            _data = await DataLoader();

        if (_data == null || !_data.Any())
            return new DataResult<TItem>(0, []);

        var query = _data.AsQueryable();

        query = FilterData(query, request);

        var total = query.Count();

        var sorted = SortData(query, request);
        sorted = PageData(sorted, request);

        return new DataResult<TItem>(total, sorted);
    }

    /// <summary>
    /// Applies paging to the queryable data.
    /// </summary>
    /// <param name="queryable">The queryable data.</param>
    /// <param name="request">The data request.</param>
    /// <returns>The paged queryable data.</returns>
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

    /// <summary>
    /// Applies sorting to the queryable data. Override to implement custom sorting.
    /// </summary>
    /// <param name="queryable">The queryable data.</param>
    /// <param name="request">The data request.</param>
    /// <returns>The sorted queryable data.</returns>
    protected virtual IQueryable<TItem> SortData(IQueryable<TItem> queryable, DataRequest request)
    {
        return queryable;
    }

    /// <summary>
    /// Applies filtering to the queryable data. Override to implement custom filtering.
    /// </summary>
    /// <param name="queryable">The queryable data.</param>
    /// <param name="request">The data request.</param>
    /// <returns>The filtered queryable data.</returns>
    protected virtual IQueryable<TItem> FilterData(IQueryable<TItem> queryable, DataRequest request)
    {
        return queryable;
    }

    /// <summary>
    /// Handles property changes in the pager state and triggers data refresh when page or page size changes.
    /// </summary>
    /// <param name="sender">The sender object.</param>
    /// <param name="e">The property changed event arguments.</param>
    private void OnStatePropertyChange(object? sender, PropertyChangedEventArgs e)
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

