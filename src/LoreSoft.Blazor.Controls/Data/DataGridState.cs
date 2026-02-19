namespace LoreSoft.Blazor.Controls;

public record DataGridState
{
    public DataGridState(
        QueryGroup? query,
        DataColumnState[]? columns)
    {
        Query = query;
        Columns = columns;
    }

    public QueryGroup? Query { get; init; }

    public DataColumnState[]? Columns { get; init; }
}
