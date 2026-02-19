namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents a single sort column and direction entry in the column picker sort tab.
/// </summary>
public sealed class DataSortState
{
    /// <summary>
    /// Gets or sets the column name to sort by.
    /// Matches <see cref="DataColumn{TItem}.ColumnName"/>.
    /// </summary>
    public string ColumnName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sort direction.
    /// Valid values are <c>"asc"</c> for ascending and <c>"desc"</c> for descending.
    /// </summary>
    public string Direction { get; set; } = "asc";
}
