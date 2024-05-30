namespace LoreSoft.Blazor.Controls;

public record DataResult<TItem>(int Total, IEnumerable<TItem> Items);
