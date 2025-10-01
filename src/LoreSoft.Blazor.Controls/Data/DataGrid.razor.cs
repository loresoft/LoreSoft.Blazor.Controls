// Ignore Spelling: queryable Groupable Deselect

using System.Text;

using LoreSoft.Blazor.Controls.Extensions;
using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

// ReSharper disable once CheckNamespace
namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents a data grid component for displaying, sorting, filtering, grouping, selecting, and exporting tabular data in Blazor.
/// Supports advanced features such as column management, quick search, and customizable templates.
/// </summary>
/// <typeparam name="TItem">The type of the data item.</typeparam>
[CascadingTypeParameter(nameof(TItem))]
public partial class DataGrid<TItem> : DataComponentBase<TItem>
{
    private readonly HashSet<TItem> _expandedItems = [];
    private readonly HashSet<string> _expandedGroups = [];

    private QueryGroup? _initialQuery;

    /// <summary>
    /// Gets or sets the template for defining data columns.
    /// </summary>
    [Parameter]
    public RenderFragment? DataColumns { get; set; }

    /// <summary>
    /// Gets or sets the template for rendering detail rows.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem>? DetailTemplate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether row selection is enabled.
    /// </summary>
    [Parameter]
    public bool Selectable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether grouping is enabled.
    /// </summary>
    [Parameter]
    public bool Groupable { get; set; }

    /// <summary>
    /// Gets or sets the CSS class for the table element.
    /// </summary>
    [Parameter]
    public string TableClass { get; set; } = "table";

    /// <summary>
    /// Gets or sets the CSS class for each row.
    /// </summary>
    [Parameter]
    public string? RowClass { get; set; }

    /// <summary>
    /// Gets or sets a function to compute the CSS style for each row.
    /// </summary>
    [Parameter]
    public Func<TItem, string>? RowStyle { get; set; }

    /// <summary>
    /// Gets or sets a function to compute additional attributes for each row.
    /// </summary>
    [Parameter]
    public Func<TItem, Dictionary<string, object>>? RowAttributes { get; set; }

    /// <summary>
    /// Gets or sets the collection of selected items.
    /// </summary>
    [Parameter]
    public IEnumerable<TItem> SelectedItems { get; set; } = [];

    /// <summary>
    /// Gets or sets the callback invoked when the selected items change.
    /// </summary>
    [Parameter]
    public EventCallback<IEnumerable<TItem>> SelectedItemsChanged { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a row is double-clicked.
    /// </summary>
    [Parameter]
    public EventCallback<TItem> RowDoubleClick { get; set; }

    /// <summary>
    /// Gets the list of columns defined for the grid.
    /// </summary>
    public List<DataColumn<TItem>> Columns { get; } = [];

    /// <summary>
    /// Gets a value indicating whether the column picker panel is open.
    /// </summary>
    protected bool ColumnPickerOpen { get; set; }

    /// <summary>
    /// Shows the column picker panel.
    /// </summary>
    public void ShowColumnPicker()
    {
        ColumnPickerOpen = true;
        StateHasChanged();
    }

    /// <summary>
    /// Closes the column picker panel.
    /// </summary>
    public void CloseColumnPicker()
    {
        ColumnPickerOpen = false;
        StateHasChanged();
    }

    /// <summary>
    /// Toggles the column picker panel open or closed.
    /// </summary>
    public void ToggleColumnPicker()
    {
        ColumnPickerOpen = !ColumnPickerOpen;
        StateHasChanged();
    }

    /// <summary>
    /// Performs a quick search on all filterable string columns.
    /// </summary>
    /// <param name="searchText">The search text.</param>
    /// <param name="clearFilter">Whether to clear existing filters.</param>
    public async Task QuickSearch(string? searchText, bool clearFilter = false)
    {
        if (clearFilter)
            RootQuery.Filters.Clear();
        else
            RootQuery.Filters.RemoveAll(f => f.Id == nameof(QuickSearch));

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var quickSearch = new QueryGroup { Id = nameof(QuickSearch), Logic = QueryLogic.Or };


            // all filterable string columns
            foreach (var column in Columns.Where(c => c.Filterable && c.PropertyType == typeof(string)))
            {
                var filter = new QueryFilter
                {
                    Field = column.ColumnName,
                    Operator = QueryOperators.Contains,
                    Value = searchText
                };
                quickSearch.Filters.Add(filter);
            }

            RootQuery.Filters.Add(quickSearch);
        }

        await RefreshAsync(true);
    }

    /// <inheritdoc />
    public override async Task RefreshAsync(bool resetPager = false, bool forceReload = false)
    {
        // clear row flags on refresh
        _expandedItems.Clear();
        SetSelectedItems([]);

        await base.RefreshAsync(resetPager, forceReload);
    }

