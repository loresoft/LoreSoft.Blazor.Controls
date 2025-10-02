using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Provides a comprehensive toolbar for <see cref="DataList{TItem}"/> components in Blazor applications.
/// The toolbar includes integrated support for search functionality, filtering controls, data export capabilities,
/// list refresh operations, and sort picker management. It can be used as a standalone component
/// or integrated seamlessly with DataList components through cascading parameters.
/// </summary>
/// <typeparam name="TItem">The type of the data item displayed in the associated DataList.
/// Must be a reference type to ensure proper equality comparisons and state management.</typeparam>
public partial class DataListToolbar<TItem> : ComponentBase
    where TItem : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataListToolbar{TItem}"/> class.
    /// Sets up the debounced search functionality with default timing to provide
    /// responsive search behavior without overwhelming the data source with requests.
    /// </summary>
    public DataListToolbar()
    {
        SearchText = new DebounceValue<string>(HandleSearch);
    }

    /// <summary>
    /// Gets or sets the parent <see cref="DataList{TItem}"/> component from the cascading parameter. 
    /// Mutually exclusive with <see cref="DataList"/>.
    /// This parameter is automatically populated when the toolbar is placed within a DataList's template,
    /// enabling automatic integration without explicit configuration.
    /// </summary>
    [CascadingParameter(Name = "Grid")]
    protected DataList<TItem>? ParentList { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="DataList{TItem}"/> instance to operate on. 
    /// Mutually exclusive with <see cref="ParentList"/>.
    /// Use this parameter when the toolbar is used outside of a DataList template
    /// or when you need to explicitly specify which list instance to control.
    /// </summary>
    [Parameter]
    public DataList<TItem>? DataList { get; set; }

    /// <summary>
    /// Gets or sets the template for the title section of the toolbar.
    /// When specified, this template replaces the default title display and receives
    /// the current DataList instance as context, allowing for dynamic title content
    /// based on list state, data, or other properties.
    /// </summary>
    [Parameter]
    public RenderFragment<DataList<TItem>>? TitleTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for the filter section of the toolbar.
    /// This template is rendered in the filter area and receives the current DataList
    /// instance as context. Use this to provide custom filter controls beyond the
    /// standard search box and filter toggle button.
    /// </summary>
    [Parameter]
    public RenderFragment<DataList<TItem>>? FilterTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for the action section of the toolbar.
    /// This template is rendered in the action area and receives the current DataList
    /// instance as context. Use this to add custom action buttons, dropdowns, or
    /// other interactive elements specific to your application's needs.
    /// </summary>
    [Parameter]
    public RenderFragment<DataList<TItem>>? ActionTemplate { get; set; }

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
    /// all filterable string fields defined in the associated DataList's query fields.
    /// The search uses contains logic with OR operations across fields.
    /// </summary>
    [Parameter]
    public bool ShowSearch { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the filter toggle button is displayed in the toolbar.
    /// When true, provides a button that opens the DataList's filter panel,
    /// allowing users to configure advanced filtering options beyond the basic search functionality.
    /// </summary>
    [Parameter]
    public bool ShowFilter { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the export button is displayed in the toolbar.
    /// When true, provides a button that exports the current DataList data (filtered but unpaged)
    /// to a CSV file. The export uses the fields defined in the DataList's query fields configuration.
    /// </summary>
    [Parameter]
    public bool ShowExport { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the refresh button is displayed in the toolbar.
    /// When true, provides a button that forces a complete refresh of the DataList data,
    /// clearing any cached data and reloading from the original data source.
    /// </summary>
    [Parameter]
    public bool ShowRefresh { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the sort picker button is displayed in the toolbar.
    /// When true, provides a button that opens the sort selection panel, allowing users
    /// to choose which field to sort by and specify the sort direction (ascending/descending).
    /// </summary>
    [Parameter]
    public bool ShowSortPicker { get; set; } = true;

    /// <summary>
    /// Gets the current <see cref="DataList{TItem}"/> instance in use by the toolbar.
    /// This property is derived from either the <see cref="DataList"/> parameter or the 
    /// cascading <see cref="ParentList"/> parameter, providing a unified way to access
    /// the associated list regardless of how the toolbar was configured.
    /// </summary>
    protected DataList<TItem>? CurrentList { get; set; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        CurrentList = DataList ?? ParentList;
    }

    /// <summary>
    /// Gets the debounced value wrapper for the search text input.
    /// This wrapper automatically delays search execution to avoid overwhelming the data source
    /// with requests during rapid typing. The default delay provides a balance between
    /// responsiveness and performance.
    /// </summary>
    protected DebounceValue<string> SearchText { get; }

    /// <summary>
    /// Handles the search action by invoking quick search on the associated DataList.
    /// This method is called automatically by the debounced search input after the
    /// configured delay period. It performs a contains-based search across all
    /// filterable string fields defined in the DataList's query fields using OR logic.
    /// </summary>
    /// <param name="searchText">The search text entered by the user. 
    /// If null or empty, any existing quick search filters are removed.</param>
    protected void HandleSearch(string? searchText)
    {
        if (CurrentList == null)
            return;

        InvokeAsync(() => CurrentList.QuickSearch(searchText));
    }

    /// <summary>
    /// Handles the refresh action by forcing a complete refresh of the DataList data.
    /// This method clears any cached data and reloads from the original data source,
    /// ensuring that the most current data is displayed. This is particularly useful
    /// for data that may have been modified externally.
    /// </summary>
    /// <returns>A task representing the asynchronous refresh operation.</returns>
    protected async Task HandelRefresh()
    {
        if (CurrentList == null)
            return;

        await CurrentList.RefreshAsync(forceReload: true);
    }

    /// <summary>
    /// Handles the export action by generating and downloading a CSV file containing the current DataList data.
    /// This method exports all data that matches the current filter criteria, bypassing pagination
    /// to include the complete filtered dataset. The export uses the fields defined in the
    /// DataList's query fields configuration to determine which columns to include and their headers.
    /// The filename is automatically generated with a timestamp to ensure uniqueness.
    /// </summary>
    /// <returns>A task representing the asynchronous export operation.</returns>
    protected async Task HandelExport()
    {
        if (CurrentList == null)
            return;

        var name = ExportName ?? Title ?? "Export";
        var fileName = $"{name} {DateTime.Now:yyyy-MM-dd-HH-mm-ss}.csv";

        await CurrentList.ExportAsync(fileName);
    }

    /// <summary>
    /// Toggles the visibility of the filter panel on the associated DataList.
    /// This method opens the filter panel, providing advanced filtering capabilities
    /// beyond the basic search functionality. The filter panel allows users to create
    /// complex filter conditions across multiple fields defined in the DataList's query fields.
    /// </summary>
    protected void ToggleFilter()
    {
        if (CurrentList == null)
            return;

        CurrentList.ShowFilter();
    }

    /// <summary>
    /// Toggles the visibility of the sort picker panel on the associated DataList.
    /// This method opens the sort picker panel, allowing users to select which field
    /// to sort by and specify the sort direction. The sort picker provides access to
    /// all properties of the data item type for flexible sorting options.
    /// </summary>
    protected void ToggleSortPicker()
    {
        if (CurrentList == null)
            return;

        CurrentList.ShowSortPicker();
    }
}
