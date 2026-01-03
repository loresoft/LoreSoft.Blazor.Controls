// Ignore Spelling: Groupable

using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

using LoreSoft.Blazor.Controls.Extensions;
using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents a column definition for <see cref="DataGrid{TItem}"/>.
/// Provides configuration for display, sorting, filtering, grouping, exporting, and templates.
/// </summary>
/// <typeparam name="TItem">The type of the data item.</typeparam>
[CascadingTypeParameter(nameof(TItem))]
public class DataColumn<TItem> : ComponentBase
{
    private Func<TItem, object>? _propertyAccessor;

    /// <summary>
    /// Gets or sets the parent <see cref="DataGrid{TItem}"/> component.
    /// </summary>
    [CascadingParameter(Name = "Grid")]
    protected DataGrid<TItem>? Grid { get; set; }

    /// <summary>
    /// Gets or sets additional attributes to be applied to the column.
    /// These attributes will be merged with computed cell attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets a function to compute cell attributes for each data item.
    /// The function receives the data item and returns a dictionary of HTML attributes.
    /// </summary>
    [Parameter]
    public Func<TItem, Dictionary<string, object>>? CellAttributes { get; set; }

    /// <summary>
    /// Gets or sets the property expression for the column. Required.
    /// This expression defines which property of the data item to display in this column.
    /// </summary>
    [Parameter, EditorRequired]
    public required Expression<Func<TItem, object>> Property { get; set; }

    /// <summary>
    /// Gets or sets the column header title.
    /// If not specified, the property name will be used with title case formatting.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the column width (CSS value).
    /// Can be any valid CSS width value (e.g., "100px", "20%", "auto").
    /// </summary>
    [Parameter]
    public string? Width { get; set; }

    /// <summary>
    /// Gets or sets the minimum column width (CSS value).
    /// Prevents the column from being resized below this width.
    /// </summary>
    [Parameter]
    public string? MinWidth { get; set; }

    /// <summary>
    /// Gets or sets the maximum column width (CSS value).
    /// Prevents the column from being resized above this width.
    /// </summary>
    [Parameter]
    public string? MaxWidth { get; set; }

    /// <summary>
    /// Gets or sets the text alignment for cell content.
    /// Controls how text is aligned within the cell.
    /// </summary>
    [Parameter]
    public TextAlign? Align { get; set; }

    /// <summary>
    /// Gets or sets the format string for cell values.
    /// Uses standard .NET format strings (e.g., "C" for currency, "d" for short date).
    /// </summary>
    [Parameter]
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets the CSS class for the cell.
    /// This class will be applied to all cells in this column.
    /// </summary>
    [Parameter]
    public string? CellClass { get; set; }

    /// <summary>
    /// Gets or sets a function to compute the cell style for each data item.
    /// The function receives the data item and returns a CSS style string.
    /// </summary>
    [Parameter]
    public Func<TItem, string>? CellStyle { get; set; }

    /// <summary>
    /// Gets or sets the text alignment for the header.
    /// Controls how the header text is aligned within the header cell.
    /// </summary>
    [Parameter]
    public TextAlign? HeaderAlign { get; set; }

    /// <summary>
    /// Gets or sets the CSS style for the header.
    /// Inline styles to be applied to the header cell.
    /// </summary>
    [Parameter]
    public string? HeaderStyle { get; set; }

    /// <summary>
    /// Gets or sets the CSS class for the header.
    /// This class will be applied to the header cell of this column.
    /// </summary>
    [Parameter]
    public string? HeaderClass { get; set; }

    /// <summary>
    /// Gets or sets the CSS style for the footer.
    /// Inline styles to be applied to the footer cell.
    /// </summary>
    [Parameter]
    public string? FooterStyle { get; set; }

    /// <summary>
    /// Gets or sets the CSS class for the footer.
    /// This class will be applied to the footer cell of this column.
    /// </summary>
    [Parameter]
    public string? FooterClass { get; set; }

