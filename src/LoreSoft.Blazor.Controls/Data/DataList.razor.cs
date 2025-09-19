using System.Reflection;
using System.Text;

using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Displays a list of data items using a customizable row template.
/// Supports optional header and footer templates, and additional HTML attributes.
/// </summary>
/// <typeparam name="TItem">The type of the data item.</typeparam>
[CascadingTypeParameter(nameof(TItem))]
public partial class DataList<TItem> : DataComponentBase<TItem>
{
    private readonly Lazy<PropertyInfo[]> _properties = new(() => typeof(TItem).GetProperties().OrderBy(p => p.Name).ToArray());

    /// <summary>
    /// Gets or sets additional attributes to be applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets the template used to render each row in the data list.
    /// </summary>
    [Parameter, EditorRequired]
    public required RenderFragment<TItem> RowTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template used to render the header of the data list.
    /// </summary>
    [Parameter]
    public RenderFragment? HeaderTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template used to render the footer of the data list.
    /// </summary>
    [Parameter]
    public RenderFragment? FooterTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for defining available query fields.
    /// Should contain one or more <see cref="QueryBuilderField{TItem}"/> child components.
    /// </summary>
    [Parameter]
    public RenderFragment? QueryFields { get; set; }

    /// <summary>
    /// Gets or sets the initial field to sort by.
    /// </summary>
    [Parameter]
    public string SortField { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a initial value indicating whether to sort in descending order.
    /// </summary>
    [Parameter]
    public bool SortDescending { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the sort picker is open.
    /// </summary>
    protected bool SortPickerOpen { get; set; }

    /// <summary>
    /// Gets or sets the CSS class name for the root element. Computed from <see cref="AdditionalAttributes"/>.
    /// </summary>
    protected string? ClassName { get; set; }

    /// <summary>
    /// Gets or sets the query builder component.
    /// </summary>
    protected QueryBuilder<TItem>? QueryBuilder { get; set; }

    protected string CurrentSortField { get; set; } = string.Empty;

    protected string CurrentSortDirection { get; set; } = "asc";

    protected IEnumerable<PropertyInfo> Properties => _properties.Value;

    protected Task HandleSortChanged()
    {
        var descending = CurrentSortDirection == "desc";
        return SortByAsync(CurrentSortField, descending);
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        ClassName = new CssBuilder("data-list")
            .MergeClass(AdditionalAttributes)
            .ToString();

        CurrentSortField = SortField;
        CurrentSortDirection = SortDescending ? "desc" : "asc";

        SortBy(SortField, SortDescending);

        base.OnParametersSet();
    }

    /// <summary>
    /// Shows the sort picker panel.
    /// </summary>
    public void ShowSortPicker()
    {
        SortPickerOpen = true;
        StateHasChanged();
    }

    /// <summary>
    /// Closes the sort picker panel.
    /// </summary>
    public void CloseSortPicker()
    {
        SortPickerOpen = false;
        StateHasChanged();
    }

    /// <summary>
    /// Toggles the sort picker panel open or closed.
    /// </summary>
    public void ToggleSortPicker()
    {
        SortPickerOpen = !SortPickerOpen;
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

        if (QueryBuilder == null)
            return;

        var fields = QueryBuilder.Fields;
        if (fields == null || fields.Count == 0)
            return;

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var quickSearch = new QueryGroup { Id = nameof(QuickSearch), Logic = QueryLogic.Or };

            // all filterable string columns
            foreach (var column in fields.Where(c => c.Type == typeof(string)))
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
    /// Exports the grid data to a CSV file.
    /// </summary>
    /// <param name="fileName">The name of the file to export.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
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
            headers: fields.Select(p => p.Column ?? p.Name ?? string.Empty),
            rows: result.Items,
            selector: item => fields.Select(f => f.FieldValue(item)),
            encoding: Encoding.UTF8,
            cancellationToken: cancellationToken);

        // need to reset stream position
        memoryStream.Seek(0, SeekOrigin.Begin);

        var downloadFile = fileName ?? $"Export {DateTime.Now:yyyy-MM-dd-HH-mm-ss}.csv";

        await DownloadService.DownloadFileStream(memoryStream, downloadFile, "text/csv");
    }

    public override Task SortByAsync(string columnName, bool? descending = null)
    {
        CurrentSortField = columnName;
        CurrentSortDirection = descending == true ? "desc" : "asc";

        return base.SortByAsync(columnName, descending);
    }
}
