namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents the result of a data query for a data-bound component, including the total item count and the returned items.
/// </summary>
/// <typeparam name="TItem">The type of the data item.</typeparam>
public record DataResult<TItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataResult{TItem}"/> class.
    /// </summary>
    /// <param name="total">The total number of items available in the data source.</param>
    /// <param name="items">The collection of items returned by the query.</param>
    public DataResult(int? total, IEnumerable<TItem> items)
    {
        Total = total;
        Items = items;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataResult{TItem}"/> class.
    /// </summary>
    /// <param name="items">The collection of items returned by the query.</param>
    /// <param name="total">The total number of items available in the data source.</param>
    /// <param name="continuationToken">An optional token used for pagination in token-based paging scenarios.</param>
    public DataResult(
        IEnumerable<TItem> items,
        int? total = null,
        string? continuationToken = null)
    {
        Items = items;
        Total = total;
        ContinuationToken = continuationToken;
    }

    /// <summary>
    /// Gets the collection of items returned by the query.
    /// </summary>
    public IEnumerable<TItem> Items { get; }

    /// <summary>
    /// Gets the total number of items available in the data source, or <c>null</c> if the total is unknown.
    /// </summary>
    public int? Total { get; }

    /// <summary>
    /// Gets the continuation token used for token-based pagination, or <c>null</c> if not applicable.
    /// </summary>
    public string? ContinuationToken { get; }
};
