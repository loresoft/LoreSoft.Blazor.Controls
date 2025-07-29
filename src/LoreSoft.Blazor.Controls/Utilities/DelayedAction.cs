// Ignore Spelling: Debounce

namespace LoreSoft.Blazor.Controls.Utilities;

/// <summary>
/// Provides utility methods for debouncing and throttling actions and asynchronous delegates.
/// Useful for reducing the frequency of event handling in applications (e.g., input, resize, scroll).
/// </summary>
/// <remarks>
/// See: https://www.meziantou.net/debouncing-throttling-javascript-events-in-a-blazor-application.htm
/// </remarks>
public static class DelayedAction
{
    /// <summary>
    /// The default delay interval used for debouncing and throttling (800 milliseconds).
    /// </summary>
    public static readonly TimeSpan DefaultDelay = TimeSpan.FromMilliseconds(800);

    /// <summary>
    /// Creates a debounced version of the specified <see cref="Action"/>.
    /// The action will only be invoked after the specified interval has elapsed without further calls.
    /// </summary>
    /// <param name="action">The action to debounce.</param>
    /// <param name="interval">The debounce interval. If null, <see cref="DefaultDelay"/> is used.</param>
    /// <returns>A debounced <see cref="Action"/> delegate.</returns>
    public static Action Debounce(this Action action, TimeSpan? interval = null)
    {
        ArgumentNullException.ThrowIfNull(action);

        var delay = interval ?? DefaultDelay;
        var last = 0;

        return () =>
        {
            var current = Interlocked.Increment(ref last);
            Task.Delay(delay)
                .ContinueWith(_ =>
                {
                    if (current == last)
                        action();
                });
        };
    }

    /// <summary>
    /// Creates a debounced version of the specified asynchronous <see cref="Func{Task}"/>.
    /// The function will only be invoked after the specified interval has elapsed without further calls.
    /// </summary>
    /// <param name="action">The asynchronous function to debounce.</param>
    /// <param name="interval">The debounce interval. If null, <see cref="DefaultDelay"/> is used.</param>
    /// <returns>A debounced <see cref="Func{Task}"/> delegate.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1047:Non-asynchronous method name should not end with 'Async'", Justification = "Delegate function")]
    public static Func<Task> DebounceAsync(this Func<Task> action, TimeSpan? interval = null)
    {
        ArgumentNullException.ThrowIfNull(action);

        var delay = interval ?? DefaultDelay;
        var last = 0;

        return async () =>
        {
            var current = Interlocked.Increment(ref last);
            await Task.Delay(delay);
            if (current == last)
            {
                await action();
            }
        };
    }

    /// <summary>
    /// Creates a debounced version of the specified <see cref="Action{T}"/>.
    /// The action will only be invoked after the specified interval has elapsed without further calls.
    /// </summary>
    /// <typeparam name="T">The type of the action argument.</typeparam>
    /// <param name="action">The action to debounce.</param>
    /// <param name="interval">The debounce interval. If null, <see cref="DefaultDelay"/> is used.</param>
    /// <returns>A debounced <see cref="Action{T}"/> delegate.</returns>
    public static Action<T> Debounce<T>(this Action<T> action, TimeSpan? interval = null)
    {
        ArgumentNullException.ThrowIfNull(action);

        var delay = interval ?? DefaultDelay;
        var last = 0;

        return arg =>
        {
            var current = Interlocked.Increment(ref last);
            Task.Delay(delay)
                .ContinueWith(_ =>
                {
                    if (current == last)
                        action(arg);
                });
        };
    }

    /// <summary>
    /// Creates a debounced version of the specified asynchronous <see cref="Func{T, Task}"/>.
    /// The function will only be invoked after the specified interval has elapsed without further calls.
    /// </summary>
    /// <typeparam name="T">The type of the function argument.</typeparam>
    /// <param name="action">The asynchronous function to debounce.</param>
    /// <param name="interval">The debounce interval. If null, <see cref="DefaultDelay"/> is used.</param>
    /// <returns>A debounced <see cref="Func{T, Task}"/> delegate.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1047:Non-asynchronous method name should not end with 'Async'", Justification = "Delegate function")]
    public static Func<T, Task> DebounceAsync<T>(this Func<T, Task> action, TimeSpan? interval = null)
    {
        ArgumentNullException.ThrowIfNull(action);

        var delay = interval ?? DefaultDelay;
        var last = 0;

        return async arg =>
        {
            var current = Interlocked.Increment(ref last);
            await Task.Delay(delay);
            if (current == last)
            {
                await action(arg);
            }
        };
    }

    /// <summary>
    /// Creates a throttled version of the specified <see cref="Action"/>.
    /// The action will only be invoked at most once per specified interval.
    /// </summary>
    /// <param name="action">The action to throttle.</param>
    /// <param name="interval">The throttle interval. If null, <see cref="DefaultDelay"/> is used.</param>
    /// <returns>A throttled <see cref="Action"/> delegate.</returns>
    public static Action Throttle(this Action action, TimeSpan? interval = null)
    {
        ArgumentNullException.ThrowIfNull(action);

        var delay = interval ?? DefaultDelay;

        Task? task = null;
        var l = new object();

        return () =>
        {
            if (task != null)
                return;

            lock (l)
            {
                if (task != null)
                    return;

                task = Task.Delay(delay)
                    .ContinueWith(_ =>
                    {
                        action();
                        task = null;
                    });
            }
        };
    }

    /// <summary>
    /// Creates a throttled version of the specified <see cref="Action{T}"/>.
    /// The action will only be invoked at most once per specified interval.
    /// </summary>
    /// <typeparam name="T">The type of the action argument.</typeparam>
    /// <param name="action">The action to throttle.</param>
    /// <param name="interval">The throttle interval. If null, <see cref="DefaultDelay"/> is used.</param>
    /// <returns>A throttled <see cref="Action{T}"/> delegate.</returns>
    public static Action<T> Throttle<T>(this Action<T> action, TimeSpan? interval = null)
    {
        ArgumentNullException.ThrowIfNull(action);

        var delay = interval ?? DefaultDelay;

        Task? task = null;
        var l = new object();
        T? args = default;

        return (T arg) =>
        {
            args = arg;
            if (task != null)
                return;

            lock (l)
            {
                if (task != null)
                    return;

                task = Task.Delay(delay)
                    .ContinueWith(_ =>
                    {
                        action(args);
                        task = null;
                    });
            }
        };
    }
}
