// Ignore Spelling: queryable Groupable Deselect

using System.Text;

using LoreSoft.Blazor.Controls.Extensions;
using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;

// ReSharper disable once CheckNamespace
namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents a data grid component for displaying, sorting, filtering, grouping, selecting, and exporting tabular data in Blazor.
/// This component extends <see cref="DataComponentBase{TItem}"/> to provide a comprehensive data grid experience
/// with advanced features such as column management, multi-column sorting, row selection, detail views, grouping,
/// quick search functionality, and CSV export capabilities.
/// </summary>
/// <typeparam name="TItem">The type of the data item that will be displayed in the grid rows.</typeparam>
[CascadingTypeParameter(nameof(TItem))]
public partial class DataGrid<TItem> : DataComponentBase<TItem>
{
    private readonly HashSet<TItem> _expandedItems = [];
    private readonly HashSet<string> _expandedGroups = [];

    private BreakpointProvider? _breakpointProvider;

    /// <summary>
    /// Gets or sets the template for defining data columns.
    /// This template should contain <see cref="DataColumn{TItem}"/> components that define
    /// the structure, formatting, and behavior of each column in the grid.
    /// </summary>
    [Parameter]
    public RenderFragment? DataColumns { get; set; }

    /// <summary>
    /// Gets or sets the template for rendering detail rows.
    /// When specified, each data row can be expanded to show additional details.
    /// The template receives the data item as its context, allowing for rich detail displays.
    /// An expand/collapse button is automatically added to each row when this template is provided.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem>? DetailTemplate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether row selection is enabled.
    /// When true, adds checkboxes to each row and a "select all" checkbox to the header,
    /// allowing users to select individual rows or all visible rows at once.
    /// </summary>
    [Parameter]
    public bool Selectable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether grouping is enabled.
    /// When true, allows columns to be configured for grouping, which organizes data
    /// into collapsible groups based on column values. Groups can be expanded/collapsed individually.
    /// </summary>
    [Parameter]
    public bool Groupable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the grid should display borders.
    /// When true, adds border styling to the grid table and cells for better visual separation.
    /// </summary>
    [Parameter]
    public Borders Borders { get; set; } = Borders.Horizontal;

    /// <summary>
    /// Gets or sets the CSS class for the table element.
    /// This class is applied to the main grid container and can be used for custom styling.
    /// </summary>
    [Parameter]
    public string? GridClass { get; set; }

    /// <summary>
    /// Gets or sets the CSS class for each row.
    /// This class is applied to all data rows in addition to any computed row classes
    /// (such as selection or expansion state classes).
    /// </summary>
    [Parameter]
    public string? RowClass { get; set; }

    /// <summary>
    /// Gets or sets a function to compute the CSS style for each row.
    /// The function receives the data item and returns a CSS style string that will be
    /// applied to the row element. Useful for conditional row styling based on data values.
    /// </summary>
    [Parameter]
    public Func<TItem, string>? RowStyle { get; set; }

    /// <summary>
    /// Gets or sets a function to compute additional attributes for each row.
    /// The function receives the data item and returns a dictionary of HTML attributes
    /// that will be applied to the row element. Useful for adding data attributes, ARIA labels, or event handlers.
    /// </summary>
    [Parameter]
    public Func<TItem, Dictionary<string, object>>? RowAttributes { get; set; }

    /// <summary>
    /// Gets or sets the collection of selected items.
    /// This property maintains the current selection state and can be used for two-way binding.
    /// Changes to this collection will update the grid's selection display.
    /// </summary>
    [Parameter]
    public IEnumerable<TItem> SelectedItems { get; set; } = [];

