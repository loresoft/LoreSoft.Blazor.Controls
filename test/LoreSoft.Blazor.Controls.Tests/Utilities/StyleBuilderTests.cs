using System;
using System.Collections.Generic;
using LoreSoft.Blazor.Controls.Utilities;
using Xunit;

namespace LoreSoft.Blazor.Controls.Tests.Utilities;

public class StyleBuilderTests
{
    [Fact]
    public void Default_WithPropAndValue_CreatesStyle()
    {
        var builder = StyleBuilder.Default("color", "red");
        Assert.Equal("color:red;", builder.ToString());
    }

    [Fact]
    public void Default_WithStyleString_CreatesStyle()
    {
        var builder = StyleBuilder.Default("margin:10px");
        Assert.Equal("margin:10px;", builder.ToString());
    }

    [Fact]
    public void Empty_CreatesEmptyStyle()
    {
        var builder = StyleBuilder.Empty();
        Assert.Equal(string.Empty, builder.ToString());
    }

    [Fact]
    public void AddStyle_AppendsStyleString()
    {
        var builder = StyleBuilder.Empty()
            .AddStyle("padding:5px")
            .AddStyle("margin:10px");

        Assert.Equal("padding:5px;margin:10px;", builder.ToString());
    }

    [Fact]
    public void AddStyle_DoesNotAppendNullOrWhitespace()
    {
        var builder = StyleBuilder.Empty()
            .AddStyle(null)
            .AddStyle("")
            .AddStyle("   ")
            .AddStyle("color:blue");

        Assert.Equal("color:blue;", builder.ToString());
    }

    [Fact]
    public void AddStyle_WithPropAndValue_AppendsStyle()
    {
        var builder = StyleBuilder.Empty()
            .AddStyle("color", "green")
            .AddStyle("background", "yellow");

        Assert.Equal("color:green;background:yellow;", builder.ToString());
    }

    [Fact]
    public void AddStyle_WithConditionTrue_AppendsStyle()
    {
        var builder = StyleBuilder.Empty()
            .AddStyle("color", "red", true);

        Assert.Equal("color:red;", builder.ToString());
    }

    [Fact]
    public void AddStyle_WithConditionFalse_DoesNotAppendStyle()
    {
        var builder = StyleBuilder.Empty()
            .AddStyle("color", "red", false);

        Assert.Equal(string.Empty, builder.ToString());
    }

    [Fact]
    public void AddStyle_WithFuncValueAndConditionTrue_AppendsStyle()
    {
        var builder = StyleBuilder.Empty()
            .AddStyle("width", () => "100px", true);

        Assert.Equal("width:100px;", builder.ToString());
    }

    [Fact]
    public void AddStyle_WithFuncValueAndFuncConditionTrue_AppendsStyle()
    {
        var builder = StyleBuilder.Empty()
            .AddStyle("height", () => "50px", () => true);

        Assert.Equal("height:50px;", builder.ToString());
    }

    [Fact]
    public void AddStyle_WithFuncPredicate_AppendsStyleWhenPredicateTrue()
    {
        var builder = StyleBuilder.Empty()
            .AddStyle("border", "1px solid", v => v == "1px solid");

        Assert.Equal("border:1px solid;", builder.ToString());
    }

    [Fact]
    public void AddStyle_WithFuncPredicate_DoesNotAppendWhenPredicateFalse()
    {
        var builder = StyleBuilder.Empty()
            .AddStyle("border", "1px solid", v => v == "none");

        Assert.Equal(string.Empty, builder.ToString());
    }

    [Fact]
    public void AddStyle_WithStyleBuilder_AppendsStyle()
    {
        var other = StyleBuilder.Default("color", "red");
        var builder = StyleBuilder.Empty()
            .AddStyle(other);

        Assert.Equal("color:red;", builder.ToString());
    }

    [Fact]
    public void AddStyle_WithStyleBuilderAndConditionTrue_AppendsStyle()
    {
        var other = StyleBuilder.Default("color", "red");
        var builder = StyleBuilder.Empty()
            .AddStyle(other, true);

        Assert.Equal("color:red;", builder.ToString());
    }

    [Fact]
    public void AddStyle_WithStyleBuilderAndConditionFalse_DoesNotAppendStyle()
    {
        var other = StyleBuilder.Default("color", "red");
        var builder = StyleBuilder.Empty()
            .AddStyle(other, false);

        Assert.Equal(string.Empty, builder.ToString());
    }

    [Fact]
    public void MergeStyle_AppendsStyleFromAttributesAndRemovesIt()
    {
        var dict = new Dictionary<string, object>
        {
            { "style", "color:blue;" },
            { "id", "test" }
        };

        var builder = StyleBuilder.Empty()
            .MergeStyle(dict);

        Assert.Equal("color:blue;", builder.ToString());
        Assert.False(dict.ContainsKey("style"));
        Assert.True(dict.ContainsKey("id"));
    }

    [Fact]
    public void MergeStyle_DoesNothingIfNoStyleKey()
    {
        var dict = new Dictionary<string, object>
        {
            { "id", "test" }
        };

        var builder = StyleBuilder.Empty()
            .MergeStyle(dict);

        Assert.Equal(string.Empty, builder.ToString());
        Assert.True(dict.ContainsKey("id"));
    }

    [Fact]
    public void MergeStyle_DoesNothingIfAttributesNull()
    {
        var builder = StyleBuilder.Empty()
            .MergeStyle(null);

        Assert.Equal(string.Empty, builder.ToString());
    }
}
