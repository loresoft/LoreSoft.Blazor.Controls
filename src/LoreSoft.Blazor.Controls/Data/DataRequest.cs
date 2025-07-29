namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents a request for data in a data-bound component, including paging, sorting, filtering, and cancellation information.
/// </summary>
/// <param name="Page">The current page number (1-based).</param>
/// <param name="PageSize">The number of items per page.</param>
/// <param name="Sorts">An array of <see cref="DataSort"/> objects specifying the sort order for the data.</param>
/// <param name="Query">A <see cref="QueryGroup"/> representing the filter rules to apply to the data.</param>
/// <param name="CancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
public record DataRequest(
    int Page,
    int PageSize,
    DataSort[]? Sorts,
    QueryGroup? Query,
    CancellationToken CancellationToken
);