    /// <summary>
    /// Gets or sets the CSS class for the column.
    /// This class will be applied to all cells in the column (header, data, and footer).
    /// </summary>
    [Parameter]
    public string? ColumnClass { get; set; }

    /// <summary>
    /// Gets or sets the CSS style for the column.
    /// Inline styles to be applied to all cells in the column (header, data, and footer).
    /// </summary>
    [Parameter]
    public string? ColumnStyle { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column is sortable.
    /// When true, users can click the column header to sort by this column.
    /// </summary>
    [Parameter]
    public bool Sortable { get; set; } = true;

    /// <summary>
    /// Gets or sets the sort index for multi-column sorting.
    /// Lower values indicate higher priority in the sort order. -1 indicates no sorting.
    /// </summary>
    [Parameter]
    public int SortIndex { get; set; } = -1;

    /// <summary>
    /// Gets or sets a value indicating whether the column is sorted in descending order.
    /// Only applies when <see cref="SortIndex"/> is not -1.
    /// </summary>
    [Parameter]
    public bool SortDescending { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column is filterable.
    /// When true, users can apply filters to this column.
    /// </summary>
    [Parameter]
    public bool Filterable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the column is included in quick search.
    /// When true, this column will be searched during quick search operations.
    /// </summary>
    [Parameter]
    public bool Searchable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the column is exportable.
    /// When true, this column will be included in data exports.
    /// </summary>
    [Parameter]
    public bool Exportable { get; set; } = true;

    /// <summary>
    /// Gets or sets the header text to use when exporting.
    /// If not specified, the <see cref="PropertyName"/> will be used for exports.
    /// </summary>
    [Parameter]
    public string? ExportHeader { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column is groupable.
    /// When true, users can group data by this column.
    /// </summary>
    [Parameter]
    public bool Groupable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether grouping is applied to this column.
    /// When true, the data will be grouped by this column's values.
    /// </summary>
    [Parameter]
    public bool Grouping { get; set; }

    /// <summary>
    /// Gets or sets the template for rendering grouped rows.
    /// The template receives an <see cref="IGrouping{TKey, TElement}"/> containing the group data.
    /// </summary>
    [Parameter]
    public RenderFragment<IGrouping<string, TItem>>? GroupTemplate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column is visible.
    /// When false, the column will be hidden from the grid display.
    /// </summary>
    [Parameter]
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the column can be hidden.
    /// When true, users can hide/show this column through the column picker.
    /// </summary>
    [Parameter]
    public bool Hideable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show a tooltip for cell values.
    /// When true, the cell value will be displayed as a tooltip via the title attribute on hover.
    /// </summary>
    [Parameter]
    public bool Tooltip { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether cell content can be multi-line.
    /// When false, content will be truncated with ellipsis if it overflows.
    /// </summary>
    [Parameter]
    public bool MultiLine { get; set; } = true;

    /// <summary>
    /// Gets or sets the template for the column header.
    /// When specified, this template will be used instead of the default header text.
    /// </summary>
    [Parameter]
    public RenderFragment? HeaderTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for cell content.
    /// When specified, this template will be used instead of the default cell value display.
    /// The template receives the data item as its context.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem>? Template { get; set; }

    /// <summary>
    /// Gets or sets the template for the column footer.
    /// The template receives a collection of all data items as its context.
    /// </summary>
    [Parameter]
    public RenderFragment<ICollection<TItem>?>? FooterTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for the filter UI.
    /// When specified, this template will be used for custom filter controls.
    /// The template receives a <see cref="QueryFilter"/> as its context.
    /// </summary>
    [Parameter]
    public RenderFragment<QueryFilter>? FilterTemplate { get; set; }

    /// <summary>
    /// Gets or sets the list of filter values for the column.
    /// These values are used for dropdown-style filters.
    /// </summary>
    [Parameter]
    public List<string>? FilterValues { get; set; }

    /// <summary>
    /// Gets or sets the responsive breakpoints for the column visibility.
    /// When specified, the column will only be visible at the defined breakpoint sizes and larger.
    /// This allows for responsive column management where columns can be hidden on smaller screens
    /// and shown on larger devices to optimize the user experience across different screen sizes.
    /// </summary>
    [Parameter]
    public Breakpoints? Breakpoint { get; set; }

    /// <summary>
    /// Gets the property name for the column. Computed from the property expression.
    /// This is the actual property name from the data model.
    /// </summary>
    public string PropertyName { get; private set; } = null!;

    /// <summary>
    /// Gets the column name, which may be set by a <see cref="ColumnAttribute"/> on the property.
    /// If no <see cref="ColumnAttribute"/> is present, this will be the same as <see cref="PropertyName"/>.
    /// </summary>
    public string ColumnName { get; private set; } = null!;

    /// <summary>
    /// Gets the type of the property for the column. Computed from the property expression.
    /// This is used for type-specific formatting and filtering operations.
    /// </summary>
    public Type PropertyType { get; private set; } = null!;

    /// <summary>
    /// Gets the display name for the column header.
    /// This is either the <see cref="Title"/> parameter or the property name formatted for display.
    /// </summary>
    public string HeaderName { get; private set; } = null!;

    /// <summary>
    /// Gets the name to use when exporting this column.
    /// This is either the <see cref="ExportHeader"/> parameter or the <see cref="PropertyName"/>.
    /// </summary>
    public string ExportName { get; private set; } = null!;

    /// <summary>
    /// Gets the computed CSS class for the header cell.
    /// This includes base classes and any custom header classes.
    /// </summary>
    internal string? CurrentHeaderClass { get; private set; }

    /// <summary>
    /// Gets the computed CSS style for the header cell.
    /// This includes sizing, alignment, and any custom header styles.
    /// </summary>
    internal string? CurrentHeaderStyle { get; private set; }

    /// <summary>
    /// Gets the computed CSS class for data cells.
    /// This includes base classes and any custom cell classes.
    /// </summary>
    internal string? CurrentCellClass { get; private set; }

    /// <summary>
    /// Gets the computed CSS class for the footer cell.
    /// This includes base classes and any custom footer classes.
    /// </summary>
    internal string? CurrentFooterClass { get; private set; }

    /// <summary>
    /// Gets the computed CSS style for the footer cell.
    /// This includes sizing, alignment, and any custom footer styles.
    /// </summary>
    internal string? CurrentFooterStyle { get; private set; }


    /// <summary>
    /// Gets or sets the current sort index for the column.
    /// This represents the actual sort index applied to the column, which may differ from the parameter value.
    /// </summary>
    internal int CurrentSortIndex { get; private set; } = -1;

    /// <summary>
    /// Gets or sets a value indicating whether the column is currently sorted in descending order.
    /// This represents the actual sort direction applied to the column.
    /// </summary>
    internal bool CurrentSortDescending { get; private set; }

    /// <summary>
    /// Gets the current sort state as a string.
    /// Returns "ascending", "descending", or "none" based on the current sort configuration.
    /// </summary>
    internal string CurrentSort { get; private set; } = "none";

    /// <summary>
    /// Gets a value indicating whether the column is currently visible.
    /// This represents the actual visibility state, which may be modified by user interactions.
    /// </summary>
    internal bool CurrentVisible { get; private set; }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        if (Grid == null)
            throw new InvalidOperationException("DataColumn must be child of DataGrid");

        if (Property == null)
            throw new InvalidOperationException("DataColumn Property parameter is required");

        CurrentSortIndex = SortIndex;
        CurrentSortDescending = SortDescending;
        CurrentVisible = Visible;
        CurrentSort = SortIndex >= 0 ? SortDescending ? "descending" : "ascending" : "none";

        // register with parent grid
        Grid.AddColumn(this);
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        UpdateProperty();
        UpdateStyles();
    }

    /// <summary>
    /// Gets the cell value for the specified data item.
    /// Applies formatting if a <see cref="Format"/> string is specified.
    /// </summary>
    /// <param name="data">The data item.</param>
    /// <returns>The formatted cell value as a string, or an empty string if the value is null.</returns>
    internal string CellValue(TItem data)
    {
        if (data == null || Property == null)
            return string.Empty;

        _propertyAccessor ??= Property.Compile();

        object? value = null;

        try
        {
            value = _propertyAccessor.Invoke(data);
        }
        catch (NullReferenceException)
        {

        }

        if (value == null)
            return string.Empty;

        return string.IsNullOrEmpty(Format)
            ? value.ToString() ?? string.Empty
            : string.Format(CultureInfo.CurrentCulture, $"{{0:{Format}}}", value);
    }

    /// <summary>
    /// Updates the sort index and direction for the column.
    /// This method is called internally when the grid's sort configuration changes.
    /// </summary>
    /// <param name="index">The sort index. Use -1 to indicate no sorting.</param>
    /// <param name="descending">True if descending; otherwise, false.</param>
    internal void UpdateSort(int index, bool descending)
    {
        CurrentSortIndex = index;
        CurrentSortDescending = descending;
        CurrentSort = index >= 0 ? descending ? "descending" : "ascending" : "none";
    }

    /// <summary>
    /// Updates the visibility of the column.
    /// This method is called internally when the column visibility is changed by user interaction.
    /// </summary>
    /// <param name="value">True to show; false to hide.</param>
    internal void UpdateVisible(bool value)
    {
        CurrentVisible = value;
    }

    /// <summary>
    /// Updates the property metadata for the column.
    /// Extracts property information from the property expression and initializes display names.
    /// </summary>
    private void UpdateProperty()
    {
        MemberInfo? memberInfo = null;

        if (Property?.Body is MemberExpression memberExpression)
            memberInfo = memberExpression.Member;
        else if (Property?.Body is UnaryExpression { Operand: MemberExpression memberOperand })
            memberInfo = memberOperand.Member;

        if (memberInfo == null)
            throw new InvalidOperationException("Property assigned not supported");

        if (memberInfo is PropertyInfo propertyInfo)
        {
            PropertyName = propertyInfo.Name;
            PropertyType = propertyInfo.PropertyType;
        }
        else if (memberInfo is FieldInfo fieldInfo)
        {
            PropertyName = fieldInfo.Name;
            PropertyType = fieldInfo.FieldType;
        }
        else
        {
            PropertyName = memberInfo.Name;
            PropertyType = typeof(object);
        }

        var columnAttribute = memberInfo.GetCustomAttribute<ColumnAttribute>(true);
        ColumnName = columnAttribute?.Name ?? PropertyName;

        // allow empty header, only default if null
        HeaderName = Title ?? PropertyName.ToTitle();

        ExportName = string.IsNullOrEmpty(ExportHeader) ? PropertyName : ExportHeader;
    }

    /// <summary>
    /// Updates the computed CSS classes and styles for the column.
    /// This method recalculates header and cell styling based on current parameter values.
    /// </summary>
    private void UpdateStyles()
    {
        bool sortable = Sortable && (Grid?.Sortable ?? false);

        CurrentHeaderClass = CssBuilder.Pool.Use(builder =>
        {
            return builder
                .AddClass("data-grid__header-cell")
                .AddClass(HeaderClass, v => v.HasValue())
                .AddClass(ColumnClass, v => v.HasValue())
                .AddClass("data-grid__header-cell--sortable-button", sortable)
                .ToString();
        });

        CurrentHeaderStyle = StyleBuilder.Pool.Use(builder =>
        {
            return builder
                .AddStyle(HeaderStyle ?? string.Empty)
                .AddStyle(ColumnStyle)
                .AddStyle($"flex", $"0 0 {Width}", Width.HasValue())
                .AddStyle("min-width", MinWidth, (v) => v.HasValue())
                .AddStyle("max-width", MaxWidth, (v) => v.HasValue())
                .AddStyle("justify-content", "center", HeaderAlign is TextAlign.Center)
                .AddStyle("justify-content", "flex-end", HeaderAlign is TextAlign.Right or TextAlign.End)
                .ToString();
        });

        CurrentCellClass = CssBuilder.Pool.Use(builder =>
        {
            return builder
                .AddClass("data-grid__cell")
                .AddClass(CellClass, v => v.HasValue())
                .AddClass(ColumnClass, v => v.HasValue())
                .ToString();
        });

        CurrentFooterClass = CssBuilder.Pool.Use(builder =>
        {
            return builder
                .AddClass("data-grid__footer-cell")
                .AddClass(FooterClass, v => v.HasValue())
                .AddClass(ColumnClass, v => v.HasValue())
                .ToString();
        });

        CurrentFooterStyle = StyleBuilder.Pool.Use(builder =>
        {
            return builder
                .AddStyle(FooterStyle ?? string.Empty)
                .AddStyle(ColumnStyle)
                .AddStyle($"flex", $"0 0 {Width}", Width.HasValue())
                .AddStyle("min-width", MinWidth, (v) => v.HasValue())
                .AddStyle("max-width", MaxWidth, (v) => v.HasValue())
                .ToString();
        });
    }

    /// <summary>
    /// Computes the attributes for a cell based on the data item.
    /// Combines default attributes (aria-label, title) with custom attributes from parameters.
    /// </summary>
    /// <param name="data">The data item for the current row.</param>
    /// <returns>A dictionary of HTML attributes to apply to the cell.</returns>
    internal Dictionary<string, object> ComputeAttributes(TItem data)
    {
        var attributes = new Dictionary<string, object>
        {
            ["aria-label"] = HeaderName
        };

        if (Tooltip)
        {
            var value = CellValue(data);
            if (!string.IsNullOrEmpty(value))
            {
                attributes["title"] = value;
            }
        }

        if (CellAttributes != null)
        {
            var computed = CellAttributes(data);
            if (computed != null)
            {
                foreach (var attribute in computed)
                    attributes[attribute.Key] = attribute.Value;
            }
        }

        if (AdditionalAttributes != null)
        {
            foreach (var attribute in AdditionalAttributes)
                attributes[attribute.Key] = attribute.Value;
        }

        return attributes;
    }

    /// <summary>
    /// Computes the style for a cell based on the data item.
    /// Combines column sizing, alignment, text overflow settings, and custom styles.
    /// </summary>
    /// <param name="data">The data item for the current row.</param>
    /// <returns>The computed CSS style string for the cell.</returns>
    internal string? ComputeCellStyle(TItem data)
    {
        return StyleBuilder.Pool.Use(builder => builder
            .AddStyle(CellStyle?.Invoke(data))
            .AddStyle(ColumnStyle)
            .AddStyle($"flex", $"0 0 {Width}", Width.HasValue())
            .AddStyle("min-width", MinWidth, (v) => v.HasValue())
            .AddStyle("max-width", MaxWidth, (v) => v.HasValue())
            .AddStyle("overflow", "hidden", !MultiLine)
            .AddStyle("text-overflow", "ellipsis", !MultiLine)
            .AddStyle("white-space", "nowrap", !MultiLine)
            .AddStyle("text-align", "center", Align is TextAlign.Center)
            .AddStyle("text-align", "left", Align is TextAlign.Left or TextAlign.Start)
            .AddStyle("text-align", "right", Align is TextAlign.Right or TextAlign.End)
            .AddStyle("justify-content", "center", Align is TextAlign.Center)
            .AddStyle("justify-content", "flex-start", Align is TextAlign.Left or TextAlign.Start)
            .AddStyle("justify-content", "flex-end", Align is TextAlign.Right or TextAlign.End)
            .ToString()
        );
    }
}
