# nullable enable

namespace LoreSoft.Blazor.Controls;

internal class ToastTimer : IDisposable
{
    private PeriodicTimer _timer;
    private readonly int _ticksToTimeout;
    private readonly CancellationToken _cancellationToken;
    private int _percentComplete;
    private bool _isPaused;
    private Func<int, Task>? _tickDelegate;
    private Action? _elapsedDelegate;

    internal ToastTimer(int timeout, CancellationToken cancellationToken = default)
    {
        _ticksToTimeout = 100;
        _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(timeout * 10));
        _cancellationToken = cancellationToken;
    }

    internal ToastTimer OnTick(Func<int, Task> updateProgressDelegate)
    {
        _tickDelegate = updateProgressDelegate;
        return this;
    }

    internal ToastTimer OnElapsed(Action elapsedDelegate)
    {
        _elapsedDelegate = elapsedDelegate;
        return this;
    }

    internal async Task StartAsync()
    {
        _percentComplete = 0;
        await DoWorkAsync();
    }

    internal void Pause()
    {
        _isPaused = true;
    }

    internal void Resume()
    {
        _isPaused = false;
    }

    private async Task DoWorkAsync()
    {
        while (await _timer.WaitForNextTickAsync(_cancellationToken) && !_cancellationToken.IsCancellationRequested)
        {
            if (!_isPaused)
                _percentComplete++;

            if (_tickDelegate != null)
                await _tickDelegate(_percentComplete);

            if (_percentComplete == _ticksToTimeout)
                _elapsedDelegate?.Invoke();
        }
    }

    public void Dispose() => _timer.Dispose();
}
