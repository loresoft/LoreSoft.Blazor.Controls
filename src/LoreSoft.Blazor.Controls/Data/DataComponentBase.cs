// Ignore Spelling: Queryable Toolbar Virtualize Overscan

using System.ComponentModel;

using LoreSoft.Blazor.Controls.Extensions;
using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Base class for data-bound components, providing data loading, virtualization, paging, sorting, and filtering support.
/// This abstract class serves as the foundation for components like <see cref="DataGrid{TItem}"/> and provides
/// comprehensive data management functionality including asynchronous data loading, query building, and state management.
/// </summary>
/// <typeparam name="TItem">The type of the data item that the component will display and manipulate.</typeparam>
public abstract class DataComponentBase<TItem> : ComponentBase, IDisposable
{
    private CancellationTokenSource? _refreshCancellation;
    private IEnumerable<TItem>? _data;
    private bool _isLoading;
    private DataSort? _currentSort;
    private QueryGroup? _initialQuery;

    /// <summary>
    /// Gets or sets the JavaScript runtime for interop calls.
    /// This service is used for client-side operations such as file downloads and DOM manipulation.
    /// </summary>
    [Inject]
    public required IJSRuntime JavaScript { get; set; }

    /// <summary>
    /// Gets or sets the service used for downloading files.
    /// This service provides methods to download data exports, reports, and other file content to the user's device.
    /// </summary>
    [Inject]
    public required DownloadService DownloadService { get; set; }

    /// <summary>
    /// Gets or sets additional attributes to be applied to the root element.
    /// These attributes will be merged with the component's default attributes and applied to the outermost HTML element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets the data items to display. Mutually exclusive with <see cref="DataProvider"/> and <see cref="DataLoader"/>.
    /// When this parameter is set, the data is treated as static and all operations (filtering, sorting, paging)
    /// are performed client-side. Use this for small to medium datasets that can be loaded entirely into memory.
    /// </summary>
    [Parameter]
    public IEnumerable<TItem>? Data { get; set; }

    /// <summary>
    /// Gets or sets the delegate to provide data asynchronously with support for paging, sorting, and filtering.
    /// Mutually exclusive with <see cref="Data"/> and <see cref="DataLoader"/>.
    /// This is the most flexible option, allowing for server-side operations and lazy loading of large datasets.
    /// The delegate receives a <see cref="DataRequest"/> with current paging, sorting, and filtering parameters.
    /// </summary>
    [Parameter]
    public DataProviderDelegate<TItem>? DataProvider { get; set; }

    /// <summary>
    /// Gets or sets the function to load data asynchronously. Mutually exclusive with <see cref="Data"/> and <see cref="DataProvider"/>.
    /// This option is suitable when you need to load all data asynchronously but want client-side operations.
    /// The function is called once to load the complete dataset, after which sorting, filtering, and paging are performed client-side.
    /// </summary>
    [Parameter]
    public Func<Task<IEnumerable<TItem>>>? DataLoader { get; set; }

    /// <summary>
    /// Gets or sets the toolbar template for the data component.
    /// This template is rendered above the data display and typically contains buttons for actions like
    /// add, edit, delete, export, and other component-specific operations. The template receives the component instance as context.
    /// </summary>
    [Parameter]
    public RenderFragment<DataComponentBase<TItem>>? DataToolbar { get; set; }

    /// <summary>
    /// Gets or sets the pagination template for the data component.
    /// This template is rendered below the data display and provides navigation controls for paging through data.
    /// The template receives the component instance as context, providing access to pager state and navigation methods.
    /// </summary>
    [Parameter]
    public RenderFragment<DataComponentBase<TItem>>? DataPagination { get; set; }

