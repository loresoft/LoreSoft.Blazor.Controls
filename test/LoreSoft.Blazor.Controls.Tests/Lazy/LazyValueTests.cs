using LoreSoft.Blazor.Controls.Tests.Data;

namespace LoreSoft.Blazor.Controls.Tests.Lazy;

public class LazyValueTests : TestContext
{
    [Fact]
    public void Renders_Value_Using_LoadMethod_And_Key()
    {
        // arrange
        var fruit = Fruit.Data().First();
        var key = fruit.Id;
        var loadMethod = Fruit.GetByIdAsync;

        // act
        var cut = RenderComponent<LazyValue<Guid, Fruit>>(parameters => parameters
            .Add(p => p.LoadMethod, loadMethod)
            .Add(p => p.Key, key)
        );

        cut.WaitForAssertion(() =>
        {
            cut.MarkupMatches(fruit.ToString());
        });
    }

    [Fact]
    public void Renders_Value_Using_Custom_ChildContent_Template()
    {
        // arrange
        var fruit = Fruit.Data().First();
        var key = fruit.Id;
        var loadMethod = Fruit.GetByIdAsync;

        // act
        var cut = RenderComponent<LazyValue<Guid, Fruit>>(parameters => parameters
            .Add(p => p.LoadMethod, loadMethod)
            .Add(p => p.Key, key)
            .Add(p => p.ChildContent, value => $"<div class='fruit'>{value?.Name}</div>")
        );

        cut.WaitForAssertion(() =>
        {
            cut.MarkupMatches($"<div class='fruit'>{fruit.Name}</div>");
        });
    }

    [Fact]
    public void Renders_Null_Value_When_Key_Not_Found()
    {
        // arrange
        var nonExistentKey = Guid.NewGuid();
        var loadMethod = Fruit.GetByIdAsync;

        // act
        var cut = RenderComponent<LazyValue<Guid, Fruit>>(parameters => parameters
            .Add(p => p.LoadMethod, loadMethod)
            .Add(p => p.Key, nonExistentKey)
        );

        cut.WaitForAssertion(() =>
        {
            cut.MarkupMatches(nonExistentKey.ToString());
        });
    }

    [Fact]
    public void Renders_Null_Value_With_Custom_Template()
    {
        // arrange
        var nonExistentKey = Guid.NewGuid();
        var loadMethod = Fruit.GetByIdAsync;

        // act
        var cut = RenderComponent<LazyValue<Guid, Fruit>>(parameters => parameters
            .Add(p => p.LoadMethod, loadMethod)
            .Add(p => p.Key, nonExistentKey)
            .Add(p => p.ChildContent, value => value == null ? "<span>Not found</span>" : $"<div>{value.Name}</div>")
        );

        cut.WaitForAssertion(() =>
        {
            cut.MarkupMatches("<span>Not found</span>");
        });
    }

