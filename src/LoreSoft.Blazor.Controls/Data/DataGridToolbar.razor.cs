using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Provides a toolbar for <see cref="DataGrid{TItem}"/> in Blazor, supporting search, filter, export, refresh, and column picker actions.
/// </summary>
/// <typeparam name="TItem">The type of the data item.</typeparam>
public partial class DataGridToolbar<TItem> : ComponentBase
    where TItem : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridToolbar{TItem}"/> class.
    /// </summary>
    public DataGridToolbar()
    {
        SearchText = new DebounceValue<string>(HandleSearch);
    }

    /// <summary>
    /// Gets or sets the JavaScript runtime for interop calls.
    /// </summary>
    [Inject]
    public required IJSRuntime JavaScript { get; set; }

    /// <summary>
    /// Gets or sets the parent <see cref="DataGrid{TItem}"/> component from the cascading parameter. Mutually exclusive with <see cref="DataGrid"/>.
    /// </summary>
    [CascadingParameter(Name = "Grid")]
    protected DataGrid<TItem>? ParentGrid { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="DataGrid{TItem}"/> instance to operate on. Mutually exclusive with <see cref="ParentGrid"/>.
    /// </summary>
    [Parameter]
    public DataGrid<TItem>? DataGrid { get; set; }

    /// <summary>
    /// Gets or sets the template for the title section.
    /// </summary>
    [Parameter]
    public RenderFragment<DataGrid<TItem>>? TitleTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for the filter section.
    /// </summary>
    [Parameter]
    public RenderFragment<DataGrid<TItem>>? FilterTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for the action section.
    /// </summary>
    [Parameter]
    public RenderFragment<DataGrid<TItem>>? ActionTemplate { get; set; }

    /// <summary>
    /// Gets or sets the toolbar title.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the export file name.
    /// </summary>
    [Parameter]
    public string? ExportName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the search box is shown.
    /// </summary>
    [Parameter]
    public bool ShowSearch { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the filter button is shown.
    /// </summary>
    [Parameter]
    public bool ShowFilter { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the export button is shown.
    /// </summary>
    [Parameter]
    public bool ShowExport { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the refresh button is shown.
    /// </summary>
    [Parameter]
    public bool ShowRefresh { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the column picker button is shown.
    /// </summary>
    [Parameter]
    public bool ShowColumnPicker { get; set; } = true;

    /// <summary>
    /// Gets the current <see cref="DataGrid{TItem}"/> instance in use.
    /// Derived from either the <see cref="DataGrid"/> parameter or the cascading <see cref="ParentGrid"/>.
    /// </summary>
    protected DataGrid<TItem>? CurrentGrid { get; set; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        CurrentGrid = DataGrid ?? ParentGrid;
    }

    /// <summary>
    /// Gets the debounce value for the search text.
    /// </summary>
    protected DebounceValue<string> SearchText { get; }

    /// <summary>
    /// Handles the search action by invoking quick search on the grid.
    /// </summary>
    /// <param name="searchText">The search text.</param>
    protected void HandleSearch(string? searchText)
    {
        if (CurrentGrid == null)
            return;

        InvokeAsync(() => CurrentGrid.QuickSearch(searchText));
    }

    /// <summary>
    /// Handles the refresh action by refreshing the grid.
    /// </summary>
    protected async Task HandelRefresh()
    {
        if (CurrentGrid == null)
            return;

        await CurrentGrid.RefreshAsync(forceReload: true);
    }

    /// <summary>
    /// Handles the export action by exporting the grid data to a CSV file.
    /// </summary>
    protected async Task HandelExport()
    {
        if (CurrentGrid == null)
            return;

        var name = ExportName ?? Title ?? "Export";
        var fileName = $"{name} {DateTime.Now:yyyy-MM-dd-HH-mm-ss}.csv";
        await CurrentGrid.ExportAsync(fileName);
    }

    /// <summary>
    /// Toggles the filter panel on the grid.
    /// </summary>
    protected void ToggleFilter()
    {
        if (CurrentGrid == null)
            return;

        CurrentGrid.ShowFilter();
    }

    /// <summary>
    /// Toggles the column picker panel on the grid.
    /// </summary>
    protected void ToggleColumnPicker()
    {
        if (CurrentGrid == null)
            return;

        CurrentGrid.ShowColumnPicker();
    }
}