    /// <summary>
    /// Gets or sets the template displayed while loading data.
    /// This template is shown during asynchronous data loading operations to provide user feedback.
    /// If not specified, a default loading indicator will be displayed.
    /// </summary>
    [Parameter]
    public RenderFragment? LoadingTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template displayed when an error occurs during data loading.
    /// The template receives the <see cref="Exception"/> instance as context, allowing for custom error display
    /// and potentially error-specific recovery actions.
    /// </summary>
    [Parameter]
    public RenderFragment<Exception>? ErrorTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template displayed when there is no data.
    /// This template is shown when the data source is empty or when filters result in no matching records.
    /// Use this to provide user guidance or alternative actions when no data is available.
    /// </summary>
    [Parameter]
    public RenderFragment? EmptyTemplate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether sorting is enabled.
    /// When true, users can sort data by clicking on sortable column headers. This affects both
    /// client-side and server-side data operations depending on the data source type.
    /// </summary>
    [Parameter]
    public bool Sortable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether filtering is enabled.
    /// When true, users can apply filters to narrow down the displayed data. Filter operations
    /// are applied according to the current data source configuration (client-side or server-side).
    /// </summary>
    [Parameter]
    public bool Filterable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether virtualization is enabled for large data sets.
    /// When enabled, only visible items are rendered in the DOM, significantly improving performance
    /// for large datasets. This is particularly useful for datasets with thousands of items.
    /// </summary>
    [Parameter]
    public bool Virtualize { get; set; } = false;

    /// <summary>
    /// Gets or sets the estimated size of each virtualized item (in pixels).
    /// This value is used by the virtualization engine to calculate scroll positions and visible item ranges.
    /// A more accurate estimate improves scroll bar behavior and reduces layout shifts.
    /// </summary>
    [Parameter]
    public float VirtualItemSize { get; set; } = 50f;

    /// <summary>
    /// Gets or sets the number of items to overscan before and after the visible region when virtualizing.
    /// Higher values provide smoother scrolling at the cost of rendering more items.
    /// Lower values improve memory usage but may cause brief rendering delays during fast scrolling.
    /// </summary>
    [Parameter]
    public int VirtualOverscan { get; set; } = 3;

    /// <summary>
    /// Gets or sets the query group for filtering and searching.
    /// This parameter allows external control over the component's filter state.
    /// When set, this query will be merged with the internal <see cref="RootQuery"/> for filtering operations.
    /// </summary>
    [Parameter]
    public QueryGroup? Query { get; set; }

    /// <summary>
    /// Event triggered when the data grid is initialized.
    /// </summary>
    [Parameter]
    public EventCallback<DataGrid<TItem>> Initialized { get; set; }

