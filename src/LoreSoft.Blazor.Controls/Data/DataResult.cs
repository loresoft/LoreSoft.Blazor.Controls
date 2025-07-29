namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents the result of a data query for a data-bound component, including the total item count and the returned items.
/// </summary>
/// <typeparam name="TItem">The type of the data item.</typeparam>
/// <param name="Total">The total number of items available, regardless of paging.</param>
/// <param name="Items">The collection of items returned for the current query or page.</param>
public record DataResult<TItem>(int Total, IEnumerable<TItem> Items);
