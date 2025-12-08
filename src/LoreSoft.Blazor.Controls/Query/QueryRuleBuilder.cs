using System.Diagnostics.CodeAnalysis;

using static System.Net.Mime.MediaTypeNames;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Provides methods for building query rules and groups for filtering operations.
/// </summary>
public static class QueryRuleBuilder
{
    /// <summary>
    /// Creates a query filter rule with the specified field, value, and operator.
    /// </summary>
    /// <typeparam name="T">The type of the filter value.</typeparam>
    /// <param name="field">The field name to filter on.</param>
    /// <param name="value">The value to filter by.</param>
    /// <param name="op">The comparison operator. Defaults to <see cref="QueryOperators.Equal"/>.</param>
    /// <param name="id">The unique identifier for the filter. If not provided, a random identifier is generated.</param>
    /// <returns>A <see cref="QueryFilter"/> instance.</returns>
    public static QueryRule? Filter<T>(
        string field,
        T? value,
        string? op = QueryOperators.Equal,
        string? id = null)
    {
        return new QueryFilter
        {
            Id = id ?? Identifier.Random(),
            Field = field,
            Operator = op ?? QueryOperators.Equal,
            Value = value,
        };
    }

    /// <summary>
    /// Creates a query group with OR logic for multiple values on the same field.
    /// </summary>
    /// <typeparam name="T">The type of the filter values.</typeparam>
    /// <param name="field">The field name to filter on.</param>
    /// <param name="values">The collection of values to filter by.</param>
    /// <param name="op">The comparison operator. Defaults to <see cref="QueryOperators.Equal"/>.</param>
    /// <param name="id">The unique identifier for the group. If not provided, a random identifier is generated.</param>
    /// <returns>A <see cref="QueryGroup"/> with OR logic, a single <see cref="QueryFilter"/> if only one value, or null if no values.</returns>
    public static QueryRule? Or<T>(
        string field,
        IEnumerable<T> values,
        string? op = QueryOperators.Equal,
        string? id = null)
    {
        return Group(field, values, QueryLogic.Or, op, id);
    }

    /// <summary>
    /// Creates a query group with AND logic for multiple values on the same field.
    /// </summary>
    /// <typeparam name="T">The type of the filter values.</typeparam>
    /// <param name="field">The field name to filter on.</param>
    /// <param name="values">The collection of values to filter by.</param>
    /// <param name="op">The comparison operator. Defaults to <see cref="QueryOperators.Equal"/>.</param>
    /// <param name="id">The unique identifier for the group. If not provided, a random identifier is generated.</param>
    /// <returns>A <see cref="QueryGroup"/> with AND logic, a single <see cref="QueryFilter"/> if only one value, or null if no values.</returns>
    public static QueryRule? And<T>(
        string field,
        IEnumerable<T> values,
        string? op = QueryOperators.Equal,
        string? id = null)
    {
        return Group(field, values, QueryLogic.And, op, id);
    }

    /// <summary>
    /// Creates a query group for multiple values on the same field with the specified logic.
    /// </summary>
    /// <typeparam name="T">The type of the filter values.</typeparam>
    /// <param name="field">The field name to filter on.</param>
    /// <param name="values">The collection of values to filter by.</param>
    /// <param name="logic">The logical operator to combine filters. Defaults to <see cref="QueryLogic.And"/>.</param>
    /// <param name="op">The comparison operator. Defaults to <see cref="QueryOperators.Equal"/>.</param>
    /// <param name="id">The unique identifier for the group. If not provided, a random identifier is generated.</param>
    /// <returns>A <see cref="QueryGroup"/> with the specified logic, a single <see cref="QueryFilter"/> if only one value, or null if no values.</returns>
    public static QueryRule? Group<T>(
        string field,
        IEnumerable<T> values,
        string logic = QueryLogic.And,
        string? op = QueryOperators.Equal,
        string? id = null)
    {
        if (field == null || values == null)
            return null;

        op ??= QueryOperators.Equal;
        var group = new QueryGroup
        {
            Id = id ?? Identifier.Random(),
            Logic = logic
        };

        foreach (var value in values)
        {
            if (value == null)
                continue;

            QueryFilter filter = new()
            {
                Field = field,
                Operator = op,
                Value = value
            };
            group.Filters.Add(filter);
        }

        // no need for group if only one filter
        if (group.Filters.Count == 1)
            return group.Filters[0];

        return group;
    }

    /// <summary>
    /// Creates a query group with OR logic for multiple query rules.
    /// </summary>
    /// <param name="rules">The collection of query rules to combine.</param>
    /// <returns>A <see cref="QueryGroup"/> with OR logic, a single rule if only one provided, or null if no rules.</returns>
    public static QueryRule? Or(
        params IEnumerable<QueryRule?> rules)
    {
        return Group(rules, QueryLogic.Or);
    }

    /// <summary>
    /// Creates a query group with AND logic for multiple query rules.
    /// </summary>
    /// <param name="rules">The collection of query rules to combine.</param>
    /// <returns>A <see cref="QueryGroup"/> with AND logic, a single rule if only one provided, or null if no rules.</returns>
    public static QueryRule? And(
        params IEnumerable<QueryRule?> rules)
    {
        return Group(rules, QueryLogic.And);
    }

    /// <summary>
    /// Creates a query group for multiple query rules with the specified logic.
    /// </summary>
    /// <param name="rules">The collection of query rules to combine.</param>
    /// <param name="logic">The logical operator to combine rules. Defaults to <see cref="QueryLogic.And"/>.</param>
    /// <param name="id">The unique identifier for the group. If not provided, a random identifier is generated.</param>
    /// <returns>A <see cref="QueryGroup"/> with the specified logic, a single rule if only one provided, or null if no rules.</returns>
    public static QueryRule? Group(
        IEnumerable<QueryRule?> rules,
        string logic = QueryLogic.And,
        string? id = null)
    {
        var groupRules = rules
            .OfType<QueryRule>()
            .ToList();

        // no rules to group
        if (groupRules.Count == 0)
            return null;

        // no need for group if only one filter
        if (groupRules.Count == 1)
            return groupRules[0];

        return new QueryGroup
        {
            Id = id ?? Identifier.Random(),
            Logic = logic,
            Filters = groupRules
        };
    }

    /// <summary>
    /// Casts or converts a <see cref="QueryRule"/> to a <see cref="QueryGroup"/> with the specified logic.
    /// </summary>
    /// <param name="rule">The query rule to convert.</param>
    /// <param name="logic">The logical operator to use for the group. Defaults to <see cref="QueryLogic.And"/>.</param>
    /// <returns>A <see cref="QueryGroup"/> representing the rule, or null if the rule is null.</returns>
    [return: NotNullIfNotNull(nameof(rule))]
    public static QueryGroup? AsGroup(
        this QueryRule? rule,
        string logic = QueryLogic.And)
    {
        if (rule == null)
            return null;

        if (rule is QueryGroup group)
            return group;

        return new QueryGroup
        {
            Id = rule.Id ?? Identifier.Random(),
            Logic = logic,
            Filters = [rule]
        };
    }
}
