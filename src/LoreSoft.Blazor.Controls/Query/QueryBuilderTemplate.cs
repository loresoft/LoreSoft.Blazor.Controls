// Ignore Spelling: Searchable

using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

using LoreSoft.Blazor.Controls.Extensions;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents a field definition used by the <see cref="QueryBuilder"/> component.
/// </summary>
/// <remarks>
/// Provides configuration for field selection, operators, input type, selectable values, and editor templates.
/// </remarks>
public class QueryBuilderTemplate : ComponentBase
{
    private Func<object, object?>? _propertyAccessor;
    private Type? _dataType;
    private string? _proertyName;

    /// <summary>
    /// Gets or sets the parent <see cref="QueryBuilder"/> component.
    /// </summary>
    [CascadingParameter(Name = "QueryBuilder")]
    protected QueryBuilder QueryBuilder { get; set; } = null!;

    /// <summary>
    /// Gets or sets the list of supported operators for this field.
    /// </summary>
    /// <remarks>
    /// When not set, default operators are determined from <see cref="Type"/>.
    /// </remarks>
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
    /// Gets or sets the format string for values.
    /// </summary>
    [Parameter]
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the field is included in quick search.
    /// When true, this field will be searched during quick search operations.
    /// </summary>
    [Parameter]
    public bool Searchable { get; set; } = true;

    /// <summary>
    /// Gets or sets which field identifier is used for filtering and sorting.
    /// </summary>
    /// <remarks>
    /// Uses either <see cref="Column"/> or <see cref="Name"/>. The default is <see cref="QueryFieldSelection.Column"/>.
    /// </remarks>
    [Parameter]
    public QueryFieldSelection FieldSelection { get; set; } = QueryFieldSelection.Column;

    /// <summary>
    /// Gets or sets the name of the field.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the column name.
    /// </summary>
    [Parameter]
    public string? Column { get; set; }

    /// <summary>
    /// Gets or sets the type of the field.
    /// </summary>
    [Parameter]
    public Type? Type { get; set; } = typeof(string);

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

    /// <summary>
    /// Gets the current field identifier based on <see cref="FieldSelection"/>.
    /// </summary>
    public string? CurrentName => FieldSelection == QueryFieldSelection.Column ? Column : Name;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        if (QueryBuilder == null)
            throw new InvalidOperationException("QueryBuilderTemplate must be child of QueryBuilder");

        QueryBuilder.AddField(this);
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        UpdateField();
        UpdateOperators();
        UpdateInputType();
        UpdateTitle();
        UpdateValues();
    }

    /// <summary>
    /// Updates base field metadata before derived values are calculated.
    /// </summary>
    /// <remarks>
    /// Ensures required metadata is present and applies default values for <see cref="Type"/> and <see cref="Column"/>.
    /// </remarks>
    protected virtual void UpdateField()
    {
        if (Name.IsNullOrWhiteSpace())
            throw new InvalidOperationException("QueryBuilderTemplate Name parameter is required");

        Type ??= typeof(string);
        Column ??= Name;
    }

    /// <summary>
    /// Gets the display value for this field from the specified data item.
    /// </summary>
    /// <param name="data">The data item instance.</param>
    /// <returns>The formatted field value, or an empty string when no value is available.</returns>
    internal virtual string FieldValue(object? data)
    {
        if (data == null || Name.IsNullOrWhiteSpace())
            return string.Empty;

        var dataType = data.GetType();

        // Cache the value accessor for performance, but rebuild if the data type or field name changes
        if (_propertyAccessor == null || _dataType != dataType || _proertyName != Name)
        {
            _propertyAccessor = BuildValueAccessor(dataType, Name);
            _dataType = dataType;
            _proertyName = Name;
        }

        if (_propertyAccessor == null)
            return string.Empty;

        object? value = null;

        try
        {
            value = _propertyAccessor.Invoke(data);
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

        CurrentInputType = GetInputType(Type);
    }

    /// <summary>
    /// Updates the list of supported operators for the field.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="Operators"/> when provided; otherwise computes defaults from <see cref="Type"/>.
    /// </remarks>
    private void UpdateOperators()
    {
        if (Operators?.Count > 0)
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
        else if (IsComparableType(Type))
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

    /// <summary>
    /// Builds a compiled accessor function for retrieving a property value from an object instance using reflection and expression trees.
    /// </summary>
    /// <param name="dataType">The type containing the property to access.</param>
    /// <param name="fieldName">The name of the public instance property to retrieve.</param>
    /// <returns>
    /// A compiled function that accepts an object and returns the property value, or <see langword="null"/> if the property is not found.
    /// </returns>
    private static Func<object, object?>? BuildValueAccessor(Type dataType, string fieldName)
    {
        var propertyInfo = dataType.GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public);

        if (propertyInfo == null)
            return null;

        var input = Expression.Parameter(typeof(object), "data");
        var castInput = Expression.Convert(input, dataType);
        var propertyAccess = Expression.Property(castInput, propertyInfo);
        var castResult = Expression.Convert(propertyAccess, typeof(object));

        return Expression.Lambda<Func<object, object?>>(castResult, input).Compile();
    }
}
