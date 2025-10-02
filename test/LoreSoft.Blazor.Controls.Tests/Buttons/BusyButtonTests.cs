using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls.Tests.Buttons;

public class BusyButtonTests : TestContext
{
    [Fact]
    public void Renders_Button_With_Default_Class()
    {
        // arrange & act
        var cut = RenderComponent<BusyButton>();

        // assert
        cut.MarkupMatches("<button class=\"busy-button\"></button>");
    }

    [Fact]
    public void Renders_Button_With_Child_Content()
    {
        // arrange & act
        var cut = RenderComponent<BusyButton>(parameters => parameters
            .AddChildContent("Click Me")
        );

        // assert
        cut.MarkupMatches("<button class=\"busy-button\">Click Me</button>");
    }

    [Fact]
    public void Button_Is_Disabled_When_Disabled_Parameter_Is_True()
    {
        // arrange & act
        var cut = RenderComponent<BusyButton>(parameters => parameters
            .Add(p => p.Disabled, true)
            .AddChildContent("Disabled Button")
        );

        // assert
        var button = cut.Find("button");
        Assert.True(button.HasAttribute("disabled"));
    }

    [Fact]
    public void Button_Is_Disabled_When_Busy_Parameter_Is_True()
    {
        // arrange & act
        var cut = RenderComponent<BusyButton>(parameters => parameters
            .Add(p => p.Busy, true)
            .AddChildContent("Normal Content")
        );

        // assert
        var button = cut.Find("button");
        Assert.True(button.HasAttribute("disabled"));
    }

    [Fact]
    public void Renders_Default_Busy_Template_When_Busy()
    {
        // arrange & act
        var cut = RenderComponent<BusyButton>(parameters => parameters
            .Add(p => p.Busy, true)
            .AddChildContent("Normal Content")
        );

        // assert
        var button = cut.Find("button");
        Assert.Contains("Processing", button.TextContent);
        Assert.NotNull(cut.Find(".busy-loading-indicator"));
        Assert.NotNull(cut.Find(".busy-loading-dot-1"));
        Assert.NotNull(cut.Find(".busy-loading-dot-2"));
        Assert.NotNull(cut.Find(".busy-loading-dot-3"));
    }

    [Fact]
    public void Renders_Custom_Busy_Text()
    {
        // arrange & act
        var cut = RenderComponent<BusyButton>(parameters => parameters
            .Add(p => p.Busy, true)
            .Add(p => p.BusyText, "Loading...")
            .AddChildContent("Normal Content")
        );

        // assert
        var button = cut.Find("button");
        Assert.Contains("Loading...", button.TextContent);
    }

    [Fact]
    public void Renders_Custom_Busy_Template()
    {
        // arrange & act
        var cut = RenderComponent<BusyButton>(parameters => parameters
            .Add(p => p.Busy, true)
            .Add(p => p.BusyTemplate, builder => builder.AddMarkupContent(0, "<span class='custom-spinner'>Spinning...</span>"))
            .AddChildContent("Normal Content")
        );

        // assert
        var button = cut.Find("button");
        Assert.Contains("Spinning...", button.TextContent);
        Assert.NotNull(cut.Find(".custom-spinner"));
    }

    [Fact]
    public void Shows_Child_Content_When_Not_Busy()
    {
        // arrange & act
        var cut = RenderComponent<BusyButton>(parameters => parameters
            .Add(p => p.Busy, false)
            .AddChildContent("Click Me")
        );

        // assert
        var button = cut.Find("button");
        Assert.Equal("Click Me", button.TextContent);
        Assert.False(button.HasAttribute("disabled"));
    }

    [Fact]
    public void Button_Has_Type_Attribute_When_Trigger_Is_Set()
    {
        // arrange
        var triggerExecuted = false;
        var trigger = EventCallback.Factory.Create(this, () => triggerExecuted = true);

        // act
        var cut = RenderComponent<BusyButton>(parameters => parameters
            .Add(p => p.Trigger, trigger)
            .AddChildContent("Click Me")
        );

        // assert
        var button = cut.Find("button");
        Assert.Equal("button", button.GetAttribute("type"));
        Assert.False(triggerExecuted);
    }

