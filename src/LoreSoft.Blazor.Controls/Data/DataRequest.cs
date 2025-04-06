namespace LoreSoft.Blazor.Controls;

public record DataRequest(int Page, int PageSize, DataSort[]? Sorts, QueryGroup? Query, CancellationToken CancellationToken);
