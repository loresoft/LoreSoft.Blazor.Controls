#nullable enable

using LoreSoft.Blazor.Controls.Utilities;

using System.Linq.Dynamic.Core;
using System.Text;

namespace LoreSoft.Blazor.Controls.Extensions;

public static class QueryExtensions
{
    public static IQueryable<T> Sort<T>(this IQueryable<T> query, DataSort? sort)
    {
        if (sort == null)
            return query;

        return Sort(query, [sort]);
    }

    public static IQueryable<T> Sort<T>(this IQueryable<T> query, IEnumerable<DataSort>? sorts)
    {
        if (sorts?.Any() != true)
            return query;

        if (query is null)
            throw new ArgumentNullException(nameof(query));

        // Create ordering expression e.g. Field1 asc, Field2 desc
        var builder = new StringBuilder();
        foreach (var sort in sorts)
        {
            if (builder.Length > 0)
                builder.Append(",");

            builder.Append(sort.Property).Append(" ");

            var isDescending = sort.Descending;

            builder.Append(isDescending ? "desc" : "asc");
        }

        return query.OrderBy(builder.ToString());
    }

    public static IQueryable<T> Filter<T>(this IQueryable<T> query, QueryRule filter)
    {
        if (filter is null)
            return query;

        if (query is null)
            throw new ArgumentNullException(nameof(query));

        var builder = new LinqExpressionBuilder();
        builder.Build(filter);

        var predicate = builder.Expression;
        var parameters = builder.Parameters.ToArray();

        // nothing to filter
        if (string.IsNullOrWhiteSpace(predicate))
            return query;

        return query.Where(predicate, parameters);
    }
}
