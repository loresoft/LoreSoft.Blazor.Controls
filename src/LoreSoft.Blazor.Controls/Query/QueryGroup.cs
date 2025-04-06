using System.Text.Json.Serialization;

namespace LoreSoft.Blazor.Controls;

public class QueryGroup : QueryRule
{
    [JsonPropertyName("logic")]
    public string? Logic { get; set; } = QueryLogic.And;

    [JsonPropertyName("filters")]
    public List<QueryRule> Filters { get; set; } = [];
}
