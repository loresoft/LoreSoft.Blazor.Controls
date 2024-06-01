namespace LoreSoft.Blazor.Controls.Utilities;

public static class ConstrainedAction
{
    public static Action<T> Debounce<T>(Action<T> action, TimeSpan? delay = null)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var interval = delay ?? TimeSpan.FromMilliseconds(800);
        var last = 0;

        return (T arguments) =>
        {
            var current = Interlocked.Increment(ref last);
            Task.Delay(interval).ContinueWith(task =>
            {
                if (current == last)
                    action(arguments);
            });
        };
    }

    public static Action Debounce(Action action, TimeSpan? delay = null)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var interval = delay ?? TimeSpan.FromMilliseconds(800);
        var last = 0;

        return () =>
        {
            var current = Interlocked.Increment(ref last);
            Task.Delay(interval).ContinueWith(task =>
            {
                if (current == last)
                    action();
            });
        };
    }


    public static Func<T, Task> Debounce<T>(Func<T, Task> action, TimeSpan? delay = null)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var interval = delay ?? TimeSpan.FromMilliseconds(800);
        var last = 0;

        return async (T arguments) =>
        {
            var current = Interlocked.Increment(ref last);
            await Task.Delay(interval);
            if (current == last)
                await action(arguments);
        };
    }

    public static Func<Task> Debounce(Func<Task> action, TimeSpan? delay = null)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var interval = delay ?? TimeSpan.FromMilliseconds(800);
        var last = 0;

        return async () =>
        {
            var current = Interlocked.Increment(ref last);
            await Task.Delay(interval);
            if (current == last)
                await action();
        };
    }


    public static Action<T> Throttle<T>(Action<T> action, TimeSpan? delay = null)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var interval = delay ?? TimeSpan.FromMilliseconds(800);

        Task task = null;
        var locker = new object();
        T arguments = default;

        return (T argument) =>
        {
            arguments = argument;
            if (task != null)
                return;

            lock (locker)
            {
                if (task != null)
                    return;

                task = Task.Delay(interval).ContinueWith(t =>
                {
                    action(arguments);
                    task = null;
                });
            }
        };
    }

    public static Action Throttle(Action action, TimeSpan? delay = null)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var interval = delay ?? TimeSpan.FromMilliseconds(800);

        Task task = null;
        var locker = new object();

        return () =>
        {
            if (task != null)
                return;

            lock (locker)
            {
                if (task != null)
                    return;

                task = Task.Delay(interval).ContinueWith(t =>
                {
                    action();
                    task = null;
                });
            }
        };
    }

}
