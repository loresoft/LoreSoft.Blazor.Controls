// Ignore Spelling: debounce, debounced

#nullable enable

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Provides a debounced value wrapper that invokes an action after a specified delay when the value changes.
/// Useful for scenarios such as search boxes where rapid input should not trigger immediate actions.
/// </summary>
/// <typeparam name="T">The type of the value being debounced.</typeparam>
public class DebounceValue<T>
{
    /// <summary>
    /// The default delay used for debouncing value changes.
    /// </summary>
    public static readonly TimeSpan DefaultDelay = TimeSpan.FromMilliseconds(800);

    private T? _value;
    private int _counter = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="DebounceValue{T}"/> class with the specified action, delay, and initial value.
    /// </summary>
    /// <param name="action">The action to invoke after the debounce delay.</param>
    /// <param name="delay">The delay to wait before invoking the action.</param>
    /// <param name="value">The initial value.</param>
    public DebounceValue(Action<T?> action, TimeSpan? delay = null, T? value = default)
    {
        Action = action;
        Delay = delay ?? DefaultDelay;
        _value = value;
    }

    /// <summary>
    /// Gets or sets the debounced value.
    /// Setting the value will schedule the action to be invoked after the specified delay,
    /// unless the value changes again before the delay elapses.
    /// </summary>
    public T? Value
    {
        get => _value;
        set
        {
            if (EqualityComparer<T>.Default.Equals(_value, value))
                return;

            _value = value;

            var current = Interlocked.Increment(ref _counter);

            Task.Delay(Delay).ContinueWith(_ =>
            {
                if (current == _counter)
                    Action(_value);
            });
        }
    }

    /// <summary>
    /// Gets the action to be invoked after the debounce delay.
    /// </summary>
    public Action<T?> Action { get; }

    /// <summary>
    /// Gets the delay used for debouncing value changes.
    /// </summary>
    public TimeSpan Delay { get; }
}
