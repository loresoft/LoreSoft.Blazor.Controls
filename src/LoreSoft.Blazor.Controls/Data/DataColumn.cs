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
/// Represents a column definition for <see cref="DataGrid{TItem}"/> in Blazor.
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
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets a function to compute cell attributes for each data item.
    /// </summary>
    [Parameter]
    public Func<TItem, Dictionary<string, object>>? CellAttributes { get; set; }

    /// <summary>
    /// Gets or sets the property expression for the column. Required.
    /// </summary>
    [Parameter, EditorRequired]
    public required Expression<Func<TItem, object>> Property { get; set; }

    /// <summary>
    /// Gets or sets the column header title.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the column width (CSS value).
    /// </summary>
    [Parameter]
    public string? Width { get; set; }

    /// <summary>
    /// Gets or sets the minimum column width (CSS value).
    /// </summary>
    [Parameter]
    public string? MinWidth { get; set; }

    /// <summary>
    /// Gets or sets the maximum column width (CSS value).
    /// </summary>
    [Parameter]
    public string? MaxWidth { get; set; }

    /// <summary>
    /// Gets or sets the text alignment for cell content.
    /// </summary>
    [Parameter]
    public TextAlign? Align { get; set; }

    /// <summary>
    /// Gets or sets the format string for cell values.
    /// </summary>
    [Parameter]
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets the CSS class for the cell.
    /// </summary>
    [Parameter]
    public string? ClassName { get; set; }

    /// <summary>
    /// Gets or sets a function to compute the cell style for each data item.
    /// </summary>
    [Parameter]
    public Func<TItem, string>? Style { get; set; }

    /// <summary>
    /// Gets or sets the text alignment for the header.
    /// </summary>
    [Parameter]
    public TextAlign? HeaderAlign { get; set; }

    /// <summary>
    /// Gets or sets the CSS style for the header.
    /// </summary>
    [Parameter]
    public string? HeaderStyle { get; set; }

    /// <summary>
    /// Gets or sets the CSS class for the header.
    /// </summary>
    [Parameter]
    public string? HeaderClass { get; set; }

    /// <summary>
    /// Gets or sets the CSS style for the footer.
    /// </summary>
    [Parameter]
    public string? FooterStyle { get; set; }

    /// <summary>
    /// Gets or sets the CSS class for the footer.
    /// </summary>
    [Parameter]
    public string? FooterClass { get; set; }

    /// <summary>
    /// Gets or sets the CSS class for the column.
    /// </summary>
    [Parameter]
    public string? ColumnClass { get; set; }

    /// <summary>
    /// Gets or sets the CSS style for the column.
    /// </summary>
    [Parameter]
    public string? ColumnStyle { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column is sortable.
    /// </summary>
    [Parameter]
    public bool Sortable { get; set; } = true;

    /// <summary>
    /// Gets or sets the sort index for multi-column sorting.
    /// </summary>
    [Parameter]
    public int SortIndex { get; set; } = -1;

    /// <summary>
    /// Gets or sets a value indicating whether the column is sorted in descending order.
    /// </summary>
    [Parameter]
    public bool SortDescending { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column is filterable.
    /// </summary>
    [Parameter]
    public bool Filterable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the column is exportable.
    /// </summary>
    [Parameter]
    public bool Exportable { get; set; } = true;

    /// <summary>
    /// Gets or sets the header text to use when exporting.
    /// </summary>
    [Parameter]
    public string? ExportHeader { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column is groupable.
    /// </summary>
    [Parameter]
    public bool Groupable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether grouping is applied to this column.
    /// </summary>
    [Parameter]
    public bool Grouping { get; set; }

    /// <summary>
    /// Gets or sets the template for rendering grouped rows.
    /// </summary>
    [Parameter]
    public RenderFragment<IGrouping<string, TItem>>? GroupTemplate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column is visible.
    /// </summary>
    [Parameter]
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the column can be hidden.
    /// </summary>
    [Parameter]
    public bool Hideable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show a tooltip for cell values.
    /// </summary>
    [Parameter]
    public bool Tooltip { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether cell content can be multi-line.
    /// </summary>
    [Parameter]
    public bool MultiLine { get; set; } = true;

    /// <summary>
    /// Gets or sets the template for the column header.
    /// </summary>
    [Parameter]
    public RenderFragment? HeaderTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for cell content.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem>? Template { get; set; }

    /// <summary>
    /// Gets or sets the template for the column footer.
    /// </summary>
    [Parameter]
    public RenderFragment<ICollection<TItem>>? FooterTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for the filter UI.
    /// </summary>
    [Parameter]
    public RenderFragment<QueryFilter>? FilterTemplate { get; set; }

    /// <summary>
    /// Gets or sets the list of filter values for the column.
    /// </summary>
    [Parameter]
    public List<string>? FilterValues { get; set; }

    /// <summary>
    /// Gets the property name for the column. Computed from the property expression.
    /// </summary>
    public string PropertyName { get; set; } = null!;

    /// <summary>
    /// Gets the column name, which may be set by a <see cref="ColumnAttribute"/> on the property.
    /// </summary>
    public string ColumnName { get; set; } = null!;

    /// <summary>
    /// Gets the type of the property for the column. Computed from the property expression.
    /// </summary>
    public Type PropertyType { get; set; } = null!;

    /// <summary>
    /// Gets or sets the current sort index for the column.
    /// </summary>
    internal int CurrentSortIndex { get; set; } = -1;

    /// <summary>
    /// Gets or sets a value indicating whether the column is currently sorted in descending order.
    /// </summary>
    internal bool CurrentSortDescending { get; set; }

    /// <summary>
    /// Gets or sets the current computed style for the column.
    /// </summary>
    internal string? CurrentColumnStyle { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column is currently visible.
    /// </summary>
    internal bool CurrentVisible { get; set; }

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

        // register with parent grid
        Grid.AddColumn(this);
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        CurrentColumnStyle = new StyleBuilder()
            .AddStyle(ColumnStyle)
            .AddStyle("width", Width, (v) => v.HasValue())
            .ToString();

        UpdateProperty();
    }

    /// <summary>
    /// Gets the header title for the column.
    /// </summary>
    /// <returns>The header title.</returns>
    internal string HeaderTitle()
    {
        if (!string.IsNullOrEmpty(Title))
            return Title;

        return PropertyName.ToTitle();
    }

    /// <summary>
    /// Gets the cell value for the specified data item.
    /// </summary>
    /// <param name="data">The data item.</param>
    /// <returns>The cell value as a string.</returns>
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
    /// Gets the export name for the column.
    /// </summary>
    /// <returns>The export name.</returns>
    internal string ExportName()
    {
        if (!string.IsNullOrEmpty(ExportHeader))
            return ExportHeader;

        return PropertyName;
    }

    /// <summary>
    /// Updates the sort index and direction for the column.
    /// </summary>
    /// <param name="index">The sort index.</param>
    /// <param name="descending">True if descending; otherwise, false.</param>
    internal void UpdateSort(int index, bool descending)
    {
        CurrentSortIndex = index;
        CurrentSortDescending = descending;
    }

    /// <summary>
    /// Updates the visibility of the column.
    /// </summary>
    /// <param name="value">True to show; false to hide.</param>
    internal void UpdateVisible(bool value)
    {
        CurrentVisible = value;
    }

    /// <summary>
    /// Updates the property metadata for the column.
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
    }

    /// <summary>
    /// Computes the attributes for a cell based on the data item.
    /// </summary>
    /// <param name="data">The data item.</param>
    /// <returns>A dictionary of attributes.</returns>
    internal Dictionary<string, object> ComputeAttributes(TItem data)
    {
        var attributes = new Dictionary<string, object>();

        if (Tooltip)
        {
            var value = CellValue(data);
            if (!string.IsNullOrEmpty(value))
            {
                attributes["title"] = value;
                attributes["aria-label"] = value;
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
    /// </summary>
    /// <param name="data">The data item.</param>
    /// <returns>The computed style string.</returns>
    internal string ComputeStyle(TItem data)
    {
        return StyleBuilder.Default(Style?.Invoke(data) ?? string.Empty)
            .AddStyle("width", Width, (v) => v.HasValue())
            .AddStyle("min-width", MinWidth, (v) => v.HasValue())
            .AddStyle("max-width", MaxWidth, (v) => v.HasValue())
            .AddStyle("overflow", "hidden", !MultiLine)
            .AddStyle("text-overflow", "ellipsis", !MultiLine)
            .AddStyle("white-space", "nowrap", !MultiLine)
            .AddStyle("text-align", "center", Align is TextAlign.Center)
            .AddStyle("text-align", "left", Align is TextAlign.Left or TextAlign.Start)
            .AddStyle("text-align", "right", Align is TextAlign.Right or TextAlign.End)
            .ToString();
    }

    /// <summary>
    /// Computes the style for the column header.
    /// </summary>
    /// <returns>The computed header style string.</returns>
    internal string ComputeHeaderStyle()
    {
        return StyleBuilder.Default(HeaderStyle ?? string.Empty)
            .AddStyle("width", Width, (v) => v.HasValue())
            .AddStyle("min-width", MinWidth, (v) => v.HasValue())
            .AddStyle("max-width", MaxWidth, (v) => v.HasValue())
            .AddStyle("text-align", "center", HeaderAlign is TextAlign.Center)
            .AddStyle("text-align", "left", HeaderAlign is TextAlign.Left or TextAlign.Start)
            .AddStyle("text-align", "right", HeaderAlign is TextAlign.Right or TextAlign.End)
            .ToString();
    }

}
