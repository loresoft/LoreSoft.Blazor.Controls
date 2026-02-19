using System.Linq.Dynamic.Core;
using System.Text;

using LoreSoft.Blazor.Controls.Utilities;

namespace LoreSoft.Blazor.Controls.Extensions;

/// <summary>
/// Provides extension methods for querying, sorting, and filtering <see cref="IQueryable{T}"/> sources.
/// </summary>
public static class QueryExtensions
{
    /// <summary>
    /// Applies a single <see cref="DataSort"/> to the query.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="sort">The sort definition to apply.</param>
    /// <returns>The sorted query, or the original query if <paramref name="sort"/> is <c>null</c>.</returns>
    public static IQueryable<T> Sort<T>(this IQueryable<T> query, DataSort? sort)
    {
        if (sort == null)
            return query;

        return Sort(query, [sort]);
    }

    /// <summary>
    /// Applies multiple <see cref="DataSort"/> definitions to the query.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="sorts">A collection of sort definitions to apply.</param>
    /// <returns>The sorted query, or the original query if <paramref name="sorts"/> is <c>null</c> or empty.</returns>
    public static IQueryable<T> Sort<T>(this IQueryable<T> query, IEnumerable<DataSort>? sorts)
    {
        if (sorts?.Any() != true)
            return query;

        ArgumentNullException.ThrowIfNull(query);

        // Create ordering expression e.g. Field1 asc, Field2 desc
        return StringBuilder.Pool.Use(builder =>
        {
            foreach (var sort in sorts)
            {
                if (builder.Length > 0)
                    builder.Append(',');

                builder.Append(sort.Property).Append(' ');

                var isDescending = sort.Descending;

                builder.Append(isDescending ? "desc" : "asc");
            }

            return query.OrderBy(builder.ToString());
        });
    }

    /// <summary>
    /// Applies a filter to the query using a <see cref="QueryRule"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="filter">The filter rule to apply.</param>
    /// <returns>The filtered query, or the original query if <paramref name="filter"/> is <c>null</c> or empty.</returns>
    public static IQueryable<T> Filter<T>(this IQueryable<T> query, QueryRule? filter)
    {
        if (filter is null)
            return query;

        ArgumentNullException.ThrowIfNull(query);

        return LinqExpressionBuilder.Pool.Use(builder =>
        {
            builder.Build(filter);

            var predicate = builder.Expression;
            var parameters = builder.Parameters.ToArray();

            // nothing to filter
            if (string.IsNullOrWhiteSpace(predicate))
                return query;

            return query.Where(predicate, parameters);
        });
    }

    /// <summary>
    /// Executes a data query with filtering, sorting, and paging based on a <see cref="DataRequest"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="request">The data request containing filter, sort, and paging information.</param>
    /// <returns>A <see cref="DataResult{T}"/> containing the total count and the paged, sorted, and filtered results.</returns>
    public static DataResult<T> DataQuery<T>(this IQueryable<T> query, DataRequest request)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(request);

        var filterQuery = query.Filter(request.Query);

        var total = filterQuery.Count();

        if (total == 0)
            return DataResult<T>.Empty;

        var sortedQuery = filterQuery.Sort(request.Sorts);

        if (request.Page > 0 && request.PageSize > 0)
            sortedQuery = sortedQuery.Page(request.Page.Value, request.PageSize);

        var results = sortedQuery.ToList();

        return new DataResult<T>(results, total);
    }
}
