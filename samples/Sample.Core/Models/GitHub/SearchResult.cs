using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sample.Core.Models.GitHub;

public class SearchResult<T>
{
    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }

    [JsonPropertyName("incomplete_results")]
    public bool IncompleteResults { get; set; }

    [JsonPropertyName("items")]
    public List<T> Items { get; set; }
}
