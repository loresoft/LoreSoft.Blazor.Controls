# nullable enable

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Provides a timer for toast notifications in applications.
/// Supports progress updates, elapsed events, pausing, and resuming.
/// </summary>
internal class ToastTimer : IDisposable
{
    private PeriodicTimer _timer;
    private readonly int _ticksToTimeout;
    private readonly CancellationToken _cancellationToken;
    private int _percentComplete;
    private bool _isPaused;
    private Func<int, Task>? _tickDelegate;
    private Action? _elapsedDelegate;

    /// <summary>
    /// Initializes a new instance of the <see cref="ToastTimer"/> class.
    /// </summary>
    /// <param name="timeout">The timeout duration in seconds for the toast notification.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the timer.</param>
    internal ToastTimer(int timeout, CancellationToken cancellationToken = default)
    {
        _ticksToTimeout = 100;
        _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(timeout * 10));
        _cancellationToken = cancellationToken;
    }

    /// <summary>
    /// Registers a delegate to be called on each timer tick, providing the percent complete.
    /// </summary>
    /// <param name="updateProgressDelegate">A delegate that receives the percent complete (0-100).</param>
    /// <returns>The current <see cref="ToastTimer"/> instance for chaining.</returns>
    internal ToastTimer OnTick(Func<int, Task> updateProgressDelegate)
    {
        _tickDelegate = updateProgressDelegate;
        return this;
    }

    /// <summary>
    /// Registers a delegate to be called when the timer has elapsed.
    /// </summary>
    /// <param name="elapsedDelegate">A delegate to invoke when the timer completes.</param>
    /// <returns>The current <see cref="ToastTimer"/> instance for chaining.</returns>
    internal ToastTimer OnElapsed(Action elapsedDelegate)
    {
        _elapsedDelegate = elapsedDelegate;
        return this;
    }

    /// <summary>
    /// Starts the timer asynchronously.
    /// </summary>
    internal async Task StartAsync()
    {
        _percentComplete = 0;
        await DoWorkAsync();
    }

    /// <summary>
    /// Pauses the timer, stopping progress updates.
    /// </summary>
    internal void Pause()
    {
        _isPaused = true;
    }

    /// <summary>
    /// Resumes the timer if it was paused.
    /// </summary>
    internal void Resume()
    {
        _isPaused = false;
    }

    /// <summary>
    /// Performs the timer work, invoking tick and elapsed delegates as appropriate.
    /// </summary>
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

    /// <summary>
    /// Disposes the timer and releases resources.
    /// </summary>
    public void Dispose() => _timer.Dispose();
}
