using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

using LoreSoft.Blazor.Controls.Extensions;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents a field definition for <see cref="DataList{TItem}"/>.
/// Provides metadata used for sorting, filtering, quick search, and exporting.
/// </summary>
/// <typeparam name="TItem">The type of the data item.</typeparam>
/// <remarks>
/// A <see cref="DataField{TItem}"/> must be a child of a <see cref="DataList{TItem}"/>. It registers
/// itself with the parent list when initialized and removes itself when disposed.
/// This type is also the base class for <see cref="DataColumn{TItem}"/>, which extends it with
/// grid specific presentation, grouping and template support.
/// </remarks>
[CascadingTypeParameter(nameof(TItem))]
public class DataField<TItem> : ComponentBase, IDisposable
{
    private Func<TItem, object>? _propertyAccessor;
    private Expression<Func<TItem, object>>? _previousProperty;

    /// <summary>
    /// Gets or sets the parent <see cref="DataList{TItem}"/> component.
    /// </summary>
    [CascadingParameter(Name = "List")]
    protected DataList<TItem>? List { get; set; }

    /// <summary>
    /// Gets or sets the property expression for the field or column. Required.
    /// This expression defines which property of the data item this component represents.
    /// </summary>
    [Parameter, EditorRequired]
    public required Expression<Func<TItem, object>> Property { get; set; }

