using System.Reflection;
using System.Text;

using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Displays a list of data items using a customizable row template with support for sorting, filtering, and exporting.
/// This component extends <see cref="DataComponentBase{TItem}"/> to provide a flexible list-based data display
/// that supports query-based filtering, field-based sorting, and CSV export capabilities.
/// Unlike <see cref="DataGrid{TItem}"/>, this component uses a single template for all rows and focuses on
/// simplicity and customization rather than tabular data presentation.
/// </summary>
/// <typeparam name="TItem">The type of the data item that will be displayed in each list row.</typeparam>
[CascadingTypeParameter(nameof(TItem))]
public partial class DataList<TItem> : DataComponentBase<TItem>
{
    private readonly Lazy<PropertyInfo[]> _properties = new(() => typeof(TItem).GetProperties().OrderBy(p => p.Name).ToArray());

    /// <summary>
    /// Gets or sets the template used to render each row in the data list.
    /// This template defines how individual data items are displayed and receives
    /// the data item as its context. The template is repeated for each item in the current view.
    /// </summary>
    [Parameter, EditorRequired]
    public required RenderFragment<TItem> RowTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template used to render the header of the data list.
    /// When specified, this template is rendered once at the top of the list,
    /// typically used for column headers, titles, or introductory content.
    /// </summary>
    [Parameter]
    public RenderFragment? HeaderTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template used to render the footer of the data list.
    /// When specified, this template is rendered once at the bottom of the list,
    /// typically used for summary information, totals, or action buttons.
    /// </summary>
    [Parameter]
    public RenderFragment? FooterTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for defining available query fields.
    /// Should contain one or more <see cref="QueryBuilderField{TItem}"/> child components
    /// that define the fields available for filtering and searching operations.
    /// These fields are used by the quick search functionality and export operations.
    /// </summary>
    [Parameter]
    public RenderFragment? QueryFields { get; set; }

