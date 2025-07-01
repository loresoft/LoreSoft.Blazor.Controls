using LoreSoft.Blazor.Controls.Utilities;

namespace LoreSoft.Blazor.Controls.Tests.Utilities;

public class AttributeBuilderTests
{
    [Fact]
    public void Default_CreatesBuilderWithSingleAttribute()
    {
        var builder = AttributeBuilder.Default("class", "btn");
        var attributes = builder.Build();

        Assert.Single(attributes);
        Assert.Equal("btn", attributes["class"]);
    }

    [Fact]
    public void Empty_CreatesBuilderWithNoAttributes()
    {
        var builder = AttributeBuilder.Empty();
        var attributes = builder.Build();

        Assert.Empty(attributes);
    }

    [Fact]
    public void AddAttribute_AddsOrUpdatesAttribute()
    {
        var builder = AttributeBuilder.Empty()
            .AddAttribute("id", "test")
            .AddAttribute("id", "updated");

        var attributes = builder.Build();

        Assert.Single(attributes);
        Assert.Equal("updated", attributes["id"]);
    }

    [Fact]
    public void AddAttribute_WithConditionTrue_AddsAttribute()
    {
        var builder = AttributeBuilder.Empty()
            .AddAttribute("hidden", true, true);

        var attributes = builder.Build();

        Assert.True(attributes.ContainsKey("hidden"));
        Assert.Equal(true, attributes["hidden"]);
    }

    [Fact]
    public void AddAttribute_WithConditionFalse_DoesNotAddAttribute()
    {
        var builder = AttributeBuilder.Empty()
            .AddAttribute("hidden", true, false);

        var attributes = builder.Build();

        Assert.False(attributes.ContainsKey("hidden"));
    }

    [Fact]
    public void AddAttribute_WithFuncConditionTrue_AddsAttribute()
    {
        var builder = AttributeBuilder.Empty()
            .AddAttribute("disabled", false, () => true);

        var attributes = builder.Build();

        Assert.True(attributes.ContainsKey("disabled"));
        Assert.Equal(false, attributes["disabled"]);
    }

    [Fact]
    public void AddAttribute_WithFuncConditionFalse_DoesNotAddAttribute()
    {
        var builder = AttributeBuilder.Empty()
            .AddAttribute("disabled", false, () => false);

        var attributes = builder.Build();

        Assert.False(attributes.ContainsKey("disabled"));
    }

    [Fact]
    public void AddAttribute_WithFuncValueAndConditionTrue_AddsAttribute()
    {
        var builder = AttributeBuilder.Empty()
            .AddAttribute("data", () => 123, true);

        var attributes = builder.Build();

        Assert.True(attributes.ContainsKey("data"));
        Assert.Equal(123, attributes["data"]);
    }

    [Fact]
    public void AddAttribute_WithFuncValueAndFuncConditionTrue_AddsAttribute()
    {
        var builder = AttributeBuilder.Empty()
            .AddAttribute("data", () => "abc", () => true);

        var attributes = builder.Build();

        Assert.True(attributes.ContainsKey("data"));
        Assert.Equal("abc", attributes["data"]);
    }

    [Fact]
    public void AddAttribute_WithFuncValueAndFuncConditionFalse_DoesNotAddAttribute()
    {
        var builder = AttributeBuilder.Empty()
            .AddAttribute("data", () => "abc", () => false);

        var attributes = builder.Build();

        Assert.False(attributes.ContainsKey("data"));
    }

    [Fact]
    public void Build_ReturnsAttributesDictionary()
    {
        var dict = new Dictionary<string, object> { { "role", "button" } };
        var builder = new AttributeBuilder(dict);

        var attributes = builder.Build();

        Assert.Same(dict, attributes);
    }
}