    /// <summary>
    /// Gets or sets the callback invoked when the selected items change.
    /// This callback is triggered whenever items are selected or deselected,
    /// enabling two-way binding with the <see cref="SelectedItems"/> property.
    /// </summary>
    [Parameter]
    public EventCallback<IEnumerable<TItem>> SelectedItemsChanged { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a row is double-clicked.
    /// The callback receives the data item from the double-clicked row,
    /// allowing for actions like opening detail views or edit dialogs.
    /// </summary>
    [Parameter]
    public EventCallback<TItem> RowDoubleClick { get; set; }

    /// <summary>
    /// Gets or sets the key used to persist the grid's state.
    /// This key is used to store and retrieve the grid's state from a storage service,
    /// enabling state persistence across page reloads or navigation.
    /// </summary>
    [Parameter]
    public string? StateKey { get; set; }

    /// <summary>
    /// Gets or sets the storage type used to persist the grid's state. This property determines
    /// whether the state is stored in local storage or session storage. Default is local storage.
    /// </summary>
    [Parameter]
    public StoreType StateStore { get; set; } = StoreType.Local;

    /// <summary>
    /// Provides access to the current breakpoint information.
    /// </summary>
    [CascadingParameter]
    private BreakpointProvider? BreakpointProvider { get; set; }

    /// <summary>
    /// Gets the list of columns defined for the grid.
    /// This collection is populated by <see cref="DataColumn{TItem}"/> components
    /// declared within the <see cref="DataColumns"/> template. It provides access
    /// to column configuration, state, and metadata for the grid's functionality.
    /// </summary>
    public List<DataColumn<TItem>> Columns { get; } = [];

    /// <summary>
    /// Gets a value indicating whether the column picker panel is open.
    /// The column picker allows users to show/hide columns that have <see cref="DataColumn{TItem}.Hideable"/> set to true.
    /// This property can be used to coordinate column picker state with other UI elements.
    /// </summary>
    protected bool ColumnPickerOpen { get; set; }

    /// <summary>
    /// Gets or sets the active tab in the column picker panel.
    /// Valid values are <c>"columns"</c> (column visibility) and <c>"sort"</c> (sort configuration).
    /// </summary>
    protected string ColumnPickerTab { get; set; } = "columns";

    /// <summary>
    /// Gets the list of sort entries currently configured in the column picker sort tab.
    /// Each entry represents one sort column and direction in priority order (index 0 = highest priority).
    /// </summary>
    protected List<DataSortState> SortEntries { get; private set; } = [];

    /// <summary>
    /// Adds a new empty sort entry to the column picker sort list.
    /// </summary>
    protected void AddSortEntry()
    {
        SortEntries.Add(new DataSortState());
    }

    /// <summary>
    /// Removes the specified sort entry and applies the updated sort configuration to the grid.
    /// </summary>
    /// <param name="entry">The sort entry to remove.</param>
    protected Task RemoveSortEntryAsync(DataSortState entry)
    {
        SortEntries.Remove(entry);
        return ApplySortEntriesAsync();
    }

    /// <summary>
    /// Applies all current sort entries to the grid columns and refreshes the data.
    /// Entries with no column selected are skipped. List order determines sort priority.
    /// </summary>
    protected Task ApplySortEntriesAsync()
    {
        Columns.ForEach(c => c.UpdateSort(-1, false));

        var index = 0;
        foreach (var entry in SortEntries.Where(e => !string.IsNullOrEmpty(e.ColumnName)))
        {
            var col = Columns.Find(c => c.ColumnName == entry.ColumnName);
            if (col == null || !col.Sortable)
                continue;

            col.UpdateSort(index++, entry.Direction == "desc");
        }

        return RefreshAsync();
    }

    /// <summary>
    /// Shows the column picker panel.
    /// Opens the interface that allows users to toggle the visibility of hideable columns.
    /// This method triggers a UI update to display the column picker interface.
    /// </summary>
    public void ShowColumnPicker()
    {
        ColumnPickerTab = "columns";
        UpdateSortPickerState();

        ColumnPickerOpen = true;
        StateHasChanged();
    }

    /// <summary>
    /// Closes the column picker panel.
    /// Hides the column visibility interface without affecting the current column visibility state.
    /// Column visibility changes made while the picker was open remain in effect.
    /// </summary>
    public void CloseColumnPicker()
    {
        ColumnPickerOpen = false;
        StateHasChanged();
    }

    /// <summary>
    /// Toggles the column picker panel open or closed.
    /// If the panel is currently open, it will be closed and vice versa.
    /// This provides a convenient single method for column picker state management.
    /// </summary>
    public void ToggleColumnPicker()
    {
        if (!ColumnPickerOpen)
            UpdateSortPickerState();

        ColumnPickerOpen = !ColumnPickerOpen;
        StateHasChanged();
    }


    /// <summary>
    /// Performs a quick search on all filterable string columns.
    /// This method creates a logical OR filter across all string columns that have
    /// <see cref="DataColumn{TItem}.Filterable"/> set to true, allowing users to quickly
    /// find rows containing the search text in any searchable column.
    /// </summary>
    /// <param name="searchText">The text to search for across all filterable string columns.
    /// If null or empty, any existing quick search filters are removed.</param>
    /// <param name="clearFilter">When true, clears all existing filters before applying the search.
    /// When false, only removes previous quick search filters while preserving other filters.</param>
    /// <returns>A task representing the asynchronous search operation and grid refresh.</returns>
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
            var columns = Columns
                .Where(c => c.Filterable && c.Searchable && c.PropertyType == typeof(string))
                .DistinctBy(p => p.ColumnName);

            foreach (var column in columns)
            {
                var filter = new QueryFilter
                {
                    Field = IsLocalProvider ? column.PropertyName : column.ColumnName,
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
    /// <remarks>
    /// This override clears row-specific state (expanded items and selected items) before refreshing
    /// to ensure a clean state when data changes. This prevents stale references to items that
    /// may no longer exist in the new data set.
    /// </remarks>
    public override async Task RefreshAsync(bool resetPager = false, bool forceReload = false)
    {
        // clear row flags on refresh
        _expandedItems.Clear();
        SetSelectedItems([]);

        await base.RefreshAsync(resetPager, forceReload);

        // resetPager=true = user-initiated change
        if (StateKey != null && resetPager)
            await SaveStateAsync();
    }

    /// <summary>
    /// Sorts the grid by the specified column.
    /// This method provides column-aware sorting that updates the column's sort state
    /// and visual indicators. When sorting by a new column, all other columns are cleared of sort state.
    /// </summary>
    /// <param name="column">The column to sort by. Must be a valid column from the <see cref="Columns"/> collection.</param>
    /// <param name="descending">The sort direction. If null, toggles the current direction or defaults to ascending.</param>
    /// <returns>A task representing the asynchronous sort operation and grid refresh.</returns>
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
    /// This override provides column name-based sorting by finding the matching column
    /// and delegating to the column-based sort method.
    /// </summary>
    /// <param name="columnName">The name of the column to sort by. Should match <see cref="DataColumn{TItem}.ColumnName"/>.</param>
    /// <param name="descending">The sort direction. If null, toggles the current direction or defaults to ascending.</param>
    /// <returns>A task representing the asynchronous sort operation and grid refresh.</returns>
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
    /// This method exports all data that matches the current filter criteria, bypassing pagination
    /// to include the complete filtered dataset. Only columns with <see cref="DataColumn{TItem}.Exportable"/>
    /// set to true are included in the export.
    /// </summary>
    /// <param name="fileName">The name of the file to download. If null, generates a timestamped filename.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the export operation.</param>
    /// <returns>A task representing the asynchronous export operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no valid data provider is available.</exception>
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
    /// This filtered collection includes only columns that have <see cref="DataColumn{TItem}.CurrentVisible"/>
    /// set to true, which is useful for rendering operations and layout calculations.
    /// </summary>
    protected List<DataColumn<TItem>> VisibleColumns => Columns.Where(c => c.CurrentVisible).ToList();

    /// <summary>
    /// Gets the total number of cells per row, including detail and selection columns.
    /// This count includes data columns, plus additional columns for detail expansion,
    /// grouping controls, and selection checkboxes. Used for colspan calculations in templates.
    /// </summary>
    protected int CellCount => (Columns?.Count(c => c.CurrentVisible) ?? 0)
        + (DetailTemplate != null || (Groupable && Columns?.Any(c => c.Grouping) == true) ? 1 : 0)
        + (Selectable ? 1 : 0);

    /// <summary>
    /// Gets a value indicating whether grouping is enabled and any column is grouped.
    /// This property combines the global <see cref="Groupable"/> setting with the actual
    /// presence of grouped columns to determine if group rendering should be active.
    /// </summary>
    protected bool HasGrouping => Groupable && Columns.Any(c => c.Grouping);

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && (Columns == null || Columns.Count == 0)) // verify columns added
            throw new InvalidOperationException("DataGrid requires at least one DataColumn child component.");

        // restore state first
        if (firstRender && StateKey != null)
            await LoadStateAsync();

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();

        _breakpointProvider = BreakpointProvider;
        if (_breakpointProvider == null)
            return;

        _breakpointProvider.Subscribe(OnBreakpointChanged);
    }

    /// <summary>
    /// Determines whether the specified row is expanded.
    /// Expansion state is tracked per data item instance, allowing users to
    /// expand/collapse detail views for individual rows when <see cref="DetailTemplate"/> is provided.
    /// </summary>
    /// <param name="item">The data item to check for expansion state.</param>
    /// <returns>True if the row is currently expanded; otherwise, false.</returns>
    protected bool IsRowExpanded(TItem item)
    {
        return _expandedItems.Contains(item);
    }

    /// <summary>
    /// Toggles the expanded state of the specified detail row.
    /// When a row is expanded, its detail template is rendered below the main row content.
    /// This method maintains the expansion state and triggers a UI update.
    /// </summary>
    /// <param name="item">The data item whose expansion state should be toggled.</param>
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
    /// Group expansion is tracked by group key, allowing individual groups
    /// to be expanded or collapsed independently when grouping is enabled.
    /// </summary>
    /// <param name="key">The unique key identifying the group.</param>
    /// <returns>True if the group is currently expanded; otherwise, false.</returns>
    protected bool IsGroupExpanded(string key)
    {
        return _expandedGroups.Contains(key);
    }

    /// <summary>
    /// Toggles the expanded state of the specified group row.
    /// When a group is expanded, its member rows are visible. When collapsed,
    /// only the group header is shown. This method maintains group state and triggers a UI update.
    /// </summary>
    /// <param name="key">The unique key identifying the group to toggle.</param>
    protected void ToggleGroupRow(string key)
    {
        if (_expandedGroups.Contains(key))
            _expandedGroups.Remove(key);
        else
            _expandedGroups.Add(key);

        StateHasChanged();
    }

    /// <summary>
    /// Determines whether all rows in the current view are selected.
    /// This method checks all visible rows to determine the state of the "select all" checkbox.
    /// Returns false if there are no visible rows or if any row is not selected.
    /// </summary>
    /// <returns>True if all visible rows are selected; otherwise, false.</returns>
    protected bool IsAllSelected()
    {
        if (View == null || View.Count == 0)
            return false;

        return View.All(IsRowSelected);
    }

    /// <summary>
    /// Toggles selection of all rows in the current view.
    /// If all rows are currently selected, deselects all rows. Otherwise, selects all rows.
    /// This method is typically called by the "select all" checkbox in the grid header.
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
    /// This method checks the current <see cref="SelectedItems"/> collection to determine
    /// if the given item is selected, handling null collections gracefully.
    /// </summary>
    /// <param name="item">The data item to check for selection state.</param>
    /// <returns>True if the row is currently selected; otherwise, false.</returns>
    protected bool IsRowSelected(TItem item)
    {
        return SelectedItems?.Contains(item) ?? false;
    }

    /// <summary>
    /// Toggles selection of the specified row.
    /// If the row is currently selected, it will be deselected. If not selected, it will be selected.
    /// This method maintains the selection state and invokes the selection changed callback.
    /// </summary>
    /// <param name="item">The data item whose selection state should be toggled.</param>
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
    /// This method adds all visible rows to the selection, typically used by the "select all" functionality.
    /// Only rows in the current view (after paging/filtering) are selected.
    /// </summary>
    protected void SelectAll()
    {
        var list = View?.ToList() ?? [];
        SetSelectedItems(list);
        StateHasChanged();
    }

    /// <summary>
    /// Deselects all rows.
    /// This method clears the entire selection, removing all items from <see cref="SelectedItems"/>.
    /// The selection changed callback is invoked with an empty collection.
    /// </summary>
    protected void DeselectAll()
    {
        SetSelectedItems(new List<TItem>());
        StateHasChanged();
    }

    /// <summary>
    /// Sets the selected items and invokes the selection changed callback.
    /// This method updates the internal selection state and notifies consumers
    /// of the selection change through the <see cref="SelectedItemsChanged"/> callback.
    /// </summary>
    /// <param name="items">The collection of items to set as selected.</param>
    protected async void SetSelectedItems(IEnumerable<TItem> items)
    {
        SelectedItems = items;
        await SelectedItemsChanged.InvokeAsync(SelectedItems);
    }

    /// <summary>
    /// Toggles the visibility of the specified column.
    /// This method updates the column's visibility state and triggers a UI update.
    /// Only columns with <see cref="DataColumn{TItem}.Hideable"/> set to true should use this method.
    /// </summary>
    /// <param name="column">The column whose visibility should be toggled.</param>
    protected async Task ToggleVisible(DataColumn<TItem> column)
    {
        var value = !column.CurrentVisible;
        column.UpdateVisible(value);

        StateHasChanged();

        await SaveStateAsync();
    }

    /// <summary>
    /// Adds a column to the grid.
    /// This method is called internally by <see cref="DataColumn{TItem}"/> components during initialization
    /// to register themselves with their parent grid. It should not be called directly by user code.
    /// </summary>
    /// <param name="column">The column to add to the grid's column collection.</param>
    internal void AddColumn(DataColumn<TItem> column)
    {
        Columns.Add(column);
    }

    /// <summary>
    /// Creates a data request for loading, sorting, and filtering data.
    /// This override includes multi-column sorting support by collecting sort information
    /// from all columns with active sort indices and creating appropriate <see cref="DataSort"/> objects.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the data request operation.</param>
    /// <returns>A <see cref="DataRequest"/> containing current paging, multi-column sorting, and filtering parameters.</returns>
    public override DataRequest CreateDataRequest(CancellationToken cancellationToken = default)
    {
        var sorts = Columns
            .Where(c => c.CurrentSortIndex >= 0)
            .OrderBy(c => c.CurrentSortIndex)
            .Select(c => new DataSort(c.ColumnName, c.CurrentSortDescending))
            .ToArray();

        return new DataRequest
        {
            Page = Pager.Page,
            PageSize = Pager.PageSize,
            ContinuationToken = Pager.ContinuationToken,
            Sorts = sorts,
            Query = RootQuery,
            CancellationToken = cancellationToken
        };
    }

    /// <summary>
    /// Applies filtering to the data source based on the current query.
    /// This method delegates to the base implementation but provides grid-specific context
    /// for filtering operations, ensuring that grid-specific filter logic is properly applied.
    /// </summary>
    /// <param name="queryable">The queryable data source to filter.</param>
    /// <param name="request">The data request containing filter specifications.</param>
    /// <returns>The filtered queryable data source containing only items matching the filter criteria.</returns>
    protected override IQueryable<TItem> FilterData(IQueryable<TItem> queryable, DataRequest request)
    {
        if (!Filterable || !LinqExpressionBuilder.IsValid(RootQuery))
            return queryable;

        return queryable.Filter(request.Query);
    }

    /// <summary>
    /// Applies sorting to the data source based on the current sort settings.
    /// This override provides column-aware sorting that uses the actual column property expressions
    /// rather than string-based property names, ensuring type safety and supporting complex property paths.
    /// </summary>
    /// <param name="queryable">The queryable data source to sort.</param>
    /// <param name="request">The data request containing sort specifications.</param>
    /// <returns>The sorted queryable data source with the requested sort order applied.</returns>
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

    /// <summary>
    /// Computes the CSS class for the grid container.
    /// This method combines the base grid classes with optional border styling and custom grid classes.
    /// </summary>
    /// <returns>The computed CSS class string for the grid container.</returns>
    private string? ComputeGridClass()
    {
        return CssBuilder.Pool.Use(builder =>
        {
            return builder
                .AddClass("data-grid")
                .AddClass("data-grid--horizontal-borders", Borders is Borders.Horizontal or Borders.All)
                .AddClass("data-grid--vertical-borders", Borders is Borders.Vertical or Borders.All)
                .AddClass(GridClass)
                .ToString();
        });
    }

    /// <summary>
    /// Computes the CSS class for a data row.
    /// This method combines the base row class with state-specific classes for selection and expansion.
    /// </summary>
    /// <param name="item">The data item for the row.</param>
    /// <returns>The computed CSS class string for the row.</returns>
    private string? ComputeRowClass(TItem item)
    {
        return CssBuilder.Pool.Use(builder =>
        {
            return builder
                .AddClass("data-grid__row")
                .AddClass("data-grid__row--selected", IsRowSelected(item))
                .AddClass("data-grid__row--expanded", IsRowExpanded(item))
                .ToString();
        });
    }

    /// <summary>
    /// Computes the CSS class for a group header row.
    /// This method combines the base group header class with expansion state classes.
    /// </summary>
    /// <param name="key">The unique key identifying the group.</param>
    /// <returns>The computed CSS class string for the group header.</returns>
    private string? ComputeGroupClass(string key)
    {
        var isGroupExpanded = IsGroupExpanded(key);

        return CssBuilder.Pool.Use(builder =>
        {
            return builder
                .AddClass("data-grid__group-header")
                .AddClass("ddata-grid__group-header--expanded", isGroupExpanded)
                .AddClass("data-grid__group-header--collapsed", !isGroupExpanded)
                .ToString();
        });
    }

    /// <summary>
    /// Handles changes to the current breakpoint.
    /// </summary>
    /// <param name="breakpointChanged">The breakpoint change event.</param>
    private void OnBreakpointChanged(BreakpointChanged breakpointChanged)
    {
        if (breakpointChanged == null || string.IsNullOrWhiteSpace(breakpointChanged.Current))
            return;

        if (!Enum.TryParse(breakpointChanged.Current, true, out Breakpoints breakpoint))
            return;

        foreach (var column in Columns)
        {
            // skip columns without breakpoint
            if (!column.Breakpoint.HasValue)
                continue;

            // visible if current breakpoint is >= column breakpoint
            var visible = breakpoint >= column.Breakpoint.Value;

            // update only if changed
            if (column.CurrentVisible != visible)
                column.UpdateVisible(visible);
        }

        InvokeAsync(StateHasChanged);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        _breakpointProvider?.Unsubscribe(OnBreakpointChanged);

        base.Dispose();

        GC.SuppressFinalize(this);
    }

    private (int, bool)? _hasFooter;

    private bool HasFooter()
    {
        // cache footer check since columns don't change often
        if (_hasFooter != null && Columns.Count == _hasFooter.Value.Item1)
            return _hasFooter.Value.Item2;

        var count = Columns.Count;
        var hasFooter = Columns.Any(c => c.FooterTemplate != null);

        _hasFooter = (count, hasFooter);

        return hasFooter;
    }

    private void UpdateSortPickerState()
    {
        SortEntries = Columns
            .Where(c => c.CurrentSortIndex >= 0)
            .OrderBy(c => c.CurrentSortIndex)
            .Select(c => new DataSortState { ColumnName = c.ColumnName, Direction = c.CurrentSortDescending ? "desc" : "asc" })
            .ToList();

        if (SortEntries.Count == 0)
            SortEntries.Add(new DataSortState());
    }

    private async Task ResetStateAsync(
        bool resetFilter = true,
        bool resetVisible = true,
        bool resetSort = true)
    {
        if (!string.IsNullOrWhiteSpace(StateKey))
            await StorageService.RemoveItemAsync(StateKey, StateStore);

        if (!resetVisible && !resetSort && !resetFilter)
            return;

        // reset filters to defaults
        if (resetFilter)
            await ResetQueryAsync();

        // reset column sort + visibility to defaults
        foreach (var column in Columns)
        {
            if (resetSort)
                column.UpdateSort(column.SortIndex, column.SortDescending);

            if (resetVisible)
                column.UpdateVisible(column.Visible);
        }

        // refresh sort picker state if needed
        UpdateSortPickerState();

        StateHasChanged();
    }

    private async Task LoadStateAsync()
    {
        if (string.IsNullOrWhiteSpace(StateKey))
            return;

        var state = await StorageService.GetItemAsync<DataGridState>(StateKey, StateStore);
        if (state is null)
            return;

        // restore filters
        if (state.Query?.Filters.Count > 0)
        {
            RootQuery.Filters.Clear();
            RootQuery.Filters.AddRange(state.Query.Filters);
        }

        if (state.Columns is not { Length: > 0 })
            return;

        // restore column sort + visibility
        foreach (var cs in state.Columns)
        {
            var col = Columns.Find(c => c.PropertyName == cs.PropertyName);
            if (col is null)
                continue;

            col.UpdateSort(cs.SortIndex, cs.SortDescending);
            col.UpdateVisible(cs.Visible);
        }
    }

    private async Task SaveStateAsync()
    {
        if (string.IsNullOrWhiteSpace(StateKey))
            return;

        var columns = Columns
            .Select(c => new DataColumnState(c.PropertyName, c.CurrentSortIndex, c.CurrentSortDescending, c.CurrentVisible))
            .ToArray();

        var state = new DataGridState(RootQuery, columns);

        await StorageService.SetItemAsync(StateKey, state, StateStore);
    }

    
}
