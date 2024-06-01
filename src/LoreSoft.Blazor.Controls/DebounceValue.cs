#nullable enable

namespace LoreSoft.Blazor.Controls;

public class DebounceValue<T>
{
    public static readonly TimeSpan DefaultDelay = TimeSpan.FromMilliseconds(800);

    private T? _value;
    private int _last = 0;

    public DebounceValue(Action<T?> action)
        : this(action, DefaultDelay, default)
    {
    }

    public DebounceValue(Action<T?> action, TimeSpan delay)
        : this(action, delay, default)
    {
    }

    public DebounceValue(Action<T?> action, TimeSpan delay, T? value)
    {
        Action = action;
        Delay = delay;
        _value = value;
    }

    public T? Value
    {
        get => _value;
        set
        {
            if (EqualityComparer<T>.Default.Equals(_value, value))
                return;

            _value = value;

            var current = Interlocked.Increment(ref _last);

            Task.Delay(Delay).ContinueWith(_ =>
            {
                if (current == _last)
                    Action(_value);
            });
        }
    }

    public Action<T?> Action { get; }

    public TimeSpan Delay { get; }
}
