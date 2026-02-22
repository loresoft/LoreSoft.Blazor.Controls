using System.Text.Json.Serialization;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents the persisted state of a single <see cref="DataColumn{TItem}"/> within a <see cref="DataGrid{TItem}"/>.
/// Instances are serialized to storage when state is saved and used to restore sort order
/// and visibility when state is loaded.
/// </summary>
public record DataColumnState
{
    [JsonConstructor]
    public DataColumnState(
        string propertyName,
        int sortIndex,
        bool sortDescending,
        bool visible,
        int index)
    {
        PropertyName = propertyName;
        SortIndex = sortIndex;
        SortDescending = sortDescending;
        Visible = visible;
        Index = index;
    }

    /// <summary>
    /// Gets the property name of the column.
    /// This value is matched against <c>DataColumn.PropertyName</c> to identify the target column
    /// when restoring persisted state.
    /// </summary>
    [JsonPropertyName("propertyName")]
    public string PropertyName { get; }


    /// <summary>
    /// Gets the zero-based sort priority of the column, or <c>-1</c> if the column is not sorted.
    /// When multiple columns are sorted, lower index values indicate higher sort priority.
    /// </summary>
    [JsonPropertyName("sortIndex")]
    public int SortIndex { get; }

    /// <summary>
    /// Gets a value indicating whether the column is sorted in descending order.
    /// </summary>
    [JsonPropertyName("sortDescending")]
    public bool SortDescending { get; }

    /// <summary>
    /// Gets a value indicating whether the column is visible.
    /// </summary>
    [JsonPropertyName("visible")]
    public bool Visible { get; }

    /// <summary>
    /// Gets the zero-based index of the column within the grid.
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; }
}