    /// <summary>
    /// Gets or sets the display title for the field or column.
    /// If not specified, the property name is used with title case formatting.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the format string used when formatting values.
    /// Uses standard .NET format strings (e.g., "C" for currency, "d" for short date).
    /// </summary>
    [Parameter]
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets a custom formatting function for values.
    /// When specified, takes precedence over <see cref="Format"/>. The function receives the
    /// raw value and returns the formatted string, or <see langword="null"/> to render empty.
    /// </summary>
    /// <remarks>
    /// This function is applied consistently for both rendering and CSV export, so the exported
    /// values match what is displayed.
    /// </remarks>
    [Parameter]
    public Func<object?, string?>? FormatValue { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the field or column can be sorted.
    /// </summary>
    [Parameter]
    public bool Sortable { get; set; } = true;

    /// <summary>
    /// Gets or sets the sort index for multi-field sorting.
    /// Lower values indicate higher priority in the sort order. -1 indicates no sorting.
    /// </summary>
    [Parameter]
    public int SortIndex { get; set; } = -1;

    /// <summary>
    /// Gets or sets a value indicating whether the field or column is sorted in descending order.
    /// Only applies when <see cref="SortIndex"/> is not -1.
    /// </summary>
    [Parameter]
    public bool SortDescending { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the field or column can be filtered.
    /// </summary>
    [Parameter]
    public bool Filterable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the field or column is included in quick search.
    /// </summary>
    [Parameter]
    public bool Searchable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the field or column is included in data exports.
    /// </summary>
    [Parameter]
    public bool Exportable { get; set; } = true;

    /// <summary>
    /// Gets or sets the header text to use when exporting.
    /// If not specified, the <see cref="PropertyName"/> is used.
    /// </summary>
    [Parameter]
    public string? ExportHeader { get; set; }

    /// <summary>
    /// Gets or sets the list of filter values for the field or column.
    /// These values are used for dropdown style filters.
    /// </summary>
    [Parameter]
    public List<string>? FilterValues { get; set; }

    /// <summary>
    /// Gets or sets the template used to render the filter value input.
    /// </summary>
    [Parameter]
    public RenderFragment<QueryFilter>? FilterTemplate { get; set; }

    /// <summary>
    /// Gets the property name. Computed from <see cref="Property"/>.
    /// </summary>
    public string PropertyName { get; protected set; } = null!;

    /// <summary>
    /// Gets the column name, which may be set by a <see cref="ColumnAttribute"/> on the property.
    /// When no <see cref="ColumnAttribute"/> is present, this matches <see cref="PropertyName"/>.
    /// </summary>
    public string ColumnName { get; protected set; } = null!;

    /// <summary>
    /// Gets the type of the property. Computed from <see cref="Property"/>.
    /// </summary>
    public Type PropertyType { get; protected set; } = null!;

    /// <summary>
    /// Gets the display name used for headers.
    /// This is either <see cref="Title"/> or the property name formatted for display.
    /// </summary>
    public string HeaderName { get; protected set; } = null!;

    /// <summary>
    /// Gets the name to use when exporting.
    /// </summary>
    public string ExportName { get; protected set; } = null!;

    /// <summary>
    /// Gets the current sort index for the field or column.
    /// This represents the actual sort index applied, which may differ from the parameter value.
    /// </summary>
    internal int CurrentSortIndex { get; private set; } = -1;

    /// <summary>
    /// Gets a value indicating whether the field or column is currently sorted in descending order.
    /// This represents the actual sort direction applied.
    /// </summary>
    internal bool CurrentSortDescending { get; private set; }

    /// <summary>
    /// Gets the current sort state as a string.
    /// Returns "ascending", "descending", or "none" based on the current sort configuration.
    /// </summary>
    internal string CurrentSort { get; private set; } = "none";

    /// <summary>
    /// Updates the sort index and direction for the field or column.
    /// This method is called internally when the parent component's sort configuration changes.
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
    /// Applies the initial sort configuration from the <see cref="SortIndex"/>
    /// and <see cref="SortDescending"/> parameters.
    /// </summary>
    protected void ApplyInitialSort()
    {
        UpdateSort(SortIndex, SortDescending);
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        RegisterParent();
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        UpdateProperty();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        UnregisterParent();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Validates the parent component and registers this instance with it.
    /// </summary>
    /// <exception cref="InvalidOperationException">The parent component is missing or required parameters are not set.</exception>
    protected virtual void RegisterParent()
    {
        if (List == null)
            throw new InvalidOperationException("DataField must be child of DataList");

        if (Property == null)
            throw new InvalidOperationException("DataField Property parameter is required");

        ApplyInitialSort();

        // register with parent list
        List.AddField(this);
    }

    /// <summary>
    /// Removes this instance from the parent component.
    /// </summary>
    protected virtual void UnregisterParent()
    {
        List?.RemoveField(this);
    }

    /// <summary>
    /// Gets the formatted value for the specified data item.
    /// </summary>
    /// <param name="data">The data item.</param>
    /// <returns>The formatted value, or an empty string when the value cannot be resolved.</returns>
    internal string FormattedValue(TItem data)
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
            // nested null reference in the expression path
        }

        if (value == null)
            return string.Empty;

        // function takes precedence over format string, allowing for custom formatting logic beyond standard .NET formats
        if (FormatValue != null)
            return FormatValue.Invoke(value) ?? string.Empty;

        if (!string.IsNullOrEmpty(Format))
            return string.Format(CultureInfo.CurrentUICulture, $"{{0:{Format}}}", value);

        return value.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Updates the property metadata from the <see cref="Property"/> expression.
    /// </summary>
    private void UpdateProperty()
    {
        // only update if the Property expression has changed
        if (ReferenceEquals(_previousProperty, Property))
            return;

        _previousProperty = Property;
        _propertyAccessor = null;

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

        ExportName = string.IsNullOrEmpty(ExportHeader) ? PropertyName : ExportHeader;

        var columnAttribute = memberInfo.GetCustomAttribute<ColumnAttribute>(true);
        ColumnName = columnAttribute?.Name ?? PropertyName;

        UpdateHeaderName(memberInfo);
        UpdateFormat(memberInfo);
    }

    /// <summary>
    /// Updates the format string from a <see cref="DisplayFormatAttribute"/> when a format is not already set.
    /// </summary>
    /// <param name="memberInfo">The member the property expression resolves to.</param>
    private void UpdateFormat(MemberInfo memberInfo)
    {
        if (Format != null || FormatValue != null)
            return;

        var displayFormatAttribute = memberInfo.GetCustomAttribute<DisplayFormatAttribute>(true);
        if (displayFormatAttribute == null || string.IsNullOrEmpty(displayFormatAttribute.DataFormatString))
            return;

        Format = displayFormatAttribute.DataFormatString;
    }

    /// <summary>
    /// Updates the header name from <see cref="Title"/>, display attributes, or the property name.
    /// </summary>
    /// <param name="memberInfo">The member the property expression resolves to.</param>
    private void UpdateHeaderName(MemberInfo memberInfo)
    {
        // allow empty header, only default if null
        if (Title != null)
        {
            HeaderName = Title;
            return;
        }

        var displayAttribute = memberInfo.GetCustomAttribute<DisplayAttribute>(true);
        var displayName = displayAttribute?.GetName();
        if (!string.IsNullOrEmpty(displayName))
        {
            HeaderName = displayName;
            return;
        }

        var displayNameAttribute = memberInfo.GetCustomAttribute<DisplayNameAttribute>(true);
        if (!string.IsNullOrEmpty(displayNameAttribute?.DisplayName))
        {
            HeaderName = displayNameAttribute.DisplayName;
            return;
        }

        HeaderName = PropertyName.ToTitle();
    }
}