    [Fact]
    public void Executes_Trigger_When_Clicked()
    {
        // arrange
        var triggerExecuted = false;
        var trigger = EventCallback.Factory.Create(this, () => triggerExecuted = true);

        var cut = RenderComponent<BusyButton>(parameters => parameters
            .Add(p => p.Trigger, trigger)
            .AddChildContent("Click Me")
        );

        // act
        var button = cut.Find("button");
        button.Click();

        // assert
        Assert.True(triggerExecuted);
    }

    [Fact]
    public async Task Button_Becomes_Busy_During_Async_Trigger_Execution()
    {
        // arrange
        var tcs = new TaskCompletionSource();
        var trigger = EventCallback.Factory.Create(this, () => tcs.Task);

        var cut = RenderComponent<BusyButton>(parameters => parameters
            .Add(p => p.Trigger, trigger)
            .AddChildContent("Click Me")
        );

        // act
        var button = cut.Find("button");
        button.Click();

        // assert - button should be busy immediately after click
        Assert.True(button.HasAttribute("disabled"));
        Assert.Contains("Processing", button.TextContent);

        // complete the async operation
        tcs.SetResult();
        await tcs.Task;

        // wait for the component to update
        cut.WaitForAssertion(() =>
        {
            var updatedButton = cut.Find("button");
            Assert.False(updatedButton.HasAttribute("disabled"));
            Assert.Equal("Click Me", updatedButton.TextContent);
        });
    }

    [Fact]
    public async Task Button_Remains_Disabled_After_Async_Trigger_If_Disabled_Parameter_Is_True()
    {
        // arrange
        var tcs = new TaskCompletionSource();
        var trigger = EventCallback.Factory.Create(this, () => tcs.Task);

        var cut = RenderComponent<BusyButton>(parameters => parameters
            .Add(p => p.Trigger, trigger)
            .Add(p => p.Disabled, true)
            .AddChildContent("Click Me")
        );

        // act
        var button = cut.Find("button");
        button.Click();

        // complete the async operation
        tcs.SetResult();
        await tcs.Task;

        // assert - button should still be disabled due to Disabled parameter
        cut.WaitForAssertion(() =>
        {
            var updatedButton = cut.Find("button");
            Assert.True(updatedButton.HasAttribute("disabled"));
        });
    }

    [Fact]
    public async Task Button_Shows_Busy_State_Even_When_Busy_Parameter_Is_False_During_Execution()
    {
        // arrange
        var tcs = new TaskCompletionSource();
        var trigger = EventCallback.Factory.Create(this, () => tcs.Task);

        var cut = RenderComponent<BusyButton>(parameters => parameters
            .Add(p => p.Trigger, trigger)
            .Add(p => p.Busy, false)
            .AddChildContent("Click Me")
        );

        // act
        var button = cut.Find("button");
        button.Click();

        // assert - button should be busy during execution even though Busy=false
        Assert.True(button.HasAttribute("disabled"));
        Assert.Contains("Processing", button.TextContent);

        // complete the async operation
        tcs.SetResult();
        await tcs.Task;

        // wait for the component to update
        cut.WaitForAssertion(() =>
        {
            var updatedButton = cut.Find("button");
            Assert.False(updatedButton.HasAttribute("disabled"));
            Assert.Equal("Click Me", updatedButton.TextContent);
        });
    }

    [Fact]
    public void Button_Shows_Busy_State_When_Both_Busy_Parameter_And_Executing_Are_True()
    {
        // arrange & act
        var cut = RenderComponent<BusyButton>(parameters => parameters
            .Add(p => p.Busy, true)
            .Add(p => p.BusyText, "External Busy")
            .AddChildContent("Click Me")
        );

        // assert
        var button = cut.Find("button");
        Assert.True(button.HasAttribute("disabled"));
        Assert.Contains("External Busy", button.TextContent);
    }

