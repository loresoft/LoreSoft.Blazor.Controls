using System.Text.Json.Serialization;

namespace LoreSoft.Blazor.Controls;

public class QueryFilter : QueryRule
{
    [JsonPropertyName("field")]
    public string? Field { get; set; }

    [JsonPropertyName("operator")]
    public string? Operator { get; set; } = QueryOperators.Equal;

    [JsonPropertyName("value")]
    public object? Value { get; set; }
}
