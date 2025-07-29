using System.Text.Json.Serialization;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents a single filter condition for a query in a data-bound component.
/// Inherits from <see cref="QueryRule"/> and specifies the field, operator, and value for filtering.
/// </summary>
public class QueryFilter : QueryRule
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
    public object? Value { get; set; }
}
