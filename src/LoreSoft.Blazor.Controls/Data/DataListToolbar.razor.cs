using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LoreSoft.Blazor.Controls;

public partial class DataListToolbar<TItem> : ComponentBase
    where TItem : class
{
    public DataListToolbar()
    {
        SearchText = new DebounceValue<string>(HandleSearch);
    }

    /// <summary>
    /// Gets or sets the parent <see cref="DataGrid{TItem}"/> component from the cascading parameter. Mutually exclusive with <see cref="DataList"/>.
    /// </summary>
    [CascadingParameter(Name = "Grid")]
    protected DataList<TItem>? ParentList { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="DataGrid{TItem}"/> instance to operate on. Mutually exclusive with <see cref="ParentList"/>.
    /// </summary>
    [Parameter]
    public DataList<TItem>? DataList { get; set; }

    /// <summary>
    /// Gets or sets the template for the title section.
    /// </summary>
    [Parameter]
    public RenderFragment<DataList<TItem>>? TitleTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for the filter section.
    /// </summary>
    [Parameter]
    public RenderFragment<DataList<TItem>>? FilterTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for the action section.
    /// </summary>
    [Parameter]
    public RenderFragment<DataList<TItem>>? ActionTemplate { get; set; }

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
    /// Gets or sets a value indicating whether the sort picker button is shown.
    /// </summary>
    [Parameter]
    public bool ShowSortPicker { get; set; } = true;

    /// <summary>
    /// Gets the current <see cref="DataGrid{TItem}"/> instance in use.
    /// Derived from either the <see cref="DataList"/> parameter or the cascading <see cref="ParentList"/>.
    /// </summary>
    protected DataList<TItem>? CurrentList { get; set; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        CurrentList = DataList ?? ParentList;
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
        if (CurrentList == null)
            return;

        InvokeAsync(() => CurrentList.QuickSearch(searchText));
    }

    /// <summary>
    /// Handles the refresh action by refreshing the grid.
    /// </summary>
    protected async Task HandelRefresh()
    {
        if (CurrentList == null)
            return;

        await CurrentList.RefreshAsync(forceReload: true);
    }

    /// <summary>
    /// Handles the export action by exporting the grid data to a CSV file.
    /// </summary>
    protected async Task HandelExport()
    {
        if (CurrentList == null)
            return;

        var name = ExportName ?? Title ?? "Export";
        var fileName = $"{name} {DateTime.Now:yyyy-MM-dd-HH-mm-ss}.csv";

        await CurrentList.ExportAsync(fileName);
    }

    /// <summary>
    /// Toggles the filter panel on the grid.
    /// </summary>
    protected void ToggleFilter()
    {
        if (CurrentList == null)
            return;

        CurrentList.ShowFilter();
    }

    /// <summary>
    /// Toggles the sort picker panel on the grid.
    /// </summary>
    protected void ToggleSortPicker()
    {
        if (CurrentList == null)
            return;

        CurrentList.ShowSortPicker();
    }
}
