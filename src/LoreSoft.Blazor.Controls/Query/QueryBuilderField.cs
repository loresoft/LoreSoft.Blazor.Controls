using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents a strongly typed field definition for the <see cref="QueryBuilder"/> component.
/// </summary>
/// <typeparam name="TItem">The type of data item being queried.</typeparam>
/// <remarks>
/// Uses the <see cref="Field"/> expression to infer the field name, data type, and optional column mapping metadata.
/// </remarks>
[CascadingTypeParameter(nameof(TItem))]
public class QueryBuilderField<TItem> : QueryBuilderTemplate
{
    private Func<TItem, object>? _propertyAccessor;
    private Expression<Func<TItem, object>>? _currentField;

    /// <summary>
    /// Gets or sets the member access expression that identifies the field. This parameter is required.
    /// </summary>
    /// <remarks>
    /// The expression should reference a property or field on <typeparamref name="TItem"/>.
    /// </remarks>
    [Parameter, EditorRequired]
    public required Expression<Func<TItem, object>> Field { get; set; }

    /// <inheritdoc />
    protected override void UpdateField()
    {
        if (Field == null)
            throw new InvalidOperationException("QueryBuilderField Field parameter is required");

        if (ReferenceEquals(_currentField, Field))
            return;

        _currentField = Field;
        _propertyAccessor = null;

        MemberInfo? memberInfo = null;

        if (_currentField.Body is MemberExpression memberExpression)
            memberInfo = memberExpression.Member;
        else if (_currentField.Body is UnaryExpression { Operand: MemberExpression memberOperand })
            memberInfo = memberOperand.Member;

        if (memberInfo is PropertyInfo propertyInfo)
        {
            Name = propertyInfo.Name;
            Type = propertyInfo.PropertyType;
        }
        else if (memberInfo is FieldInfo fieldInfo)
        {
            Name = fieldInfo.Name;
            Type = fieldInfo.FieldType;
        }
        else
        {
            Name = memberInfo?.Name;
            Type = typeof(object);
        }

        var columnAttribute = memberInfo?.GetCustomAttribute<ColumnAttribute>(true);
        Column = columnAttribute != null ? columnAttribute.Name : Name;
    }

    /// <summary>
    /// Gets the formatted field value for the specified data item.
    /// </summary>
    /// <param name="data">The data item instance.</param>
    /// <returns>
    /// The formatted field value, or an empty string when the value cannot be resolved.
    /// </returns>
    internal override string FieldValue(object? data)
    {
        if (data is not TItem item || _currentField == null)
            return string.Empty;

        _propertyAccessor ??= _currentField.Compile();

        object? value = null;

        try
        {
            value = _propertyAccessor.Invoke(item);
        }
        catch (NullReferenceException)
        {

        }

        if (value is null)
            return string.Empty;

        return string.IsNullOrEmpty(Format)
            ? value.ToString() ?? string.Empty
            : string.Format(CultureInfo.CurrentCulture, $"{{0:{Format}}}", value);
    }
}
