namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents a delegate that asynchronously provides paged, sorted, and filtered data for a data-bound component.
/// </summary>
/// <typeparam name="TItem">The type of the data item.</typeparam>
/// <param name="request">The <see cref="DataRequest"/> containing paging, sorting, filtering, and cancellation information.</param>
/// <returns>
/// A <see cref="ValueTask{TResult}"/> that resolves to a <see cref="DataResult{TItem}"/> containing the result items and total count.
/// </returns>
public delegate ValueTask<DataResult<TItem>> DataProviderDelegate<TItem>(DataRequest request);
