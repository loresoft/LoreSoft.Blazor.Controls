using System.Text.Json.Serialization;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents the persisted state of a <see cref="DataGrid{TItem}"/>, capturing the active filter query,
/// per-column sort and visibility settings, and any additional state contributed by
/// <see cref="DataGrid{TItem}.StateSaving"/> subscribers via <see cref="Extensions"/>.
/// Instances are serialized to and deserialized from the configured storage provider.
/// </summary>
public record DataGridState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridState"/> record.
    /// </summary>
    /// <param name="query">The filter query that was active when the state was saved, or <see langword="null"/> if no filters were applied.</param>
    /// <param name="columns">The per-column state entries (sort index, sort direction, and visibility) captured at save time,
    /// or <see langword="null"/> if no column state was captured.</param>
    /// <param name="extensions">An optional dictionary of additional state entries written by
    /// <see cref="DataGrid{TItem}.StateSaving"/> subscribers. Defaults to an empty dictionary when <see langword="null"/>.</param>
    [JsonConstructor]
    public DataGridState(
        QueryGroup? query,
        List<DataColumnState>? columns,
        Dictionary<string, string?>? extensions = null)
    {
        Query = query;
        Columns = columns;
        Extensions = extensions ?? [];
    }

    /// <summary>
    /// Gets the filter query that was active when the state was saved.
    /// Restored to the grid's root query on load. <see langword="null"/> when no filters were active.
    /// </summary>
    [JsonPropertyName("query")]
    public QueryGroup? Query { get; }

    /// <summary>
    /// Gets the per-column state entries captured when the state was saved.
    /// Each entry stores a column's property name, sort index, sort direction, and visibility.
    /// <see langword="null"/> when no column state was captured.
    /// </summary>
    [JsonPropertyName("columns")]
    public List<DataColumnState>? Columns { get; }

    /// <summary>
    /// Gets a dictionary of additional state entries written by <see cref="DataGrid{TItem}.StateSaving"/> subscribers
    /// and read back by <see cref="DataGrid{TItem}.StateLoaded"/> subscribers.
    /// Keys and values are strings to ensure reliable JSON serialization across storage providers.
    /// </summary>
    [JsonPropertyName("extensions")]
    public Dictionary<string, string?> Extensions { get; }
}
