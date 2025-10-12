using System.Text.Json.Serialization;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Serves as the abstract base class for all query rules in data-bound components.
/// Supports polymorphic serialization for <see cref="QueryGroup"/> and <see cref="QueryFilter"/>.
/// </summary>
[JsonDerivedType(typeof(QueryGroup))]
[JsonDerivedType(typeof(QueryFilter))]
public abstract class QueryRule
{
    /// <summary>
    /// Gets or sets the unique identifier for this query rule instance.
    /// Used for UI tracking and internal logic; not serialized.
    /// </summary>
    [JsonIgnore]
    public string Id { get; set; } = Identifier.Random();
}