    [Fact]
    public void Reloads_Value_When_Key_Changes()
    {
        // arrange
        var fruits = Fruit.Data().Take(2).ToArray();
        var firstKey = fruits[0].Id;
        var secondKey = fruits[1].Id;
        var loadMethod = Fruit.GetByIdAsync;

        // act
        var cut = RenderComponent<LazyValue<Guid, Fruit>>(parameters => parameters
            .Add(p => p.LoadMethod, loadMethod)
            .Add(p => p.Key, firstKey)
        );

        // assert first value
        cut.WaitForAssertion(() =>
        {
            cut.MarkupMatches(fruits[0].ToString());
        });

        // change key parameter
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Key, secondKey)
        );

        // assert second value
        cut.WaitForAssertion(() =>
        {
            cut.MarkupMatches(fruits[1].ToString());
        });
    }

    [Fact]
    public void Renders_Complex_Object_With_Custom_Template()
    {
        // arrange
        var fruit = Fruit.Data().First();
        var key = fruit.Id;
        var loadMethod = Fruit.GetByIdAsync;

        // act
        var cut = RenderComponent<LazyValue<Guid, Fruit>>(parameters => parameters
            .Add(p => p.LoadMethod, loadMethod)
            .Add(p => p.Key, key)
            .Add(p => p.ChildContent, value => $@"
                <div class='fruit-card'>
                    <h3>{value?.Name}</h3>
                    <p>Rank: {value?.Rank}</p>
                    <p>{value?.Description}</p>
                </div>".Trim())
        );

        cut.WaitForAssertion(() =>
        {
            var expectedMarkup = $@"
                <div class='fruit-card'>
                    <h3>{fruit.Name}</h3>
                    <p>Rank: {fruit.Rank}</p>
                    <p>{fruit.Description}</p>
                </div>".Trim();
            cut.MarkupMatches(expectedMarkup);
        });
    }

    [Fact]
    public void Value_Property_Is_Set_After_Loading()
    {
        // arrange
        var fruit = Fruit.Data().First();
        var key = fruit.Id;
        var loadMethod = Fruit.GetByIdAsync;

        // act
        var cut = RenderComponent<LazyValue<Guid, Fruit>>(parameters => parameters
            .Add(p => p.LoadMethod, loadMethod)
            .Add(p => p.Key, key)
        );

        // assert
        cut.WaitForAssertion(() =>
        {
            Assert.NotNull(cut.Instance.Value);
            Assert.Equal(fruit.Id, cut.Instance.Value.Id);
            Assert.Equal(fruit.Name, cut.Instance.Value.Name);
            Assert.Equal(fruit.Rank, cut.Instance.Value.Rank);
        });
    }

    [Fact]
    public void Value_Property_Is_Null_When_Not_Found()
    {
        // arrange
        var nonExistentKey = Guid.NewGuid();
        var loadMethod = Fruit.GetByIdAsync;

        // act
        var cut = RenderComponent<LazyValue<Guid, Fruit>>(parameters => parameters
            .Add(p => p.LoadMethod, loadMethod)
            .Add(p => p.Key, nonExistentKey)
        );

        // assert
        cut.WaitForAssertion(() =>
        {
            Assert.Null(cut.Instance.Value);
        });
    }

    [Fact]
    public void Loads_Value_With_String_Key()
    {
        // arrange
        static Task<string?> LoadStringAsync(string? key) => Task.FromResult(key?.ToUpper());
        var key = "hello";

        // act
        var cut = RenderComponent<LazyValue<string, string>>(parameters => parameters
            .Add(p => p.LoadMethod, LoadStringAsync)
            .Add(p => p.Key, key)
        );

        cut.WaitForAssertion(() =>
        {
            cut.MarkupMatches("HELLO");
        });
    }

    [Fact]
    public void Renders_Empty_String_Value()
    {
        // arrange
        static Task<string?> LoadEmptyStringAsync(string? key) => Task.FromResult<string?>("");
        var key = "test";

        // act
        var cut = RenderComponent<LazyValue<string, string>>(parameters => parameters
            .Add(p => p.LoadMethod, LoadEmptyStringAsync)
            .Add(p => p.Key, key)
        );

        cut.WaitForAssertion(() =>
        {
            cut.MarkupMatches("");
        });
    }

    [Fact]
    public void Handles_Multiple_Rapid_Key_Changes()
    {
        // arrange
        var fruits = Fruit.Data().Take(3).ToArray();
        var loadMethod = Fruit.GetByIdAsync;

        // act
        var cut = RenderComponent<LazyValue<Guid, Fruit>>(parameters => parameters
            .Add(p => p.LoadMethod, loadMethod)
            .Add(p => p.Key, fruits[0].Id)
        );

        // assert first value
        cut.WaitForAssertion(() =>
        {
            cut.MarkupMatches(fruits[0].ToString());
        });

        // rapidly change keys
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Key, fruits[1].Id)
        );

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Key, fruits[2].Id)
        );

        // assert final value
        cut.WaitForAssertion(() =>
        {
            cut.MarkupMatches(fruits[2].ToString());
        });
    }

    [Fact]
    public void Handles_Same_Key_Set_Multiple_Times()
    {
        // arrange
        var fruit = Fruit.Data().First();
        var key = fruit.Id;
        var loadMethod = Fruit.GetByIdAsync;

        // act
        var cut = RenderComponent<LazyValue<Guid, Fruit>>(parameters => parameters
            .Add(p => p.LoadMethod, loadMethod)
            .Add(p => p.Key, key)
        );

        // assert first value
        cut.WaitForAssertion(() =>
        {
            cut.MarkupMatches(fruit.ToString());
        });

        // set same key again
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Key, key)
        );

        // assert value is still the same
        cut.WaitForAssertion(() =>
        {
            cut.MarkupMatches(fruit.ToString());
        });
    }

    [Fact]
    public void Handles_Template_With_Null_Check()
    {
        // arrange
        var fruit = Fruit.Data().First();
        var key = fruit.Id;
        var loadMethod = Fruit.GetByIdAsync;

        // act
        var cut = RenderComponent<LazyValue<Guid, Fruit>>(parameters => parameters
            .Add(p => p.LoadMethod, loadMethod)
            .Add(p => p.Key, key)
            .Add(p => p.ChildContent, value =>
                value != null
                    ? $"<span class='loaded'>{value.Name} - {value.Rank}</span>"
                    : "<span class='loading'>Loading...</span>")
        );

        cut.WaitForAssertion(() =>
        {
            cut.MarkupMatches($"<span class='loaded'>{fruit.Name} - {fruit.Rank}</span>");
        });
    }
}
