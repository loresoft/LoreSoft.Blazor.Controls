using System.Text.Json.Serialization;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents a logical group of query rules for building complex filter expressions in data-bound components.
/// A group can contain multiple filters and/or subgroups, combined using a logical operator (AND/OR).
/// Inherits from <see cref="QueryRule"/>.
/// </summary>
public class QueryGroup : QueryRule
{
    /// <summary>
    /// Gets or sets the logical operator used to combine the filters in this group.
    /// Typically <see cref="QueryLogic.And"/> or <see cref="QueryLogic.Or"/>.
    /// </summary>
    [JsonPropertyName("logic")]
    public string? Logic { get; set; } = QueryLogic.And;

    /// <summary>
    /// Gets or sets the list of filters and/or subgroups contained in this group.
    /// Each item is a <see cref="QueryRule"/>, which may be a <see cref="QueryFilter"/> or another <see cref="QueryGroup"/>.
    /// </summary>
    [JsonPropertyName("filters")]
    public List<QueryRule> Filters { get; set; } = [];
}
