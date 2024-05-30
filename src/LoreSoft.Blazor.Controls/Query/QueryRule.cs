using System.Text.Json.Serialization;

namespace LoreSoft.Blazor.Controls;

[JsonDerivedType(typeof(QueryGroup))]
[JsonDerivedType(typeof(QueryFilter))]
public abstract class QueryRule
{
    [JsonIgnore]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
}
