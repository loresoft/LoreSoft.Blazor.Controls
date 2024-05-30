namespace LoreSoft.Blazor.Controls;

public delegate ValueTask<DataResult<TItem>> DataProviderDelegate<TItem>(DataRequest request);
