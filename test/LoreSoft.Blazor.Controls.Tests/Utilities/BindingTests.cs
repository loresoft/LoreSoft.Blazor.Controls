using LoreSoft.Blazor.Controls.Utilities;

namespace LoreSoft.Blazor.Controls.Tests.Utilities;

public class BindingTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("abc", "abc")]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    [InlineData(42, "42")]
    [InlineData(123L, "123")]
    [InlineData((short)7, "7")]
    [InlineData(3.14f, "3.14")]
    [InlineData(2.718, "2.718")]
    [InlineData(1.23, "1.23")]
    [InlineData(1.24d, "1.24")]
    [InlineData(1.25f, "1.25")]
    public void Format_Primitives_ReturnsExpectedString(object? value, string? expected)
    {
        var result = Binding.Format(value);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Format_NullableTypes_ReturnsExpectedString()
    {
        int? intValue = 5;
        int? nullInt = null;
        bool? boolValue = true;
        bool? nullBool = null;
        DateTime? dateValue = new DateTime(2024, 6, 30);
        DateTime? nullDate = null;

        Assert.Equal("5", Binding.Format(intValue));
        Assert.Null(Binding.Format(nullInt));
        Assert.Equal("true", Binding.Format(boolValue));
        Assert.Null(Binding.Format(nullBool));
        Assert.Equal("2024-06-30", Binding.Format(dateValue));
        Assert.Null(Binding.Format(nullDate));
    }

    [Fact]
    public void Format_DateTime_ReturnsIsoDate()
    {
        var date = new DateTime(2024, 6, 30, 15, 45, 0);
        var result = Binding.Format(date);
        Assert.Equal("2024-06-30", result);
    }

    [Fact]
    public void Format_DateTimeOffset_ReturnsIsoDate()
    {
        var date = new DateTimeOffset(2024, 6, 30, 15, 45, 0, TimeSpan.Zero);
        var result = Binding.Format(date);
        Assert.Equal("2024-06-30", result);
    }

    [Fact]
    public void Format_DateOnly_ReturnsIsoDate()
    {
        var date = new DateOnly(2024, 6, 30);
        var result = Binding.Format(date);
        Assert.Equal("2024-06-30", result);
    }

    [Fact]
    public void Format_TimeOnly_ReturnsIsoTime()
    {
        var time = new TimeOnly(15, 45, 30);
        var result = Binding.Format(time);
        Assert.Equal("15:45:30", result);
    }

    [Fact]
    public void Format_OtherType_CallsToString()
    {
        var obj = new { Name = "Test" };
        var result = Binding.Format(obj);
        Assert.Equal(obj.ToString(), result);
    }

    [Theory]
    [InlineData("123", typeof(int), 123)]
    [InlineData("true", typeof(bool), true)]
    [InlineData(null, typeof(string), null)]
    public void Convert_StringToType_ReturnsExpected(string? input, Type type, object? expected)
    {
        var result = Binding.Convert(input, type);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_Generic_ReturnsExpected()
    {
        var result = Binding.Convert<int>("42");
        Assert.Equal(42, result);
    }

    [Fact]
    public void Convert_NullableTypes_ReturnsExpected()
    {
        Assert.Equal(42, Binding.Convert<int?>("42"));
        Assert.Null(Binding.Convert<int?>(null));
        Assert.True((bool?)Binding.Convert<bool?>("true"));
        Assert.Null(Binding.Convert<bool?>(null));
        Assert.Equal(new DateTime(2024, 6, 30), Binding.Convert<DateTime?>("2024-06-30"));
        Assert.Null(Binding.Convert<DateTime?>(null));
    }
}