    /// <summary>
    /// Sorts the grid by the specified column.
    /// </summary>
    /// <param name="column">The column to sort by.</param>
    /// <param name="descending">Whether to sort in descending order.</param>
    public async Task SortByAsync(DataColumn<TItem> column, bool? descending = null)
    {
        if (column == null || !Sortable || !column.Sortable)
            return;

        descending ??= column.CurrentSortIndex >= 0 && !column.CurrentSortDescending;

        Columns.ForEach(c => c.UpdateSort(-1, false));

        column.UpdateSort(0, descending ?? false);

        await RefreshAsync();
    }

    /// <summary>
    /// Sorts the grid by the column with the specified name.
    /// </summary>
    /// <param name="columnName">The name of the column to sort by.</param>
    /// <param name="descending">Whether to sort in descending order.</param>
    public override async Task SortByAsync(string columnName, bool? descending = null)
    {
        if (!Sortable)
            return;

        var column = Columns.Find(c => c.ColumnName == columnName);
        if (column == null)
            return;

        await SortByAsync(column, descending);
    }

    /// <summary>
    /// Exports the grid data to a CSV file.
    /// </summary>
    /// <param name="fileName">The name of the file to export.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public virtual async Task ExportAsync(string? fileName = null, CancellationToken cancellationToken = default)
    {
        if (CurrentDataProvider == null)
            throw new InvalidOperationException("Invalid Data Provider");

        var request = CreateDataRequest(cancellationToken);

        // clear paging for export
        request = request with { Page = 0, PageSize = 0 };

        var result = await CurrentDataProvider(request);

        await using var memoryStream = new MemoryStream();

        await CsvWriter.WriteAsync(
            stream: memoryStream,
            headers: Columns.Where(c => c.Exportable).Select(c => c.ExportName),
            rows: result.Items,
            selector: item => Columns.Where(c => c.Exportable).Select(c => c.CellValue(item)),
            encoding: Encoding.UTF8,
            cancellationToken: cancellationToken);

        // need to reset stream position
        memoryStream.Seek(0, SeekOrigin.Begin);

        var downloadFile = fileName ?? $"Export {DateTime.Now:yyyy-MM-dd-HH-mm-ss}.csv";

        await DownloadService.DownloadFileStream(memoryStream, downloadFile, "text/csv");
    }

    /// <summary>
    /// Gets the list of currently visible columns.
    /// </summary>
    protected List<DataColumn<TItem>> VisibleColumns => Columns.Where(c => c.CurrentVisible).ToList();

    /// <summary>
    /// Gets the total number of cells per row, including detail and selection columns.
    /// </summary>
    protected int CellCount => (Columns?.Count(c => c.CurrentVisible) ?? 0)
        + (DetailTemplate != null || (Groupable && Columns?.Any(c => c.Grouping) == true) ? 1 : 0)
        + (Selectable ? 1 : 0);

