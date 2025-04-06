using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

using LoreSoft.Blazor.Controls.Extensions;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

public class QueryBuilderField<TItem> : ComponentBase
{
    [CascadingParameter(Name = "QueryBuilder")]
    protected QueryBuilder<TItem> QueryBuilder { get; set; } = null!;

    [Parameter, EditorRequired]
    public required Expression<Func<TItem, object>> Field { get; set; }

    [Parameter]
    public List<string>? Operators { get; set; }

    [Parameter]
    public string? InputType { get; set; }

    [Parameter]
    public string? Title { get; set; }


    [Parameter]
    public RenderFragment<QueryFilter>? ValueTemplate { get; set; }

    [Parameter]
    public RenderFragment<QueryFilter>? OperatorTemplate { get; set; }


    public string? Name { get; set; }

    public string? Column { get; set; }

    public Type? Type { get; set; }


    public List<string>? CurrentOperators { get; set; }

    public string? CurrentInputType { get; set; }

    public string? CurrentTitle { get; set; }


    protected override void OnInitialized()
    {
        if (QueryBuilder == null)
            throw new InvalidOperationException("QueryColumn must be child of QueryBuilder");

        if (Field == null)
            throw new InvalidOperationException("QueryColumn Property parameter is required");

        // register with parent grid
        QueryBuilder.AddField(this);
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        UpdateProperty();
        UpdateOperators();
        UpdateInputType();
        UpdateTitle();
    }


    private void UpdateTitle()
    {
        if (Title.HasValue())
        {
            CurrentTitle = Title;
            return;
        }

        CurrentTitle = Name.ToTitle();
    }

    private void UpdateInputType()
    {
        if (InputType.HasValue())
        {
            CurrentInputType = InputType;
            return;
        }

        CurrentInputType = GetInputType(Type);
    }

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


    private bool IsComparableType(Type? targetType)
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

    private string GetInputType(Type? targetType)
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