    [Fact]
    public void Button_Updates_When_Busy_Parameter_Changes()
    {
        // arrange
        var cut = RenderComponent<BusyButton>(parameters => parameters
            .Add(p => p.Busy, false)
            .AddChildContent("Click Me")
        );

        // assert initial state
        var button = cut.Find("button");
        Assert.False(button.HasAttribute("disabled"));
        Assert.Equal("Click Me", button.TextContent);

        // act - change Busy parameter
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Busy, true)
        );

        // assert updated state
        button = cut.Find("button");
        Assert.True(button.HasAttribute("disabled"));
        Assert.Contains("Processing", button.TextContent);
    }

    [Fact]
    public void Button_Updates_When_BusyText_Parameter_Changes()
    {
        // arrange
        var cut = RenderComponent<BusyButton>(parameters => parameters
            .Add(p => p.Busy, true)
            .Add(p => p.BusyText, "Loading...")
            .AddChildContent("Click Me")
        );

        // assert initial state
        var button = cut.Find("button");
        Assert.Contains("Loading...", button.TextContent);

        // act - change BusyText parameter
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.BusyText, "Please wait...")
        );

        // assert updated state
        button = cut.Find("button");
        Assert.Contains("Please wait...", button.TextContent);
    }

    [Fact]
    public async Task Multiple_Rapid_Clicks_During_Execution_Are_Ignored()
    {
        // arrange
        var executionCount = 0;
        var tcs = new TaskCompletionSource();
        var trigger = EventCallback.Factory.Create(this, async () =>
        {
            executionCount++;
            await tcs.Task;
        });

        var cut = RenderComponent<BusyButton>(parameters => parameters
            .Add(p => p.Trigger, trigger)
            .AddChildContent("Click Me")
        );

        // act - click multiple times rapidly
        var button = cut.Find("button");
        button.Click();
        button.Click();
        button.Click();

        // complete the async operation
        tcs.SetResult();
        await tcs.Task;

        // assert - trigger should have been executed only once
        Assert.Equal(1, executionCount);
    }

    [Fact]
    public void Button_Without_Trigger_Does_Not_Have_Type_Attribute()
    {
        // arrange & act
        var cut = RenderComponent<BusyButton>(parameters => parameters
            .AddChildContent("Static Button")
        );

        // assert
        var button = cut.Find("button");
        Assert.False(button.HasAttribute("type"));
        Assert.False(button.HasAttribute("onclick"));
    }

    [Fact]
    public void Complex_Markup_In_Child_Content_Is_Preserved()
    {
        // arrange & act
        var cut = RenderComponent<BusyButton>(parameters => parameters
            .AddChildContent("<span class='icon'>📤</span> <strong>Submit</strong>")
        );

        // assert
        var button = cut.Find("button");
        Assert.NotNull(cut.Find(".icon"));
        Assert.NotNull(cut.Find("strong"));
        Assert.Contains("📤", button.TextContent);
        Assert.Contains("Submit", button.TextContent);
    }

    [Fact]
    public void Complex_Markup_In_Busy_Template_Is_Rendered()
    {
        // arrange & act
        var cut = RenderComponent<BusyButton>(parameters => parameters
            .Add(p => p.Busy, true)
            .Add(p => p.BusyTemplate, builder =>
            {
                builder.AddMarkupContent(0, "<div class='spinner'></div>");
                builder.AddMarkupContent(1, "<span class='status'>Working...</span>");
            })
            .AddChildContent("Normal Content")
        );

        // assert
        Assert.NotNull(cut.Find(".spinner"));
        Assert.NotNull(cut.Find(".status"));
        var statusSpan = cut.Find(".status");
        Assert.Equal("Working...", statusSpan.TextContent);
    }
}
