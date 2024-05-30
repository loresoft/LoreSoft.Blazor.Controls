using System.Timers;

namespace LoreSoft.Blazor.Controls;

public class DebounceValue<T> : IDisposable
{
    private readonly System.Timers.Timer _debounceTimer;
    private T _value;


    public DebounceValue(int interval = 800)
        : this(default, interval)
    {
    }

    public DebounceValue(T value, int interval = 800)
    {
        _value = value;
        _debounceTimer = new System.Timers.Timer();
        _debounceTimer.Interval = interval;
        _debounceTimer.AutoReset = false;
        _debounceTimer.Elapsed += OnElapsed;
    }

    public T Value
    {
        get => _value;
        set
        {
            if (EqualityComparer<T>.Default.Equals(_value, value))
                return;

            _value = value;
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }
    }

    public Action<T> Trigger { get; set; }

    private void OnElapsed(object sender, ElapsedEventArgs e)
    {
        Trigger?.Invoke(_value);
    }

    public void Dispose()
    {
        _debounceTimer.Elapsed -= OnElapsed;
        _debounceTimer.Dispose();
    }
}
