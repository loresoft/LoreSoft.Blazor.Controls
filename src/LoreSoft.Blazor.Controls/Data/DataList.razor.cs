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
    /// Gets or sets the template for defining the fields of the data list.
    /// Should contain one or more <see cref="DataField{TItem}"/> child components
    /// that define the fields available for sorting, filtering, searching, and exporting.
    /// </summary>
    [Parameter]
    public RenderFragment? DataFields { get; set; }

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
    /// Gets the collection of fields registered by the <see cref="DataFields"/> template.
    /// The fields register as soon as the component renders, so sorting, quick search, and export
    /// work without requiring the filter panel to be opened first.
    /// </summary>
    public List<DataField<TItem>> Fields { get; } = [];

    /// <inheritdoc />
    protected override IReadOnlyList<DataField<TItem>> SortFields => Fields;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        ClassName = CssBuilder.Pool.Use(builder => builder
            .AddClass("data-list")
            .MergeClass(AdditionalAttributes)
            .ToString()
        );

        base.OnParametersSet();
    }

    /// <summary>
    /// Shows the sort picker panel.
    /// Opens the interface that allows users to select the sort fields and directions.
    /// This method triggers a UI update to display the sort picker interface.
    /// </summary>
    public void ShowSortPicker()
    {
        UpdateSortPickerState();

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
        if (!SortPickerOpen)
            UpdateSortPickerState();

        SortPickerOpen = !SortPickerOpen;
        StateHasChanged();
    }

    /// <summary>
    /// Performs a quick search across all searchable string fields defined in <see cref="DataFields"/>.
    /// This method creates a logical OR filter across all string-type fields, allowing users to quickly
    /// find items containing the search text in any searchable field.
    /// </summary>
    /// <param name="searchText">The text to search for across all searchable string fields.
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

        if (Fields.Count == 0)
            return;

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var quickSearch = new QueryGroup { Id = nameof(QuickSearch), Logic = QueryLogic.Or };

            // all filterable string fields
            var fields = Fields
                .Where(f => f.Filterable && f.Searchable && f.PropertyType == typeof(string))
                .DistinctBy(f => f.ColumnName);

            foreach (var field in fields)
            {
                var filter = new QueryFilter
                {
                    Field = FieldName(field),
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
    /// to include the complete filtered dataset. The export uses the exportable fields defined in the
    /// <see cref="DataFields"/> template to determine which columns to include and their headers.
    /// </summary>
    /// <param name="fileName">The name of the file to download. If null, generates a timestamped filename.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the export operation.</param>
    /// <returns>A task representing the asynchronous export operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no valid data provider is available.</exception>
    public virtual async Task ExportAsync(string? fileName = null, CancellationToken cancellationToken = default)
    {
        if (CurrentDataProvider == null)
            throw new InvalidOperationException("Invalid Data Provider");

        var fields = Fields.Where(f => f.Exportable).ToList();
        if (fields.Count == 0)
            return;

        var request = CreateDataRequest(cancellationToken);

        // clear paging for export
        request = request with { Page = 0, PageSize = 0 };

        var result = await CurrentDataProvider(request);

        await using var memoryStream = new MemoryStream();

        await CsvWriter.WriteAsync(
            stream: memoryStream,
            headers: fields.Select(f => f.ExportName),
            rows: result.Items,
            selector: item => fields.Select(f => f.FormattedValue(item)),
            encoding: Encoding.UTF8,
            cancellationToken: cancellationToken);

        // need to reset stream position
        memoryStream.Seek(0, SeekOrigin.Begin);

        var downloadFile = fileName ?? $"Export {DateTime.Now:yyyy-MM-dd-HH-mm-ss}.csv";

        await DownloadService.DownloadFileStream(memoryStream, downloadFile, "text/csv");
    }

    /// <summary>
    /// Registers a data field with the list.
    /// This method is called by child <see cref="DataField{TItem}"/> components during initialization
    /// and ignores duplicate registrations for the same field instance.
    /// </summary>
    /// <param name="field">The field to register with the list.</param>
    internal void AddField(DataField<TItem> field)
    {
        if (Fields.Contains(field))
            return;

        Fields.Add(field);
    }

    /// <summary>
    /// Unregisters a data field from the list.
    /// This method is called by child <see cref="DataField{TItem}"/> components when they are disposed
    /// so the list no longer tracks fields that are not rendered.
    /// </summary>
    /// <param name="field">The field to unregister from the list.</param>
    internal void RemoveField(DataField<TItem> field)
    {
        Fields.Remove(field);
    }

    /// <summary>
    /// Gets the identifier used for filtering and sorting the specified field.
    /// Local data providers use the property name while remote providers use the column name.
    /// </summary>
    /// <param name="field">The field to resolve the name for.</param>
    /// <returns>The property name for local providers; otherwise the column name.</returns>
    internal string FieldName(DataField<TItem> field)
        => IsLocalProvider ? field.PropertyName : field.ColumnName;
}
