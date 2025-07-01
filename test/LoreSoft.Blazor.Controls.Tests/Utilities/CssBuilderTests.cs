using System;
using System.Collections.Generic;
using LoreSoft.Blazor.Controls.Utilities;
using Xunit;

namespace LoreSoft.Blazor.Controls.Tests.Utilities;

public class CssBuilderTests
{
    [Fact]
    public void Default_CreatesBuilderWithInitialValue()
    {
        var builder = CssBuilder.Default("foo");
        Assert.Equal("foo", builder.ToString());
    }

    [Fact]
    public void Empty_CreatesBuilderWithNoValue()
    {
        var builder = CssBuilder.Empty();
        Assert.Equal(string.Empty, builder.ToString());
    }

    [Fact]
    public void AddClass_AppendsClass()
    {
        var builder = CssBuilder.Empty()
            .AddClass("foo")
            .AddClass("bar");

        Assert.Equal("foo bar", builder.ToString());
    }

    [Fact]
    public void AddClass_DoesNotAppendNullOrEmpty()
    {
        var builder = CssBuilder.Empty()
            .AddClass(null)
            .AddClass(string.Empty)
            .AddClass("foo");

        Assert.Equal("foo", builder.ToString());
    }

    [Fact]
    public void AddClass_WithConditionTrue_AppendsClass()
    {
        var builder = CssBuilder.Empty()
            .AddClass("foo", true);

        Assert.Equal("foo", builder.ToString());
    }

    [Fact]
    public void AddClass_WithConditionFalse_DoesNotAppendClass()
    {
        var builder = CssBuilder.Empty()
            .AddClass("foo", false);

        Assert.Equal(string.Empty, builder.ToString());
    }

    [Fact]
    public void AddClass_WithFuncConditionTrue_AppendsClass()
    {
        var builder = CssBuilder.Empty()
            .AddClass("foo", () => true);

        Assert.Equal("foo", builder.ToString());
    }

    [Fact]
    public void AddClass_WithFuncConditionFalse_DoesNotAppendClass()
    {
        var builder = CssBuilder.Empty()
            .AddClass("foo", () => false);

        Assert.Equal(string.Empty, builder.ToString());
    }

    [Fact]
    public void AddClass_WithFuncValueAndConditionTrue_AppendsClass()
    {
        var builder = CssBuilder.Empty()
            .AddClass(() => "foo", true);

        Assert.Equal("foo", builder.ToString());
    }

    [Fact]
    public void AddClass_WithFuncValueAndFuncConditionTrue_AppendsClass()
    {
        var builder = CssBuilder.Empty()
            .AddClass(() => "foo", () => true);

        Assert.Equal("foo", builder.ToString());
    }

    [Fact]
    public void AddClass_WithFuncPredicate_AppendsClassWhenPredicateTrue()
    {
        var builder = CssBuilder.Empty()
            .AddClass("foo", v => v == "foo");

        Assert.Equal("foo", builder.ToString());
    }

    [Fact]
    public void AddClass_WithFuncPredicate_DoesNotAppendWhenPredicateFalse()
    {
        var builder = CssBuilder.Empty()
            .AddClass("foo", v => v == "bar");

        Assert.Equal(string.Empty, builder.ToString());
    }

    [Fact]
    public void AddClass_WithCssBuilderAndConditionTrue_AppendsClass()
    {
        var other = CssBuilder.Default("foo");
        var builder = CssBuilder.Empty()
            .AddClass(other, true);

        Assert.Equal("foo", builder.ToString());
    }

    [Fact]
    public void AddClass_WithCssBuilderAndConditionFalse_DoesNotAppendClass()
    {
        var other = CssBuilder.Default("foo");
        var builder = CssBuilder.Empty()
            .AddClass(other, false);

        Assert.Equal(string.Empty, builder.ToString());
    }

    [Fact]
    public void MergeClass_AppendsClassFromAttributesAndRemovesIt()
    {
        var dict = new Dictionary<string, object>
        {
            { "class", "foo bar" },
            { "id", "test" }
        };

        var builder = CssBuilder.Empty()
            .MergeClass(dict);

        Assert.Equal("foo bar", builder.ToString());
        Assert.False(dict.ContainsKey("class"));
        Assert.True(dict.ContainsKey("id"));
    }

    [Fact]
    public void MergeClass_DoesNothingIfNoClassKey()
    {
        var dict = new Dictionary<string, object>
        {
            { "id", "test" }
        };

        var builder = CssBuilder.Empty()
            .MergeClass(dict);

        Assert.Equal(string.Empty, builder.ToString());
        Assert.True(dict.ContainsKey("id"));
    }

    [Fact]
    public void MergeClass_DoesNothingIfAttributesNull()
    {
        var builder = CssBuilder.Empty()
            .MergeClass(null);

        Assert.Equal(string.Empty, builder.ToString());
    }
}
