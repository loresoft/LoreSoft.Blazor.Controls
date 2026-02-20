using System.Text.Json;

using LoreSoft.Blazor.Controls.Extensions;

namespace LoreSoft.Blazor.Controls.Tests.Query;

public class QueryFilterTests
{
    private static object? RoundTrip(object? value)
    {
        var filter = new QueryFilter { Value = value };
        var json = JsonSerializer.Serialize(filter);
        return JsonSerializer.Deserialize<QueryFilter>(json)!.Value;
    }

    private static string RawValueJson(object? value)
    {
        var filter = new QueryFilter { Value = value };
        var doc = JsonSerializer.SerializeToDocument(filter);
        return doc.RootElement.GetProperty("value").GetRawText();
    }

    // --- Write format ---

    [Fact]
    public void Write_WhenNull_WritesJsonNull()
    {
        Assert.Equal("null", RawValueJson(null));
    }

    [Fact]
    public void Write_WhenString_WritesTwoElementArray()
    {
        Assert.Equal($$"""["{{typeof(string).GetPortableName()}}","hello"]""", RawValueJson("hello"));
    }

    [Fact]
    public void Write_WhenBoolTrue_WritesTwoElementArray()
    {
        Assert.Equal($$"""["{{typeof(bool).GetPortableName()}}",true]""", RawValueJson(true));
    }

    [Fact]
    public void Write_WhenBoolFalse_WritesTwoElementArray()
    {
        Assert.Equal($$"""["{{typeof(bool).GetPortableName()}}",false]""", RawValueJson(false));
    }

    [Fact]
    public void Write_WhenInt32_WritesTwoElementArray()
    {
        Assert.Equal($$"""["{{typeof(int).GetPortableName()}}",42]""", RawValueJson(42));
    }

    // --- Round-trip: null ---

    [Fact]
    public void RoundTrip_Null_ReturnsNull()
    {
        Assert.Null(RoundTrip(null));
    }

    // --- Round-trip: primitives ---

    [Theory]
    [InlineData("")]
    [InlineData("hello")]
    [InlineData("  spaces  ")]
    public void RoundTrip_String_PreservesValueAndType(string input)
    {
        var typed = Assert.IsType<string>(RoundTrip(input));
        Assert.Equal(input, typed);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RoundTrip_Bool_PreservesValueAndType(bool input)
    {
        var typed = Assert.IsType<bool>(RoundTrip(input));
        Assert.Equal(input, typed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void RoundTrip_Int32_PreservesValueAndType(int input)
    {
        var typed = Assert.IsType<int>(RoundTrip(input));
        Assert.Equal(input, typed);
    }

    [Theory]
    [InlineData(long.MinValue)]
    [InlineData(long.MaxValue)]
    public void RoundTrip_Int64_PreservesValueAndType(long input)
    {
        var typed = Assert.IsType<long>(RoundTrip(input));
        Assert.Equal(input, typed);
    }

    [Fact]
    public void RoundTrip_Double_PreservesValueAndType()
    {
        const double input = 3.14159;
        var typed = Assert.IsType<double>(RoundTrip(input));
        Assert.Equal(input, typed);
    }

    [Fact]
    public void RoundTrip_Decimal_PreservesValueAndType()
    {
        const decimal input = 1234567890.123456789m;
        var typed = Assert.IsType<decimal>(RoundTrip(input));
        Assert.Equal(input, typed);
    }

    // --- Round-trip: BCL value types ---

    [Fact]
    public void RoundTrip_Guid_PreservesValueAndType()
    {
        var input = Guid.NewGuid();
        var typed = Assert.IsType<Guid>(RoundTrip(input));
        Assert.Equal(input, typed);
    }

    [Fact]
    public void RoundTrip_DateTimeOffset_PreservesValueAndType()
    {
        var input = new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.FromHours(2));
        var typed = Assert.IsType<DateTimeOffset>(RoundTrip(input));
        Assert.Equal(input, typed);
    }

    [Fact]
    public void RoundTrip_DateTime_PreservesValueAndType()
    {
        var input = new DateTime(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc);
        var typed = Assert.IsType<DateTime>(RoundTrip(input));
        Assert.Equal(input, typed);
    }

    [Fact]
    public void RoundTrip_DateOnly_PreservesValueAndType()
    {
        var input = new DateOnly(2024, 6, 15);
        var typed = Assert.IsType<DateOnly>(RoundTrip(input));
        Assert.Equal(input, typed);
    }

    [Fact]
    public void RoundTrip_TimeOnly_PreservesValueAndType()
    {
        var input = new TimeOnly(10, 30, 45);
        var typed = Assert.IsType<TimeOnly>(RoundTrip(input));
        Assert.Equal(input, typed);
    }

    // --- Round-trip: strings that look like other types ---

    [Theory]
    [InlineData("true")]
    [InlineData("2024-06-15")]
    [InlineData("550e8400-e29b-41d4-a716-446655440000")]
    public void RoundTrip_StringsThatResembleOtherTypes_RemainsString(string input)
    {
        // Type is encoded explicitly so there is no ambiguous token-kind inference.
        Assert.IsType<string>(RoundTrip(input));
        Assert.Equal(input, RoundTrip(input));
    }

    // --- Error cases ---

    [Fact]
    public void Read_WhenValueIsPlainNumber_ThrowsJsonException()
    {
        // A bare number has no type envelope — the converter must reject it.
        const string json = """{"field":"Age","operator":"eq","value":42}""";
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<QueryFilter>(json));
    }

    [Fact]
    public void Read_WhenTypeNameIsUnknown_ThrowsJsonException()
    {
        const string json = """{"field":"X","operator":"eq","value":["Some.Unknown.Type",42]}""";
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<QueryFilter>(json));
    }

    [Fact]
    public void Read_WhenFirstElementIsNotString_ThrowsJsonException()
    {
        const string json = """{"field":"X","operator":"eq","value":[42,"hello"]}""";
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<QueryFilter>(json));
    }
}
