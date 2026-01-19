// Ignore Spelling: debounce, debounced, debounces

#nullable enable

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Provides a debounced action handler that invokes an asynchronous action after a specified delay.
/// Multiple rapid calls will cancel previous pending invocations, ensuring only the last call executes.
/// </summary>
/// <param name="defaultDelay">The default delay to wait before invoking the action. If not specified, defaults to 800 milliseconds.</param>
public class DebounceAction(TimeSpan? defaultDelay = null)
{
    private readonly TimeSpan _defaultDelay = defaultDelay ?? TimeSpan.FromMilliseconds(800);
    private int _counter;

    /// <summary>
    /// Debounces the specified action, ensuring it only executes after the delay period with no new calls.
    /// </summary>
    /// <param name="action">The asynchronous action to invoke after the debounce delay.</param>
    /// <param name="delay">The delay to wait before invoking the action. If not specified, uses the default delay.</param>
    /// <returns>A <see cref="Task"/> that completes when the debounce period ends, either by invoking the action or being cancelled by a new call.</returns>
    public async Task Debounce(Func<Task> action, TimeSpan? delay = null)
    {
        // Increment the counter to represent a new action request
        var current = Interlocked.Increment(ref _counter);

        // Wait for the specified debounce delay
        await Task.Delay(delay ?? _defaultDelay);

        // If new requests have been made, return without invoking the action
        if (current != _counter)
            return;

        // Invoke the action
        await action();
    }
}

/// <summary>
/// Provides a debounced action handler with value tracking that invokes an asynchronous action after a specified delay.
/// Multiple rapid calls will cancel previous pending invocations, ensuring only the last call executes.
/// Optionally skips duplicate values to prevent redundant action invocations.
/// </summary>
/// <typeparam name="T">The type of the value being debounced.</typeparam>
/// <param name="defaultDelay">The default delay to wait before invoking the action. If not specified, defaults to 800 milliseconds.</param>
/// <param name="skipDuplicates">If <c>true</c>, skips invoking the action when the value is the same as the last invoked value. Defaults to <c>true</c>.</param>
/// <example>
/// <code>
/// &lt;InputText @bind-Value="searchText"
///            @bind-Value:event="oninput"
///            @oninput="OnSearchInput" /&gt;
///
/// @code {
///     private string searchText = "";
///     private readonly DebounceAction&lt;string&gt; _debouncer = new();
///
///     private async Task OnSearchInput(ChangeEventArgs e)
///     {
///         var value = e.Value?.ToString() ?? "";
///         await _debouncer.Debounce(value, PerformSearch);
///     }
///
///     private async Task PerformSearch(string query)
///     {
///         Console.WriteLine($"Searching for: {query}");
///         await Task.CompletedTask;
///     }
/// }
/// </code>
/// </example>
public class DebounceAction<T>(TimeSpan? defaultDelay = null, bool skipDuplicates = true)
{
    private readonly TimeSpan _defaultDelay = defaultDelay ?? TimeSpan.FromMilliseconds(800);
    private readonly bool _skipDuplicates = skipDuplicates;

    private int _counter;
    private T? _lastValue;

    /// <summary>
    /// Debounces the specified action with the provided value, ensuring it only executes after the delay period with no new calls.
    /// If <c>skipDuplicates</c> is enabled, the action will not be invoked if the value matches the last invoked value.
    /// </summary>
    /// <param name="value">The value to pass to the action.</param>
    /// <param name="action">The asynchronous action to invoke with the value after the debounce delay.</param>
    /// <param name="delay">The delay to wait before invoking the action. If not specified, uses the default delay.</param>
    /// <returns>A <see cref="Task"/> that completes when the debounce period ends, either by invoking the action or being cancelled by a new call.</returns>
    public async Task Debounce(T value, Func<T, Task> action, TimeSpan? delay = null)
    {
        // Increment the counter to represent a new action request
        var current = Interlocked.Increment(ref _counter);

        // Wait for the specified debounce delay
        await Task.Delay(delay ?? _defaultDelay);

        // If new requests have been made, return without invoking the action
        if (current != _counter)
            return;

        // Optionally skip if value same as last called value
        if (_skipDuplicates && EqualityComparer<T>.Default.Equals(value, _lastValue))
            return;

        // Update the last value and invoke the action
        _lastValue = value;
        await action(value);
    }
}
