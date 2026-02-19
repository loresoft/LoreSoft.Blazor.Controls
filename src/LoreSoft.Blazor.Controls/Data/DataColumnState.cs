namespace LoreSoft.Blazor.Controls;

public record DataColumnState
{
    public DataColumnState(
        string columnName,
        int sortIndex,
        bool sortDescending,
        bool visible)
    {
        PropertyName = columnName;
        SortIndex = sortIndex;
        SortDescending = sortDescending;
        Visible = visible;
    }

    public string PropertyName { get; init; }

    public int SortIndex { get; init; }

    public bool SortDescending { get; init; }

    public bool Visible { get; init; }
}