    /// <summary>
    /// Gets or sets the initial field name to sort by.
    /// This field name should correspond to a property name on the <typeparamref name="TItem"/> type.
    /// If not specified, no initial sorting is applied to the data.
    /// </summary>
    [Parameter]
    public string SortField { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the initial sort direction.
    /// When true, the initial sort is applied in descending order.
    /// When false, the initial sort is applied in ascending order.
    /// This setting only applies if <see cref="SortField"/> is specified.
    /// </summary>
    [Parameter]
    public bool SortDescending { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the sort picker panel is currently open.
    /// The sort picker allows users to select which field to sort by and the sort direction.
    /// This property can be used to coordinate sort picker state with other UI elements.
    /// </summary>
    protected bool SortPickerOpen { get; set; }

    /// <summary>
    /// Gets or sets the computed CSS class name for the root element.
    /// This class is derived from the component's base classes and any additional attributes
    /// specified in <see cref="DataComponentBase{TItem}.AdditionalAttributes"/>.
    /// </summary>
    protected string? ClassName { get; set; }

    /// <summary>
    /// Gets or sets the query builder component instance.
    /// This component manages the available query fields and is used for filtering
    /// operations and export field selection. It's populated when <see cref="QueryFields"/> is specified.
    /// </summary>
    protected QueryBuilder<TItem>? QueryBuilder { get; set; }

    /// <summary>
    /// Gets or sets the currently selected sort field name.
    /// This property tracks the active sort field and is updated when users change
    /// the sort selection through the sort picker interface.
    /// </summary>
    protected string CurrentSortField { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current sort direction as a string.
    /// Valid values are "asc" for ascending and "desc" for descending.
    /// This property is synchronized with the sort picker UI controls.
    /// </summary>
    protected string CurrentSortDirection { get; set; } = "asc";

    /// <summary>
    /// Gets the collection of properties available on the data item type.
    /// This collection is lazily initialized and cached, providing access to
    /// all public properties of <typeparamref name="TItem"/> sorted by name.
    /// Used for reflection-based operations and dynamic field access.
    /// </summary>
    protected IEnumerable<PropertyInfo> Properties => _properties.Value;

    /// <summary>
    /// Handles changes to the sort configuration from the sort picker UI.
    /// This method is called when users modify the sort field or direction
    /// through the sort picker interface, triggering an immediate sort and refresh.
    /// </summary>
    /// <returns>A task representing the asynchronous sort operation.</returns>
    protected Task HandleSortChanged()
    {
        var descending = CurrentSortDirection == "desc";
        return SortByAsync(CurrentSortField, descending);
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        ClassName = CssBuilder.Pool.Use(builder => builder
            .AddClass("data-list")
            .MergeClass(AdditionalAttributes)
            .ToString()
        );

        CurrentSortField = SortField;
        CurrentSortDirection = SortDescending ? "desc" : "asc";

        SortBy(SortField, SortDescending);

        base.OnParametersSet();
    }

    /// <summary>
    /// Shows the sort picker panel.
    /// Opens the interface that allows users to select the sort field and direction.
    /// This method triggers a UI update to display the sort picker interface.
    /// </summary>
    public void ShowSortPicker()
    {
        SortPickerOpen = true;
        StateHasChanged();
    }

    /// <summary>
    /// Closes the sort picker panel.
    /// Hides the sort selection interface without affecting the current sort configuration.
    /// Any sort changes made while the picker was open remain in effect.
    /// </summary>
    public void CloseSortPicker()
    {
        SortPickerOpen = false;
        StateHasChanged();
    }

    /// <summary>
    /// Toggles the sort picker panel open or closed.
    /// If the panel is currently open, it will be closed and vice versa.
    /// This provides a convenient single method for sort picker state management.
    /// </summary>
    public void ToggleSortPicker()
    {
        SortPickerOpen = !SortPickerOpen;
        StateHasChanged();
    }

    /// <summary>
    /// Performs a quick search across all filterable string fields defined in the query builder.
    /// This method creates a logical OR filter across all string-type fields that have been
    /// configured in the <see cref="QueryFields"/> template, allowing users to quickly
    /// find items containing the search text in any searchable field.
    /// </summary>
    /// <param name="searchText">The text to search for across all filterable string fields.
    /// If null or empty, any existing quick search filters are removed.</param>
    /// <param name="clearFilter">When true, clears all existing filters before applying the search.
    /// When false, only removes previous quick search filters while preserving other filters.</param>
    /// <returns>A task representing the asynchronous search operation and data refresh.</returns>
    public async Task QuickSearch(string? searchText, bool clearFilter = false)
    {
        if (clearFilter)
            RootQuery.Filters.Clear();
        else
            RootQuery.Filters.RemoveAll(f => f.Id == nameof(QuickSearch));

        if (QueryBuilder == null)
            return;

        var fields = QueryBuilder.Fields;
        if (fields == null || fields.Count == 0)
            return;

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var quickSearch = new QueryGroup { Id = nameof(QuickSearch), Logic = QueryLogic.Or };

            // all filterable string columns
            foreach (var column in fields.Where(c => c.Searchable && c.Type == typeof(string)))
            {
                var fieldName = column.Name;
                if (string.IsNullOrWhiteSpace(fieldName))
                    continue;

                var filter = new QueryFilter
                {
                    Field = fieldName,
                    Operator = QueryOperators.Contains,
                    Value = searchText
                };
                quickSearch.Filters.Add(filter);
            }

            RootQuery.Filters.Add(quickSearch);
        }

        await RefreshAsync(true);
    }

    /// <summary>
    /// Exports the list data to a CSV file.
    /// This method exports all data that matches the current filter criteria, bypassing pagination
    /// to include the complete filtered dataset. The export uses the fields defined in the
    /// <see cref="QueryFields"/> template to determine which columns to include and their headers.
    /// </summary>
    /// <param name="fileName">The name of the file to download. If null, generates a timestamped filename.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the export operation.</param>
    /// <returns>A task representing the asynchronous export operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no valid data provider is available.</exception>
    public virtual async Task ExportAsync(string? fileName = null, CancellationToken cancellationToken = default)
    {
        if (CurrentDataProvider == null)
            throw new InvalidOperationException("Invalid Data Provider");

        if (QueryBuilder == null)
            return;

        var fields = QueryBuilder.Fields;
        if (fields == null || fields.Count == 0)
            return;

        var request = CreateDataRequest(cancellationToken);

        // clear paging for export
        request = request with { Page = 0, PageSize = 0 };

        var result = await CurrentDataProvider(request);

        await using var memoryStream = new MemoryStream();

        await CsvWriter.WriteAsync(
            stream: memoryStream,
            headers: fields.Select(p => p.CurrentName ?? p.Name ?? string.Empty),
            rows: result.Items,
            selector: item => fields.Select(f => f.FieldValue(item)),
            encoding: Encoding.UTF8,
            cancellationToken: cancellationToken);

        // need to reset stream position
        memoryStream.Seek(0, SeekOrigin.Begin);

        var downloadFile = fileName ?? $"Export {DateTime.Now:yyyy-MM-dd-HH-mm-ss}.csv";

        await DownloadService.DownloadFileStream(memoryStream, downloadFile, "text/csv");
    }

    /// <summary>
    /// Sorts the data by the specified column name and refreshes the display.
    /// This override updates the internal sort tracking properties to maintain
    /// synchronization with the sort picker UI controls, then delegates to the base implementation.
    /// </summary>
    /// <param name="columnName">The name of the column to sort by.
    /// This should match a property name in the data item type.</param>
    /// <param name="descending">The sort direction. If null, toggles the current direction
    /// or defaults to ascending for new sorts.</param>
    /// <returns>A task representing the asynchronous sort operation and data refresh.</returns>
    public override Task SortByAsync(string columnName, bool? descending = null)
    {
        CurrentSortField = columnName;
        CurrentSortDirection = descending == true ? "desc" : "asc";

        return base.SortByAsync(columnName, descending);
    }
}
