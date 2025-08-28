using LoreSoft.Blazor.Controls.Extensions;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// A Blazor component that renders date and time values in the user's local time zone.
/// Supports both <see cref="DateTime"/> and <see cref="DateTimeOffset"/> values and renders them as HTML time elements.
/// </summary>
/// <typeparam name="TValue">The type of the date/time value. Must be <see cref="DateTime"/>, <see cref="DateTimeOffset"/>, or a nullable variant of these types.</typeparam>
/// <remarks>
/// <para>
/// This component automatically detects the user's browser time zone and converts the provided date/time values accordingly.
/// The component renders an HTML &lt;time&gt; element with appropriate datetime attributes for accessibility and semantic markup.
/// </para>
/// <para>
/// The component will render nothing if the <see cref="Value"/> parameter is null.
/// For unsupported types, the component falls back to calling ToString() on the value.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;LocalTime Value="@DateTime.Now" /&gt;
/// &lt;LocalTime Value="@DateTimeOffset.UtcNow" DisplayFormat="F" TitleFormat="R" /&gt;
/// </code>
/// </example>
public class LocalTime<TValue> : ComponentBase
{
    private const string HtmlFormat = "o";

    /// <summary>
    /// Gets or sets the time zone provider service used to determine the browser's local time zone.
    /// </summary>
    /// <value>
    /// The browser culture provider that retrieves time zone information from the client browser.
    /// This service is automatically injected by the dependency injection container.
    /// </value>
    [Inject]
    public required BrowserCultureProvider BrowserProvider { get; set; }

    /// <summary>
    /// Gets or sets the date/time value to display.
    /// </summary>
    /// <value>
    /// The date/time value to render. Supports <see cref="DateTime"/>, <see cref="DateTimeOffset"/>,
    /// and their nullable variants. If null, nothing will be rendered.
    /// </value>
    [Parameter]
    public TValue? Value { get; set; }

    /// <summary>
    /// Gets or sets the format string used to display the date/time value.
    /// </summary>
    /// <value>
    /// A standard or custom date/time format string. Defaults to "g" (general date/time pattern short).
    /// </value>
    /// <remarks>
    /// This format is applied to the localized time value and controls how the date/time appears to users.
    /// </remarks>
    [Parameter]
    public string? DisplayFormat { get; set; } = "g";

    /// <summary>
    /// Gets or sets the format string used for the title attribute of the time element.
    /// </summary>
    /// <value>
    /// A standard or custom date/time format string used for the HTML title attribute.
    /// Defaults to "f" (full date/time pattern short). Set to null or empty string to disable the title attribute.
    /// </value>
    /// <remarks>
    /// The title attribute provides a tooltip when users hover over the time element,
    /// typically showing a more detailed representation of the date/time.
    /// </remarks>
    [Parameter]
    public string? TitleFormat { get; set; } = "f";

    /// <summary>
    /// Gets the local time zone information for the user's browser.
    /// </summary>
    /// <value>
    /// The <see cref="TimeZoneInfo"/> representing the user's local time zone.
    /// Initially set to <see cref="TimeZoneInfo.Local"/> and updated during component initialization
    /// with the actual browser time zone.
    /// </value>
    protected TimeZoneInfo LocalTimeZone { get; set; } = TimeZoneInfo.Local;

    /// <summary>
    /// Called after the component has finished rendering and is executed only during the first render.
    /// Retrieves the browser's local time zone and triggers a re-render to display the correct localized time.
    /// </summary>
    /// <param name="firstRender">True if this is the first time the component has been rendered; otherwise, false.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This method uses the <see cref="BrowserProvider"/> to determine the user's actual time zone
    /// from the browser, replacing the initial default of <see cref="TimeZoneInfo.Local"/>.
    /// A state change is triggered to re-render the component with the correct time zone information.
    /// </remarks>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        // get the local time zone from the browser
        LocalTimeZone = await BrowserProvider.GetTimeZone();
        StateHasChanged();
    }

    /// <summary>
    /// Builds the render tree for the component. Renders an HTML time element with the localized date/time
    /// or nothing if the value is null.
    /// </summary>
    /// <param name="builder">The render tree builder used to construct the component's output.</param>
    /// <remarks>
    /// <para>
    /// The method handles <see cref="DateTime"/> and <see cref="DateTimeOffset"/> values specifically,
    /// converting them to the user's local time zone and rendering them as semantic HTML time elements.
    /// </para>
    /// <para>
    /// For unsupported types, the method falls back to calling ToString() on the value.
    /// If the value is null, no content is rendered.
    /// </para>
    /// </remarks>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // if no value, do not render anything
        if (Value is null)
            return;

        // only render if date time or date time offset
        if (Value is DateTime dateTime)
            RenderDateTime(builder, dateTime);
        else if (Value is DateTimeOffset dateTimeOffset)
            RenderDateTimeOffset(builder, dateTimeOffset);
        else
            builder.AddContent(0, Value?.ToString());
    }

    private void RenderDateTimeOffset(RenderTreeBuilder builder, DateTimeOffset dateTimeOffset)
    {
        var localDate = dateTimeOffset.ToTimeZone(LocalTimeZone);

        builder.OpenElement(0, "time");
        builder.AddAttribute(1, "datetime", localDate.ToString(HtmlFormat));
        builder.AddAttribute(2, "data-timezone", LocalTimeZone.Id);

        if (!string.IsNullOrEmpty(TitleFormat))
            builder.AddAttribute(3, "title", localDate.ToString(TitleFormat));

        builder.AddContent(4, localDate.ToString(DisplayFormat));
        builder.CloseElement();
    }

    private void RenderDateTime(RenderTreeBuilder builder, DateTime dateTime)
    {
        var localDate = dateTime.ToTimeZone(LocalTimeZone);

        builder.OpenElement(0, "time");
        builder.AddAttribute(1, "datetime", localDate.ToString(HtmlFormat));
        builder.AddAttribute(2, "data-timezone", LocalTimeZone.Id);

        if (!string.IsNullOrEmpty(TitleFormat))
            builder.AddAttribute(3, "title", localDate.ToString(TitleFormat));

        builder.AddContent(4, localDate.ToString(DisplayFormat));
        builder.CloseElement();
    }
}

