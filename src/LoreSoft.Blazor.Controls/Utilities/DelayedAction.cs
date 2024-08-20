// Ignore Spelling: Debounce

namespace LoreSoft.Blazor.Controls.Utilities;

#nullable enable

// https://www.meziantou.net/debouncing-throttling-javascript-events-in-a-blazor-application.htm
public static class DelayedAction
{
    public static readonly TimeSpan DefaultDelay = TimeSpan.FromMilliseconds(800);

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
