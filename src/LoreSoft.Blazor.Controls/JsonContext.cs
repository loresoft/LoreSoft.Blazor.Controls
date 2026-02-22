using System.Text.Json.Serialization;

namespace LoreSoft.Blazor.Controls;

[JsonSerializable(typeof(QueryRule))]
[JsonSerializable(typeof(QueryGroup))]
[JsonSerializable(typeof(QueryFilter))]
[JsonSerializable(typeof(DataGridState))]
[JsonSerializable(typeof(DataColumnState))]
[JsonSerializable(typeof(DataColumnState))]
public partial class JsonContext : JsonSerializerContext;
