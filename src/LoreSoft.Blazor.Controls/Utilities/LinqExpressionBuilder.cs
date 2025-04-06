// Ignore Spelling: Linq

using System.Text;

using LoreSoft.Blazor.Controls.Extensions;

namespace LoreSoft.Blazor.Controls.Utilities;


public class LinqExpressionBuilder
{
    private static readonly Dictionary<string, Action<StringBuilder, List<object?>, QueryFilter>> _filterWriters = new(StringComparer.OrdinalIgnoreCase);

    static LinqExpressionBuilder()
    {
        _filterWriters.TryAdd(QueryOperators.Contains, WriteStringFilter);
        _filterWriters.TryAdd(QueryOperators.NotContains, WriteStringFilter);
        _filterWriters.TryAdd(QueryOperators.StartsWith, WriteStringFilter);
        _filterWriters.TryAdd(QueryOperators.NotStartsWith, WriteStringFilter);
        _filterWriters.TryAdd(QueryOperators.EndsWith, WriteStringFilter);
        _filterWriters.TryAdd(QueryOperators.NotEndsWith, WriteStringFilter);

        _filterWriters.TryAdd(QueryOperators.Equal, WriteStandardFilter);
        _filterWriters.TryAdd(QueryOperators.NotEqual, WriteStandardFilter);
        _filterWriters.TryAdd(QueryOperators.GreaterThan, WriteStandardFilter);
        _filterWriters.TryAdd(QueryOperators.GreaterThanOrEqual, WriteStandardFilter);
        _filterWriters.TryAdd(QueryOperators.LessThan, WriteStandardFilter);
        _filterWriters.TryAdd(QueryOperators.LessThanOrEqual, WriteStandardFilter);

        _filterWriters.TryAdd(QueryOperators.IsNull, WriteNullFilter);
        _filterWriters.TryAdd(QueryOperators.IsNotNull, WriteNullFilter);
    }

    public static void RegisterWriter(string @operator, Action<StringBuilder, List<object?>, QueryFilter> writer)
    {
        _filterWriters[@operator] = writer;
    }


    public static bool IsValid(QueryRule? queryRule)
    {
        if (queryRule is QueryGroup group)
            return IsValid(group);

        if (queryRule is QueryFilter filter)
            return IsValid(filter);

        return false;
    }

    public static bool IsValid(QueryGroup? queryGroup)
    {
        if (queryGroup == null || queryGroup.Filters.Count == 0)
            return false;

        return queryGroup.Filters.Any(IsValid);
    }

    public static bool IsValid(QueryFilter? queryFilter)
    {
        if (queryFilter == null)
            return false;

        return queryFilter.Field.HasValue();
    }


    private readonly StringBuilder _expression = new();
    private readonly List<object?> _values = [];

    /// <summary>
    /// Gets the expression parameters.
    /// </summary>
    /// <value>
    /// The expression parameters.
    /// </value>
    public IReadOnlyList<object?> Parameters => _values;

    /// <summary>
    /// Gets the Linq expression string.
    /// </summary>
    /// <value>
    /// The Linq expression string.
    /// </value>
    public string Expression => _expression.ToString();

    /// <summary>
    /// Builds a string base Linq expression from the specified <see cref="QueryRule"/>.
    /// </summary>
    /// <param name="queryRule">The query to build expression from.</param>
    public void Build(QueryRule? queryRule)
    {
        _expression.Length = 0;
        _values.Clear();

        if (queryRule == null)
            return;

        Visit(queryRule);
    }


    private void Visit(QueryRule queryRule)
    {
        if (queryRule == null)
            return;

        if (queryRule is QueryGroup group)
            WriteGroup(group);
        else if (queryRule is QueryFilter filter)
            WriteExpression(filter);
    }

    private void WriteGroup(QueryGroup entityFilter)
    {
        var filters = entityFilter.Filters;

        if (filters == null || filters.Count == 0)
            return;

        var logic = entityFilter.Logic.IsNullOrWhiteSpace()
            ? QueryLogic.And
            : entityFilter.Logic;

        var wroteFirst = false;

        _expression.Append('(');
        foreach (var filter in filters)
        {
            if (wroteFirst)
                _expression.Append(' ').Append(logic).Append(' ');

            Visit(filter);
            wroteFirst = true;
        }
        _expression.Append(')');
    }

    private void WriteExpression(QueryFilter filter)
    {
        // Field require for expression
        if (string.IsNullOrWhiteSpace(filter.Field))
            return;

        // default comparison equal
        var comparison = filter.Operator;
        if (comparison.IsNullOrEmpty())
            comparison = QueryOperators.Equal;

        if (_filterWriters.TryGetValue(comparison, out var action))
            action(_expression, _values, filter);
        else
            WriteStandardFilter(_expression, _values, filter);
    }

    private static void WriteStringFilter(StringBuilder builder, List<object?> parameters, QueryFilter filter)
    {
        // Field require for expression
        if (string.IsNullOrWhiteSpace(filter.Field))
            return;

        int index = parameters.Count;

        var field = filter.Field;
        var value = filter.Value;

        var method = filter.Operator switch
        {
            QueryOperators.StartsWith => "StartsWith",
            QueryOperators.NotStartsWith => "StartsWith",
            QueryOperators.EndsWith => "EndsWith",
            QueryOperators.NotEndsWith => "EndsWith",
            QueryOperators.Contains => "Contains",
            QueryOperators.NotContains => "Contains",
            _ => "Contains"
        };

        var negation = filter.Operator switch
        {
            QueryOperators.NotContains => true,
            QueryOperators.NotStartsWith => true,
            QueryOperators.NotEndsWith => true,
            _ => false
        };

        builder
            .AppendIf("!", negation)
            .Append(field)
            .Append('.')
            .Append(method)
            .Append("(@")
            .Append(index)
            .Append(", StringComparison.OrdinalIgnoreCase")
            .Append(')');

        parameters.Add(value);
    }

    private static void WriteStandardFilter(StringBuilder builder, List<object?> parameters, QueryFilter filter)
    {
        // Field require for expression
        if (string.IsNullOrWhiteSpace(filter.Field))
            return;

        int index = parameters.Count;

        var field = filter.Field;
        var value = filter.Value;

        var comparison = filter.Operator switch
        {
            QueryOperators.Equal => "==",
            QueryOperators.NotEqual => "!=",
            QueryOperators.GreaterThan => ">",
            QueryOperators.GreaterThanOrEqual => ">=",
            QueryOperators.LessThan => "<",
            QueryOperators.LessThanOrEqual => "<=",
            _ => "=="
        };

        builder
            .Append(field)
            .Append(' ')
            .Append(comparison)
            .Append(" @")
            .Append(index);

        parameters.Add(value);
    }

    private static void WriteNullFilter(StringBuilder builder, List<object?> parameters, QueryFilter filter)
    {
        // Field require for expression
        if (string.IsNullOrWhiteSpace(filter.Field))
            return;

        int index = parameters.Count;

        var field = filter.Field;
        var value = filter.Value;

        var comparison = filter.Operator switch
        {
            QueryOperators.IsNull => "==",
            QueryOperators.IsNotNull => "!=",
            _ => "=="
        };

        builder
            .Append(field)
            .Append(' ')
            .Append(comparison)
            .Append(" NULL");
    }
}
