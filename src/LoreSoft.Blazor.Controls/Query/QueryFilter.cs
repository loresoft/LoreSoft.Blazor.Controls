using System.Text.Json.Serialization;

using LoreSoft.Blazor.Controls.Converters;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents a single filter condition for a query in a data-bound component.
/// Inherits from <see cref="QueryRule"/> and specifies the field, operator, and value for filtering.
/// </summary>
public class QueryFilter : QueryRule, IEquatable<QueryFilter?>
{
    /// <summary>
    /// Gets or sets the name of the field to filter on.
    /// </summary>
    [JsonPropertyName("field")]
    public string? Field { get; set; }

    /// <summary>
    /// Gets or sets the operator used for the filter (e.g., equals, contains, greater than).
    /// Defaults to <see cref="QueryOperators.Equal"/>.
    /// </summary>
    [JsonPropertyName("operator")]
    public string? Operator { get; set; } = QueryOperators.Equal;

    /// <summary>
    /// Gets or sets the value to compare against the field using the specified operator.
    /// </summary>
    [JsonPropertyName("value")]
    [JsonConverter(typeof(JsonObjectConverter))]
    public object? Value { get; set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return Equals(obj as QueryFilter);
    }

    /// <inheritdoc/>
    public bool Equals(QueryFilter? other)
    {
        return other is not null &&
               Field == other.Field &&
               Operator == other.Operator &&
               EqualityComparer<object?>.Default.Equals(Value, other.Value);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Field, Operator, Value);
    }
}
