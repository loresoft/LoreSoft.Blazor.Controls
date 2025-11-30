namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents a request for data in a data-bound component, including paging, sorting, filtering, and cancellation information.
/// </summary>
public record DataRequest
{
    /// <summary>
    /// Gets or initializes the current page number (1-based), or <c>null</c> if not using page-based pagination.
    /// </summary>
    public int? Page { get; init; }

    /// <summary>
    /// Gets or initializes the number of items per page.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Gets or initializes an optional token used for fetching the next set of results in a token-based paginated query.
    /// </summary>
    public string? ContinuationToken { get; init; }

    /// <summary>
    /// Gets or initializes an array of <see cref="DataSort"/> objects specifying the sort order for the data, or <c>null</c> if no sorting is applied.
    /// </summary>
    public DataSort[]? Sorts { get; init; }

    /// <summary>
    /// Gets or initializes a <see cref="QueryGroup"/> representing the filter rules to apply to the data, or <c>null</c> if no filtering is applied.
    /// </summary>
    public QueryGroup? Query { get; init; }

    /// <summary>
    /// Gets or initializes a <see cref="CancellationToken"/> to observe while waiting for the data request to complete.
    /// </summary>
    public CancellationToken CancellationToken { get; init; } = CancellationToken.None;
};