    /// <summary>
    /// Gets the root query group for filtering and searching.
    /// This is the primary filter container that holds all active filters and query groups.
    /// It's automatically managed by the component's filter operations but can be accessed for advanced scenarios.
    /// </summary>
    public QueryGroup RootQuery { get; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the component is currently loading data.
    /// This property automatically triggers UI updates when its value changes, allowing templates
    /// and other UI elements to respond to loading state changes.
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
    /// Provides access to current page, page size, total items, and navigation properties.
    /// The pager automatically notifies the component when page-related properties change.
    /// </summary>
    public DataPagerState Pager { get; } = new();

    /// <summary>
    /// Gets the current view of data items after paging, sorting, and filtering.
    /// This collection represents the actual data that should be displayed to the user,
    /// containing only the items for the current page and matching current filters and sort criteria.
    /// </summary>
    protected ICollection<TItem>? View { get; private set; }

    /// <summary>
    /// Gets the exception that occurred during data loading, if any.
    /// When not null, indicates that the last data loading operation failed.
    /// This is typically used by error templates to display appropriate error messages.
    /// </summary>
    protected Exception? DataError { get; private set; }

    /// <summary>
    /// Gets the current data provider delegate.
    /// This is the active data provider being used for data operations, which may be
    /// the user-specified <see cref="DataProvider"/> or the internal <see cref="DefaultProvider"/>.
    /// </summary>
    protected DataProviderDelegate<TItem>? CurrentDataProvider { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the filter panel is open.
    /// This property controls the visibility of filter UI components and can be used
    /// to coordinate filter panel state with other UI elements.
    /// </summary>
    protected bool FilterOpen { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the component has been rendered.
    /// </summary>
    protected bool Rendered { get; private set; }

    /// <summary>
    /// Shows the filter panel.
    /// If no filters exist, a default empty filter is added to provide a starting point for user input.
    /// This method triggers a UI update to display the filter interface.
    /// </summary>
    public void ShowFilter()
    {
        if (RootQuery.Filters.Count == 0)
            RootQuery.Filters.Add(new QueryFilter());

        FilterOpen = true;
        StateHasChanged();
    }

    /// <summary>
    /// Closes the filter panel.
    /// This method hides the filter UI without affecting the current filter state.
    /// Applied filters remain active and continue to affect the displayed data.
    /// </summary>
    public void CloseFilter()
    {
        FilterOpen = false;
        StateHasChanged();
    }

    /// <summary>
    /// Toggles the filter panel open or closed.
    /// If the panel is currently open, it will be closed and vice versa.
    /// When opening a panel with no existing filters, a default empty filter is added.
    /// </summary>
    public void ToggleFilter()
    {
        if (!FilterOpen && RootQuery.Filters.Count == 0)
            RootQuery.Filters.Add(new QueryFilter());

        FilterOpen = !FilterOpen;
        StateHasChanged();
    }

    /// <summary>
    /// Determines whether the filter is active.
    /// A filter is considered active when the <see cref="RootQuery"/> contains valid filter rules
    /// that would affect the data display.
    /// </summary>
    /// <returns>True if the filter is active and will affect data display; otherwise, false.</returns>
    public bool IsFilterActive()
    {
        return LinqExpressionBuilder.IsValid(RootQuery);
    }

    /// <summary>
    /// Applies the current filters and refreshes the data display.
    /// This method closes the filter panel and triggers a data refresh with the current filter state.
    /// The pager is reset to the first page to ensure users see the filtered results from the beginning.
    /// </summary>
    protected async Task ApplyFilters()
    {
        FilterOpen = false;
        await RefreshAsync(true);
    }

    /// <summary>
    /// Clears all filters and refreshes the data display.
    /// This method removes all active filters, closes the filter panel, and refreshes the data
    /// to show the complete unfiltered dataset. The pager is reset to the first page.
    /// </summary>
    public async Task ClearFilters()
    {
        RootQuery.Filters.Clear();
        FilterOpen = false;
        await RefreshAsync(true);
    }

    /// <summary>
    /// Applies the specified filter rules to the data display.
    /// This method allows programmatic application of filters without user interaction with the filter UI.
    /// </summary>
    /// <param name="rules">The collection of filter rules to apply. Each rule defines a specific filter condition.</param>
    /// <param name="replace">
    /// When true, existing filters are cleared before applying new rules.
    /// When false, new rules are added to existing filters.
    /// </param>
    /// <param name="refresh">Whether to refresh the data display after applying the filter.</param>
    /// <returns>A task representing the asynchronous filter application and data refresh operation.</returns>
    public async Task ApplyFilters(IEnumerable<QueryRule> rules, bool replace = false, bool refresh = true)
    {
        if (replace)
            RootQuery.Filters.Clear();

        if (rules != null)
            RootQuery.Filters.AddRange(rules);

        if (refresh)
            await RefreshAsync(true);
    }

    /// <summary>
    /// Applies a single filter rule to the data display.
    /// If a filter with the same ID already exists, it will be replaced with the new rule.
    /// This allows for easy updates to existing filters without duplicating filter conditions.
    /// </summary>
    /// <param name="rule">The filter rule to apply. If null, no action is taken.</param>
    /// <param name="refresh">Whether to refresh the data display after applying the filter.</param>
    /// <returns>A task representing the asynchronous filter application and data refresh operation.</returns>
    public async Task ApplyFilter(QueryRule? rule, bool refresh = true)
    {
        if (rule == null)
            return;

        if (rule.Id.HasValue())
            RootQuery.Filters.RemoveAll(f => f.Id == rule.Id);

        RootQuery.Filters.Add(rule);

        if (refresh)
            await RefreshAsync(true);
    }

    /// <summary>
    /// Removes filters matching the specified predicate.
    /// This method provides flexible filter removal based on custom criteria,
    /// allowing for removal of filters by property values, types, or other characteristics.
    /// </summary>
    /// <param name="match">The predicate function that determines which filters to remove.
    /// Filters for which this function returns true will be removed.</param>
    /// <returns>A task representing the asynchronous filter removal and data refresh operation.</returns>
    public async Task RemoveFilters(Predicate<QueryRule> match)
    {
        RootQuery.Filters.RemoveAll(match);
        await RefreshAsync(true);
    }

    /// <summary>
    /// Removes filters with the specified ID.
    /// This method provides a convenient way to remove specific filters when their IDs are known,
    /// commonly used for removing filters applied programmatically.
    /// </summary>
    /// <param name="id">The unique identifier of the filter(s) to remove.</param>
    /// <returns>A task representing the asynchronous filter removal and data refresh operation.</returns>
    public async Task RemoveFilter(string id)
    {
        RootQuery.Filters.RemoveAll(f => f.Id == id);
        await RefreshAsync(true);
    }

    /// <summary>
    /// Refreshes the data asynchronously, optionally resetting the pager or forcing a reload.
    /// This is the primary method for updating the component's data display and should be called
    /// whenever the underlying data or display parameters change.
    /// </summary>
    /// <param name="resetPager">When true, resets the pager to the first page.
    /// Useful when applying filters or sorts that may change the total number of available pages.</param>
    /// <param name="forceReload">When true, forces reloading of data from the original source,
    /// bypassing any cached data. Only applicable when using <see cref="DataLoader"/>.</param>
    /// <returns>A task representing the asynchronous data refresh operation.</returns>
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
    /// This method cancels any pending data operations and unsubscribes from pager events
    /// to prevent memory leaks and ensure proper cleanup.
    /// </summary>
    public virtual void Dispose()
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
        if (_initialQuery != Query)
        {
            await ApplyFilter(Query, Rendered);
            _initialQuery = Query;
        }

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
        Rendered = true;

        if (!firstRender)
            return;

        // notify initialized
        await Initialized.InvokeAsync((DataGrid<TItem>)this);

        // re-render due to columns being added
        await RefreshAsync();
    }

    /// <summary>
    /// Core logic for refreshing data, handling cancellation, loading, and error states.
    /// This method orchestrates the complete data loading process, including error handling,
    /// cancellation support, and state management for loading indicators.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous data refresh operation.</returns>
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
    /// Sorts the data by the column with the specified name and refreshes the display.
    /// This method updates the sort configuration and immediately applies it by refreshing the data.
    /// For more granular control without immediate refresh, use <see cref="SortBy(string, bool?)"/>.
    /// </summary>
    /// <param name="columnName">The name of the column to sort by.
    /// This should match a property name in the data item type.</param>
    /// <param name="descending">The sort direction. If null, toggles the current direction
    /// or defaults to ascending for new sorts.</param>
    /// <returns>A task representing the asynchronous sort and refresh operation.</returns>
    public virtual async Task SortByAsync(string columnName, bool? descending = null)
    {
        if (!Sortable)
            return;

        SortBy(columnName, descending);

        await RefreshAsync();
    }

    /// <summary>
    /// Sorts the data by the column with the specified name without refreshing the display.
    /// This method updates the internal sort state but does not trigger a data refresh.
    /// Call RefreshAsync() after configuring sorts to apply the changes.
    /// </summary>
    /// <param name="columnName">The name of the column to sort by.
    /// This should match a property name in the data item type.
    /// Pass null or empty string to clear the current sort.</param>
    /// <param name="descending">The sort direction. If null, toggles the current direction
    /// or defaults to ascending for new sorts.</param>
    public virtual void SortBy(string columnName, bool? descending = null)
    {
        if (!Sortable)
            return;

        if (string.IsNullOrWhiteSpace(columnName))
            _currentSort = null;
        else if (_currentSort == null || _currentSort.Property != columnName)
            _currentSort = new DataSort(columnName, descending ?? false);
        else
            _currentSort = new DataSort(columnName, descending ?? !_currentSort.Descending);
    }

    /// <summary>
    /// Creates a <see cref="DataRequest"/> for the current paging, sorting, and filtering state.
    /// This method packages all current component state into a request object that can be
    /// used by data providers to fetch the appropriate data subset.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the request,
    /// allowing the data operation to be cancelled if needed.</param>
    /// <returns>A new <see cref="DataRequest"/> instance containing current paging, sorting, and filtering parameters.</returns>
    public virtual DataRequest CreateDataRequest(CancellationToken cancellationToken = default)
    {
        DataSort[] sorts = _currentSort != null ? [_currentSort] : [];
        return new DataRequest(Pager.Page, Pager.PageSize, sorts, RootQuery, cancellationToken);
    }

    /// <summary>
    /// Default data provider used when <see cref="Data"/> or <see cref="DataLoader"/> is set directly.
    /// This provider handles client-side operations including filtering, sorting, and paging
    /// for scenarios where all data is available locally rather than being fetched from an external source.
    /// </summary>
    /// <param name="request">The data request containing paging, sorting, and filtering parameters.</param>
    /// <returns>A <see cref="DataResult{TItem}"/> containing the filtered, sorted, and paged data subset.</returns>
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
    /// This method implements standard skip/take paging logic to return only the items
    /// for the requested page. Override this method to implement custom paging behavior.
    /// </summary>
    /// <param name="queryable">The queryable data to page.</param>
    /// <param name="request">The data request containing paging parameters.</param>
    /// <returns>The paged queryable data containing only items for the requested page.</returns>
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
    /// This method processes the sort specifications in the request and applies them
    /// to the queryable using LINQ dynamic sorting extensions.
    /// </summary>
    /// <param name="queryable">The queryable data to sort.</param>
    /// <param name="request">The data request containing sort specifications.</param>
    /// <returns>The sorted queryable data with the requested sort order applied.</returns>
    protected virtual IQueryable<TItem> SortData(IQueryable<TItem> queryable, DataRequest request)
    {
        if (!Sortable || request.Sorts == null || request.Sorts.Length == 0)
            return queryable;

        return queryable.Sort(request.Sorts);
    }

    /// <summary>
    /// Applies filtering to the queryable data. Override to implement custom filtering.
    /// This method processes the query filters and converts them to LINQ expressions
    /// that are applied to the queryable data source.
    /// </summary>
    /// <param name="queryable">The queryable data to filter.</param>
    /// <param name="request">The data request containing filter specifications.</param>
    /// <returns>The filtered queryable data containing only items matching the filter criteria.</returns>
    protected virtual IQueryable<TItem> FilterData(IQueryable<TItem> queryable, DataRequest request)
    {
        if (!Filterable || !LinqExpressionBuilder.IsValid(RootQuery))
            return queryable;

        return queryable.Filter(request.Query);
    }

    /// <summary>
    /// Handles property changes in the pager state and triggers data refresh when page or page size changes.
    /// This method ensures that the component automatically refreshes its data display
    /// when users navigate between pages or change the page size.
    /// </summary>
    /// <param name="sender">The object that raised the PropertyChanged event (typically the <see cref="Pager"/>).</param>
    /// <param name="e">The property changed event arguments containing the name of the changed property.</param>
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

