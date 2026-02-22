using System.Text.Json.Serialization;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Serves as the abstract base class for all query rules in data-bound components.
/// Supports polymorphic serialization for <see cref="QueryGroup"/> and <see cref="QueryFilter"/>.
/// </summary>
[JsonDerivedType(typeof(QueryGroup), nameof(QueryGroup))]
[JsonDerivedType(typeof(QueryFilter), nameof(QueryFilter))]
public abstract class QueryRule
{
    /// <summary>
    /// Gets or sets the unique identifier for this query rule instance.
    /// Used for UI tracking and internal logic.
    /// </summary>
    public string Id { get; set; } = Identifier.Random();

    /// <summary>
    /// Gets or sets a value indicating whether the object is not persisted.
    /// </summary>
    [JsonIgnore]
    public bool IsTransient { get; set; }
}
