using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreSoft.Blazor.Controls.Utilities;

public static class ConstrainedAction
{
    public static Action<T> Debounce<T>(Action<T> action, TimeSpan interval)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

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

    public static Action<T> Throttle<T>(Action<T> action, TimeSpan interval)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

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
}
