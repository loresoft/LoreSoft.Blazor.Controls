using System.Globalization;

using Microsoft.JSInterop;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Provides functionality to retrieve and cache the browser's culture and time zone information.
/// This service uses JavaScript interop to detect the user's language preference and local time zone from the browser.
/// </summary>
public class BrowserCultureProvider : IAsyncDisposable
{
    private readonly IJSRuntime _javaScript;
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

    public BrowserCultureProvider(IJSRuntime javaScript)
    {
        _javaScript = javaScript;
        _moduleTask = new(() => _javaScript.InvokeAsync<IJSObjectReference>(
           "import", "./_content/LoreSoft.Blazor.Controls/js/browser.js").AsTask());
    }


    private TimeZoneInfo? _cachedTimeZone;
    private string? _cachedLanguage;

    /// <summary>
    /// Asynchronously retrieves the browser's time zone information.
    /// The result is cached after the first successful retrieval unless forced to refresh.
    /// </summary>
    /// <param name="force">
    /// If <c>true</c>, forces a fresh retrieval from the browser, bypassing the cache.
    /// If <c>false</c> (default), returns the cached value if available.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask{TimeZoneInfo}"/> representing the asynchronous operation.
    /// Returns the browser's time zone if successfully detected, otherwise returns <see cref="TimeZoneInfo.Local"/>.
    /// </returns>
    /// <exception cref="JSException">
    /// Thrown when JavaScript interop fails to execute the browser time zone detection.
    /// </exception>
    public async ValueTask<TimeZoneInfo> GetTimeZone(bool force = false)
    {
        if (!force && _cachedTimeZone is not null)
            return _cachedTimeZone;

        var module = await _moduleTask.Value;

        var browserTimeZone = await module.InvokeAsync<string>("browserTimeZone");

        if (string.IsNullOrWhiteSpace(browserTimeZone))
        {
            _cachedTimeZone = TimeZoneInfo.Local;
            return TimeZoneInfo.Local;
        }

        if (TimeZoneInfo.TryFindSystemTimeZoneById(browserTimeZone, out var timeZone))
        {
            _cachedTimeZone = timeZone;
            return timeZone;
        }

        _cachedTimeZone = TimeZoneInfo.Local;
        return TimeZoneInfo.Local;
    }

    /// <summary>
    /// Asynchronously retrieves the browser's language preference.
    /// The result is cached after the first successful retrieval unless forced to refresh.
    /// </summary>
    /// <param name="force">
    /// If <c>true</c>, forces a fresh retrieval from the browser, bypassing the cache.
    /// If <c>false</c> (default), returns the cached value if available.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask{String}"/> representing the asynchronous operation.
    /// Returns the browser's language code if successfully detected, otherwise returns <see cref="CultureInfo.CurrentUICulture"/>.
    /// The language code follows standard culture naming conventions (e.g., "en-US", "fr-FR").
    /// </returns>
    /// <exception cref="JSException">
    /// Thrown when JavaScript interop fails to execute the browser language detection.
    /// </exception>
    public async ValueTask<string> GetLanguage(bool force = false)
    {
        if (!force && _cachedLanguage is not null)
            return _cachedLanguage;

        var module = await _moduleTask.Value;

        _cachedLanguage = await module.InvokeAsync<string>("browserLanguage")
            ?? CultureInfo.CurrentUICulture.Name;

        return _cachedLanguage ?? string.Empty;
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }
}

