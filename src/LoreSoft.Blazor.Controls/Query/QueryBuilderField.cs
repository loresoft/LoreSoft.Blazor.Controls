using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

using LoreSoft.Blazor.Controls.Extensions;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents a field definition for the <see cref="QueryBuilder{TItem}"/> component.
/// Provides configuration for field selection, operators, input type, values, and templates for building query/filter expressions.
/// </summary>
/// <typeparam name="TItem">The type of the data item being queried.</typeparam>
public class QueryBuilderField<TItem> : ComponentBase
{
    /// <summary>
    /// Gets or sets the parent <see cref="QueryBuilder{TItem}"/> component.
    /// </summary>
    [CascadingParameter(Name = "QueryBuilder")]
    protected QueryBuilder<TItem> QueryBuilder { get; set; } = null!;

    /// <summary>
    /// Gets or sets the property expression for the field. Required.
    /// </summary>
    [Parameter, EditorRequired]
    public required Expression<Func<TItem, object>> Field { get; set; }

    /// <summary>
    /// Gets or sets the list of supported operators for this field.
    /// If not set, defaults are determined by the field type.
    /// </summary>
    [Parameter]
    public List<string>? Operators { get; set; }

    /// <summary>
    /// Gets or sets the input type for the field (e.g., "text", "number", "date").
    /// If not set, inferred from the field type.
    /// </summary>
    [Parameter]
    public string? InputType { get; set; }

    /// <summary>
    /// Gets or sets the display title for the field.
    /// If not set, inferred from the property name.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the list of selectable values for the field.
    /// </summary>
    [Parameter]
    public List<string>? Values { get; set; }

    /// <summary>
    /// Gets or sets the template for rendering the value input for this field.
    /// </summary>
    [Parameter]
    public RenderFragment<QueryFilter>? ValueTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for rendering the operator selector for this field.
    /// </summary>
    [Parameter]
    public RenderFragment<QueryFilter>? OperatorTemplate { get; set; }

    /// <summary>
    /// Gets the name of the field (property name).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets the column name, which may be set by a <see cref="ColumnAttribute"/>.
    /// </summary>
    public string? Column { get; set; }

    /// <summary>
    /// Gets the type of the field.
    /// </summary>
    public Type? Type { get; set; }

    /// <summary>
    /// Gets the current list of operators for the field.
    /// </summary>
    public List<string>? CurrentOperators { get; set; }

    /// <summary>
    /// Gets the current input type for the field.
    /// </summary>
    public string? CurrentInputType { get; set; }

    /// <summary>
    /// Gets the current display title for the field.
    /// </summary>
    public string? CurrentTitle { get; set; }

    /// <summary>
    /// Gets the current list of selectable values for the field.
    /// </summary>
    public List<string>? CurrentValues { get; set; }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        if (QueryBuilder == null)
            throw new InvalidOperationException("QueryColumn must be child of QueryBuilder");

        if (Field == null)
            throw new InvalidOperationException("QueryColumn Property parameter is required");

        // register with parent grid
        QueryBuilder.AddField(this);
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        UpdateProperty();
        UpdateOperators();
        UpdateInputType();
        UpdateTitle();
        UpdateValues();
    }

    /// <summary>
    /// Updates the list of selectable values for the field.
    /// </summary>
    private void UpdateValues()
    {
        if (Values?.Count > 0)
        {
            CurrentValues = Values;
            return;
        }

        CurrentValues = [];
    }

    /// <summary>
    /// Updates the display title for the field.
    /// </summary>
    private void UpdateTitle()
    {
        if (Title.HasValue())
        {
            CurrentTitle = Title;
            return;
        }

        CurrentTitle = Name.ToTitle();
    }

    /// <summary>
    /// Updates the input type for the field.
    /// </summary>
    private void UpdateInputType()
    {
        if (InputType.HasValue())
        {
            CurrentInputType = InputType;
            return;
        }

        CurrentInputType = QueryBuilderField<TItem>.GetInputType(Type);
    }

    /// <summary>
    /// Updates the list of supported operators for the field, based on type or provided values.
    /// </summary>
    private void UpdateOperators()
    {
        if (Operators != null && Operators.Count > 0)
        {
            CurrentOperators = Operators;
            return;
        }

        CurrentOperators = [QueryOperators.Equal, QueryOperators.NotEqual];

        if (Type == typeof(string))
        {
            CurrentOperators.Add(QueryOperators.Contains);
            CurrentOperators.Add(QueryOperators.NotContains);

            CurrentOperators.Add(QueryOperators.StartsWith);
            CurrentOperators.Add(QueryOperators.EndsWith);
        }
        else if (QueryBuilderField<TItem>.IsComparableType(Type))
        {
            CurrentOperators.Add(QueryOperators.GreaterThan);
            CurrentOperators.Add(QueryOperators.GreaterThanOrEqual);

            CurrentOperators.Add(QueryOperators.LessThan);
            CurrentOperators.Add(QueryOperators.LessThanOrEqual);
        }

        CurrentOperators.Add(QueryOperators.IsNull);
        CurrentOperators.Add(QueryOperators.IsNotNull);
    }

    /// <summary>
    /// Updates the property metadata for the field.
    /// </summary>
    private void UpdateProperty()
    {
        MemberInfo? memberInfo = null;

        if (Field?.Body is MemberExpression memberExpression)
            memberInfo = memberExpression.Member;
        else if (Field?.Body is UnaryExpression { Operand: MemberExpression memberOperand })
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
    /// Determines whether the specified type is comparable (supports range operators).
    /// </summary>
    /// <param name="targetType">The type to check.</param>
    /// <returns>True if the type is comparable; otherwise, false.</returns>
    private static bool IsComparableType(Type? targetType)
    {
        if (targetType == null)
            return false;

        var type = targetType.GetUnderlyingType();

        if (type == typeof(int))
            return true;
        if (type == typeof(long))
            return true;
        if (type == typeof(short))
            return true;
        if (type == typeof(float))
            return true;
        if (type == typeof(double))
            return true;
        if (type == typeof(decimal))
            return true;
        if (type == typeof(DateTime))
            return true;
        if (type == typeof(DateTimeOffset))
            return true;
        if (type == typeof(DateOnly))
            return true;
        if (type == typeof(TimeOnly))
            return true;
        if (type == typeof(TimeSpan))
            return true;

        return false;
    }

    /// <summary>
    /// Gets the recommended input type for the specified field type.
    /// </summary>
    /// <param name="targetType">The type to evaluate.</param>
    /// <returns>The input type string (e.g., "text", "number", "date", "time").</returns>
    private static string GetInputType(Type? targetType)
    {
        if (targetType == null)
            return "text";

        var type = targetType.GetUnderlyingType();

        if (type == typeof(int))
            return "number";
        if (type == typeof(long))
            return "number";
        if (type == typeof(short))
            return "number";
        if (type == typeof(float))
            return "number";
        if (type == typeof(double))
            return "number";
        if (type == typeof(decimal))
            return "number";
        if (type == typeof(DateTime))
            return "date";
        if (type == typeof(DateTimeOffset))
            return "date";
        if (type == typeof(DateOnly))
            return "date";
        if (type == typeof(TimeOnly))
            return "time";
        if (type == typeof(TimeSpan))
            return "time";

        return "text";
    }
}
