using System.Text.Json;

using LoreSoft.Blazor.Controls;

using Xunit;

namespace LoreSoft.Blazor.Controls.Tests.Data;

public class DataGridStateTests
{
    // DataColumnState serialization

    [Fact]
    public void DataColumnState_WhenSerializedAndDeserialized_RoundTrips()
    {
        var state = new DataColumnState("Name", sortIndex: 0, sortDescending: false, visible: true, index: 0);

        var json = JsonSerializer.Serialize(state);
        var result = JsonSerializer.Deserialize<DataColumnState>(json);

        Assert.NotNull(result);
        Assert.Equal("Name", result.PropertyName);
        Assert.Equal(0, result.SortIndex);
        Assert.False(result.SortDescending);
        Assert.True(result.Visible);
    }

    [Fact]
    public void DataColumnState_WhenSortDescendingAndHidden_RoundTrips()
    {
        var state = new DataColumnState("Age", sortIndex: 1, sortDescending: true, visible: false, index: 1);

        var json = JsonSerializer.Serialize(state);
        var result = JsonSerializer.Deserialize<DataColumnState>(json);

        Assert.NotNull(result);
        Assert.Equal("Age", result.PropertyName);
        Assert.Equal(1, result.SortIndex);
        Assert.True(result.SortDescending);
        Assert.False(result.Visible);
    }

    [Fact]
    public void DataColumnState_WhenNotSorted_SortIndexIsNegativeOne()
    {
        var state = new DataColumnState("Status", sortIndex: -1, sortDescending: false, visible: true, index: 0);

        var json = JsonSerializer.Serialize(state);
        var result = JsonSerializer.Deserialize<DataColumnState>(json);

        Assert.NotNull(result);
        Assert.Equal(-1, result.SortIndex);
    }

    // DataGridState serialization

    [Fact]
    public void DataGridState_WithNullQueryAndNullColumns_RoundTrips()
    {
        var state = new DataGridState(query: null, columns: null);

        var json = JsonSerializer.Serialize(state);
        var result = JsonSerializer.Deserialize<DataGridState>(json);

        Assert.NotNull(result);
        Assert.Null(result.Query);
        Assert.Null(result.Columns);
        Assert.Empty(result.Extensions);
    }

    [Fact]
    public void DataGridState_WithColumns_RoundTrips()
    {
        List<DataColumnState> columns =
        [
            new("Name", sortIndex: 0, sortDescending: false, visible: true, index: 0),
            new("Age",  sortIndex: 1, sortDescending: true,  visible: false, index: 1),
        ];
        var state = new DataGridState(query: null, columns: columns);

        var json = JsonSerializer.Serialize(state);
        var result = JsonSerializer.Deserialize<DataGridState>(json);

        Assert.NotNull(result);
        Assert.NotNull(result.Columns);
        Assert.Equal(2, result.Columns.Count);
        Assert.Equal("Name", result.Columns[0].PropertyName);
        Assert.Equal(0,      result.Columns[0].SortIndex);
        Assert.False(result.Columns[0].SortDescending);
        Assert.True(result.Columns[0].Visible);
        Assert.Equal("Age", result.Columns[1].PropertyName);
        Assert.Equal(1,     result.Columns[1].SortIndex);
        Assert.True(result.Columns[1].SortDescending);
        Assert.False(result.Columns[1].Visible);
    }

    [Fact]
    public void DataGridState_WithQuery_RoundTrips()
    {
        var query = new QueryGroup
        {
            Logic = QueryLogic.Or,
            Filters =
            [
                new QueryFilter { Field = "Name", Operator = QueryOperators.Contains, Value = "Alice" }
            ]
        };
        var state = new DataGridState(query, columns: null);

        var json = JsonSerializer.Serialize(state);
        var result = JsonSerializer.Deserialize<DataGridState>(json);

        Assert.NotNull(result);
        Assert.NotNull(result.Query);
        Assert.Equal(QueryLogic.Or, result.Query.Logic);
        Assert.Single(result.Query.Filters);
        var filter = Assert.IsType<QueryFilter>(result.Query.Filters[0]);
        Assert.Equal("Name", filter.Field);
        Assert.Equal(QueryOperators.Contains, filter.Operator);
        Assert.Equal("Alice", filter.Value);
    }

    [Fact]
    public void DataGridState_WithExtensions_RoundTrips()
    {
        var extensions = new Dictionary<string, string?>
        {
            ["pageSize"] = "25",
            ["theme"]    = null
        };
        var state = new DataGridState(query: null, columns: null, extensions: extensions);

        var json = JsonSerializer.Serialize(state);
        var result = JsonSerializer.Deserialize<DataGridState>(json);

        Assert.NotNull(result);
        Assert.Equal(2, result.Extensions.Count);
        Assert.Equal("25", result.Extensions["pageSize"]);
        Assert.Null(result.Extensions["theme"]);
    }

    [Fact]
    public void DataGridState_WhenExtensionsNullInJson_DefaultsToEmptyDictionary()
    {
        const string json = """{"Query":null,"Columns":null,"Extensions":null}""";

        var result = JsonSerializer.Deserialize<DataGridState>(json);

        Assert.NotNull(result);
        Assert.NotNull(result.Extensions);
        Assert.Empty(result.Extensions);
    }

    [Fact]
    public void DataGridState_WhenExtensionsAbsentFromJson_DefaultsToEmptyDictionary()
    {
        const string json = """{"Query":null,"Columns":null}""";

        var result = JsonSerializer.Deserialize<DataGridState>(json);

        Assert.NotNull(result);
        Assert.NotNull(result.Extensions);
        Assert.Empty(result.Extensions);
    }

    [Fact]
    public void DataGridState_FullState_RoundTrips()
    {
        List<DataColumnState> columns = [new("Name", sortIndex: 0, sortDescending: false, visible: true, index: 0)];
        var query = new QueryGroup
        {
            Logic = QueryLogic.And,
            Filters = [new QueryFilter { Field = "Active", Operator = QueryOperators.Equal, Value = true }]
        };
        var extensions = new Dictionary<string, string?> { ["scrollPos"] = "100" };
        var state = new DataGridState(query, columns, extensions);

        var json = JsonSerializer.Serialize(state);
        var result = JsonSerializer.Deserialize<DataGridState>(json);

        Assert.NotNull(result);
        Assert.NotNull(result.Query);
        Assert.Equal(QueryLogic.And, result.Query.Logic);
        Assert.Single(result.Query.Filters);
        var fullFilter = Assert.IsType<QueryFilter>(result.Query.Filters[0]);
        Assert.Equal(true, fullFilter.Value);
        Assert.NotNull(result.Columns);
        Assert.Single(result.Columns);
        Assert.Equal("Name", result.Columns[0].PropertyName);
        Assert.Single(result.Extensions);
        Assert.Equal("100", result.Extensions["scrollPos"]);
    }
}