    /// <summary>
    /// Gets a value indicating whether grouping is enabled and any column is grouped.
    /// </summary>
    protected bool HasGrouping => Groupable && Columns.Any(c => c.Grouping);

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && (Columns == null || Columns.Count == 0)) // verify columns added
            throw new InvalidOperationException("DataGrid requires at least one DataColumn child component.");

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // only update RootQuery if Query changed
        if (_initialQuery != Query)
        {
            _initialQuery = Query;
            RootQuery = Query ?? new QueryGroup();
        }

        RootQuery ??= new QueryGroup();
    }

    /// <summary>
    /// Determines whether the specified row is expanded.
    /// </summary>
    /// <param name="item">The data item.</param>
    /// <returns>True if the row is expanded; otherwise, false.</returns>
    protected bool IsRowExpanded(TItem item)
    {
        return _expandedItems.Contains(item);
    }

    /// <summary>
    /// Toggles the expanded state of the specified detail row.
    /// </summary>
    /// <param name="item">The data item.</param>
    protected void ToggleDetailRow(TItem item)
    {
        if (_expandedItems.Contains(item))
            _expandedItems.Remove(item);
        else
            _expandedItems.Add(item);

        StateHasChanged();
    }

    /// <summary>
    /// Determines whether the specified group is expanded.
    /// </summary>
    /// <param name="key">The group key.</param>
    /// <returns>True if the group is expanded; otherwise, false.</returns>
    protected bool IsGroupExpanded(string key)
    {
        return _expandedGroups.Contains(key);
    }

    /// <summary>
    /// Toggles the expanded state of the specified group row.
    /// </summary>
    /// <param name="key">The group key.</param>
    protected void ToggleGroupRow(string key)
    {
        if (_expandedGroups.Contains(key))
            _expandedGroups.Remove(key);
        else
            _expandedGroups.Add(key);

        StateHasChanged();
    }

    /// <summary>
    /// Determines whether all rows are selected.
    /// </summary>
    /// <returns>True if all rows are selected; otherwise, false.</returns>
    protected bool IsAllSelected()
    {
        if (View == null || View.Count == 0)
            return false;

        return View.All(IsRowSelected);
    }

    /// <summary>
    /// Toggles selection of all rows.
    /// </summary>
    protected void ToggleSelectAll()
    {
        if (IsAllSelected())
            DeselectAll();
        else
            SelectAll();
    }

    /// <summary>
    /// Determines whether the specified row is selected.
    /// </summary>
    /// <param name="item">The data item.</param>
    /// <returns>True if the row is selected; otherwise, false.</returns>
    protected bool IsRowSelected(TItem item)
    {
        return SelectedItems?.Contains(item) ?? false;
    }

    /// <summary>
    /// Toggles selection of the specified row.
    /// </summary>
    /// <param name="item">The data item.</param>
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

    /// <summary>
    /// Selects all rows in the current view.
    /// </summary>
    protected void SelectAll()
    {
        var list = View?.ToList() ?? new List<TItem>();
        SetSelectedItems(list);
        StateHasChanged();
    }

    /// <summary>
    /// Deselects all rows.
    /// </summary>
    protected void DeselectAll()
    {
        SetSelectedItems(new List<TItem>());
        StateHasChanged();
    }

    /// <summary>
    /// Sets the selected items and invokes the selection changed callback.
    /// </summary>
    /// <param name="items">The items to select.</param>
    protected async void SetSelectedItems(IEnumerable<TItem> items)
    {
        SelectedItems = items;
        await SelectedItemsChanged.InvokeAsync(SelectedItems);
    }

    /// <summary>
    /// Toggles the visibility of the specified column.
    /// </summary>
    /// <param name="column">The column to toggle.</param>
    protected void ToggleVisible(DataColumn<TItem> column)
    {
        var value = !column.CurrentVisible;
        column.UpdateVisible(value);

        StateHasChanged();
    }

    /// <summary>
    /// Adds a column to the grid.
    /// </summary>
    /// <param name="column">The column to add.</param>
    internal void AddColumn(DataColumn<TItem> column)
    {
        Columns.Add(column);
    }

    /// <summary>
    /// Creates a data request for loading, sorting, and filtering data.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The data request.</returns>
    public override DataRequest CreateDataRequest(CancellationToken cancellationToken = default)
    {
        var sorts = Columns
            .Where(c => c.CurrentSortIndex >= 0)
            .OrderBy(c => c.CurrentSortIndex)
            .Select(c => new DataSort(c.ColumnName, c.CurrentSortDescending))
            .ToArray();

        return new DataRequest(Pager.Page, Pager.PageSize, sorts, RootQuery, cancellationToken);
    }

    /// <summary>
    /// Applies filtering to the data source based on the current query.
    /// </summary>
    /// <param name="queryable">The data source.</param>
    /// <param name="request">The data request.</param>
    /// <returns>The filtered data source.</returns>
    protected override IQueryable<TItem> FilterData(IQueryable<TItem> queryable, DataRequest request)
    {
        if (!Filterable || !LinqExpressionBuilder.IsValid(RootQuery))
            return queryable;

        return queryable.Filter(request.Query);
    }

    /// <summary>
    /// Applies sorting to the data source based on the current sort settings.
    /// </summary>
    /// <param name="queryable">The data source.</param>
    /// <param name="request">The data request.</param>
    /// <returns>The sorted data source.</returns>
    protected override IQueryable<TItem> SortData(IQueryable<TItem> queryable, DataRequest request)
    {
        if (!Sortable || request.Sorts == null || request.Sorts.Length == 0)
            return queryable;

        var columns = request.Sorts.Select(s => s.Property);

        var sorted = Columns
            .Where(c => columns.Contains(c.ColumnName))
            .OrderBy(c => c.CurrentSortIndex)
            .ToList();

        if (sorted.Count == 0)
            return queryable;

        var queue = new Queue<DataColumn<TItem>>(sorted);
        var column = queue.Dequeue();

        var orderedQueryable = column.CurrentSortDescending
            ? queryable.OrderByDescending(column.Property)
            : queryable.OrderBy(column.Property);

        while (queue.Count > 0)
        {
            column = queue.Dequeue();

            orderedQueryable = column.CurrentSortDescending
                ? orderedQueryable.ThenByDescending(column.Property)
                : orderedQueryable.ThenBy(column.Property);
        }

        return orderedQueryable;
    }
}
