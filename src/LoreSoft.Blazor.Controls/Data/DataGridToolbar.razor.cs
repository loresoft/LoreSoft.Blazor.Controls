using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Provides a comprehensive toolbar for <see cref="DataGrid{TItem}"/> components in Blazor applications.
/// The toolbar includes integrated support for search functionality, filtering controls, data export capabilities,
/// grid refresh operations, and column visibility management. It can be used as a standalone component
/// or integrated seamlessly with DataGrid components through cascading parameters.
/// </summary>
/// <typeparam name="TItem">The type of the data item displayed in the associated DataGrid.
/// Must be a reference type to ensure proper equality comparisons and state management.</typeparam>
public partial class DataGridToolbar<TItem> : ComponentBase, IDisposable
    where TItem : class
{
    /// <summary>
    /// The key used to store the current search text inside <see cref="DataGridState.Extensions"/>
    /// when persisting toolbar state alongside the grid state.
    /// </summary>
    private const string SearchTextKey = "toolbar-search";
    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridToolbar{TItem}"/> class.
    /// Sets up the debounced search functionality with default timing to provide
    /// responsive search behavior without overwhelming the data source with requests.
    /// </summary>
    public DataGridToolbar()
    {
        SearchText = new DebounceValue<string>(HandleSearch);
    }

    /// <summary>
    /// Gets or sets the JavaScript runtime for interop calls.
    /// This service is used for client-side operations such as DOM manipulation,
    /// focus management, and other browser-specific functionality required by the toolbar.
    /// </summary>
    [Inject]
    public required IJSRuntime JavaScript { get; set; }

    /// <summary>
    /// Gets or sets the parent <see cref="DataGrid{TItem}"/> component from the cascading parameter.
    /// Mutually exclusive with <see cref="DataGrid"/>.
    /// This parameter is automatically populated when the toolbar is placed within a DataGrid's template,
    /// enabling automatic integration without explicit configuration.
    /// </summary>
    [CascadingParameter(Name = "Grid")]
    protected DataGrid<TItem>? ParentGrid { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="DataGrid{TItem}"/> instance to operate on.
    /// Mutually exclusive with <see cref="ParentGrid"/>.
    /// Use this parameter when the toolbar is used outside of a DataGrid template
    /// or when you need to explicitly specify which grid instance to control.
    /// </summary>
    [Parameter]
    public DataGrid<TItem>? DataGrid { get; set; }

    /// <summary>
    /// Gets or sets the template for the title section of the toolbar.
    /// When specified, this template replaces the default title display and receives
    /// the current DataGrid instance as context, allowing for dynamic title content
    /// based on grid state, data, or other properties.
    /// </summary>
    [Parameter]
    public RenderFragment<DataGrid<TItem>>? TitleTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for the filter section of the toolbar.
    /// This template is rendered in the filter area and receives the current DataGrid
    /// instance as context. Use this to provide custom filter controls beyond the
    /// standard search box and filter toggle button.
    /// </summary>
    [Parameter]
    public RenderFragment<DataGrid<TItem>>? FilterTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for the action section of the toolbar.
    /// This template is rendered in the action area and receives the current DataGrid
    /// instance as context. Use this to add custom action buttons, dropdowns, or
    /// other interactive elements specific to your application's needs.
    /// </summary>
    [Parameter]
    public RenderFragment<DataGrid<TItem>>? ActionTemplate { get; set; }

    /// <summary>
    /// Gets or sets the toolbar title text.
    /// This title is displayed prominently in the toolbar header and can be used
    /// to identify the purpose or context of the data being displayed.
    /// If not specified, no title will be shown unless a <see cref="TitleTemplate"/> is provided.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the base name for exported files.
    /// This name is used as the prefix for CSV export files, with a timestamp automatically
    /// appended to ensure unique filenames. If not specified, falls back to <see cref="Title"/>
    /// or a default "Export" prefix.
    /// </summary>
    [Parameter]
    public string? ExportName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the search box is displayed in the toolbar.
    /// When true, provides a debounced search input that performs quick searches across
    /// all filterable string columns in the associated DataGrid. The search uses contains
    /// logic with OR operations across columns.
    /// </summary>
    [Parameter]
    public bool ShowSearch { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the filter toggle button is displayed in the toolbar.
    /// When true, provides a button that opens/closes the DataGrid's filter panel,
    /// allowing users to configure advanced filtering options beyond the basic search functionality.
    /// </summary>
    [Parameter]
    public bool ShowFilter { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the export button is displayed in the toolbar.
    /// When true, provides a button that exports the current DataGrid data (filtered but unpaged)
    /// to a CSV file. Only exportable columns are included in the export.
    /// </summary>
    [Parameter]
    public bool ShowExport { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the refresh button is displayed in the toolbar.
    /// When true, provides a button that forces a complete refresh of the DataGrid data,
    /// clearing any cached data and reloading from the original data source.
    /// </summary>
    [Parameter]
    public bool ShowRefresh { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the column picker button is displayed in the toolbar.
    /// When true, provides a button that opens the column visibility panel, allowing users
    /// to show/hide columns that have been marked as hideable in the DataGrid configuration.
    /// </summary>
    [Parameter]
    public bool ShowColumnPicker { get; set; } = true;

    /// <summary>
    /// Gets the current <see cref="DataGrid{TItem}"/> instance in use by the toolbar.
    /// This property is derived from either the <see cref="DataGrid"/> parameter or the
    /// cascading <see cref="ParentGrid"/> parameter, providing a unified way to access
    /// the associated grid regardless of how the toolbar was configured.
    /// </summary>
    protected DataGrid<TItem>? CurrentGrid { get; set; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // snapshot the grid reference from the previous render so we can detect a change
        var previousGrid = CurrentGrid;

        // explicit DataGrid parameter takes priority over the cascading parent
        CurrentGrid = DataGrid ?? ParentGrid;

        // rewire state events only when the grid instance actually changed; avoids
        // redundant subscribe/unsubscribe on every parameter update cycle
        if (previousGrid == CurrentGrid)
            return;

        // unsubscribe from the old grid to prevent memory leaks and stale callbacks
        if (previousGrid != null)
        {
            previousGrid.StateSaving -= HandleStateSaving;
            previousGrid.StateLoaded -= HandleStateLoaded;
            previousGrid.StateResetting -= HandleStateResetting;
        }

        // subscribe to the new grid so the toolbar can persist and restore its own state
        if (CurrentGrid != null)
        {
            CurrentGrid.StateSaving += HandleStateSaving;
            CurrentGrid.StateLoaded += HandleStateLoaded;
            CurrentGrid.StateResetting += HandleStateResetting;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (CurrentGrid != null)
        {
            CurrentGrid.StateSaving -= HandleStateSaving;
            CurrentGrid.StateLoaded -= HandleStateLoaded;
            CurrentGrid.StateResetting -= HandleStateResetting;
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Gets the debounced value wrapper for the search text input.
    /// This wrapper automatically delays search execution to avoid overwhelming the data source
    /// with requests during rapid typing. The default delay provides a balance between
    /// responsiveness and performance.
    /// </summary>
    protected DebounceValue<string> SearchText { get; }

    /// <summary>
    /// Handles the search action by invoking quick search on the associated DataGrid.
    /// This method is called automatically by the debounced search input after the
    /// configured delay period. It performs a contains-based search across all
    /// filterable string columns using OR logic.
    /// </summary>
    /// <param name="searchText">The search text entered by the user.
    /// If null or empty, any existing quick search filters are removed.</param>
    protected void HandleSearch(string? searchText)
    {
        if (CurrentGrid == null)
            return;

        InvokeAsync(() => CurrentGrid.QuickSearch(searchText));
    }

    /// <summary>
    /// Handles the refresh action by forcing a complete refresh of the DataGrid data.
    /// This method clears any cached data and reloads from the original data source,
    /// ensuring that the most current data is displayed. This is particularly useful
    /// for data that may have been modified externally.
    /// </summary>
    /// <returns>A task representing the asynchronous refresh operation.</returns>
    protected async Task HandelRefresh()
    {
        if (CurrentGrid == null)
            return;

        await CurrentGrid.RefreshAsync(forceReload: true);
    }

    /// <summary>
    /// Handles the export action by generating and downloading a CSV file containing the current DataGrid data.
    /// This method exports all data that matches the current filter criteria, bypassing pagination
    /// to include the complete filtered dataset. Only columns marked as exportable are included.
    /// The filename is automatically generated with a timestamp to ensure uniqueness.
    /// </summary>
    /// <returns>A task representing the asynchronous export operation.</returns>
    protected async Task HandelExport()
    {
        if (CurrentGrid == null)
            return;

        var name = ExportName ?? Title ?? "Export";
        var fileName = $"{name} {DateTime.Now:yyyy-MM-dd-HH-mm-ss}.csv";
        await CurrentGrid.ExportAsync(fileName);
    }

    /// <summary>
    /// Toggles the visibility of the filter panel on the associated DataGrid.
    /// This method opens the filter panel if it's currently closed, or closes it if open.
    /// The filter panel provides advanced filtering capabilities beyond the basic search functionality,
    /// allowing users to create complex filter conditions across multiple columns.
    /// </summary>
    protected void ToggleFilter()
    {
        if (CurrentGrid == null)
            return;

        CurrentGrid.ToggleFilter();
    }

    /// <summary>
    /// Toggles the visibility of the column picker panel on the associated DataGrid.
    /// This method opens the column picker if it's currently closed, or closes it if open.
    /// The column picker allows users to show/hide columns that have been configured
    /// as hideable, providing control over the displayed data structure.
    /// </summary>
    protected void ToggleColumnPicker()
    {
        if (CurrentGrid == null)
            return;

        CurrentGrid.ToggleColumnPicker();
    }

    /// <summary>
    /// Handles the <see cref="DataGrid{TItem}.StateSaving"/> event by writing the current
    /// search text into <see cref="DataGridState.Extensions"/> so it is persisted with the grid state.
    /// </summary>
    /// <param name="state">The grid state being saved.</param>
    private Task HandleStateSaving(DataGridState state)
    {
        state.Extensions[SearchTextKey] = SearchText.Value;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles the <see cref="DataGrid{TItem}.StateLoaded"/> event by restoring the search text
    /// from <see cref="DataGridState.Extensions"/>. The search filter itself is already restored
    /// by the grid, so only the text box value is updated here.
    /// </summary>
    /// <param name="state">The grid state that was loaded.</param>
    private Task HandleStateLoaded(DataGridState state)
    {
        // silently update the text box; the filter is already restored by the grid
        if (state.Extensions.TryGetValue(SearchTextKey, out var text))
            SearchText.Update(text);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles the <see cref="DataGrid{TItem}.StateResetting"/> event by clearing the search
    /// text box so it reflects the reset grid state.
    /// </summary>
    private Task HandleStateResetting()
    {
        SearchText.Update(null);
        return Task.CompletedTask;
    }
}
