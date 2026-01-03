#nullable enable

using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents an individual toast notification element in a application.
/// Handles display, progress bar, timeout, close animation, and user interaction for a single toast.
/// </summary>
public partial class ToastElement : IDisposable
{
    /// <summary>
    /// Gets or sets the parent <see cref="ToastContainer"/> component.
    /// </summary>
    [CascadingParameter]
    private ToastContainer ToastContainer { get; set; } = default!;

    /// <summary>
    /// Gets or sets the unique identifier for this toast instance.
    /// </summary>
    [Parameter, EditorRequired]
    public Guid ToastId { get; set; }

    /// <summary>
    /// Gets or sets the settings for this toast, such as timeout, progress bar, and appearance.
    /// </summary>
    [Parameter, EditorRequired]
    public ToastSettings Settings { get; set; } = default!;

    /// <summary>
    /// Gets or sets the level of the toast (e.g., Information, Success, Warning, Error).
    /// </summary>
    [Parameter]
    public ToastLevel? Level { get; set; }

    /// <summary>
    /// Gets or sets the content to display in the toast.
    /// </summary>
    [Parameter]
    public RenderFragment? Message { get; set; }

    /// <summary>
    /// Gets the current progress value for the toast's progress bar (0-100).
    /// </summary>
    private int Progress { get; set; } = 100;

    private ToastTimer? _toastTimer;

    /// <summary>
    /// Gets a value indicating whether the toast is currently closing (for CSS transition).
    /// </summary>
    private bool Closing { get; set; }

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        if (Settings.Timeout == 0)
        {
            return;
        }

        if (Settings.ShowProgressBar == true)
        {
            _toastTimer = new ToastTimer(Settings.Timeout)
                .OnTick(CalculateProgressAsync)
                .OnElapsed(Close);
        }
        else
        {
            _toastTimer = new ToastTimer(Settings.Timeout)
                .OnElapsed(Close);
        }

        await _toastTimer.StartAsync();
    }

    /// <summary>
    /// Closes the toast, triggers the close animation, and removes it from the container after a delay.
    /// </summary>
    public void Close()
    {
        _toastTimer?.Pause();

        // start close css transition
        Closing = true;

        // delay removing toast to allow css transition to finish
        Task.Delay(TimeSpan.FromSeconds(1))
            .ContinueWith(_ => ToastContainer.RemoveToast(ToastId));
    }

    /// <summary>
    /// Pauses the toast countdown if progress should pause on hover.
    /// </summary>
    private void TryPauseCountdown()
    {
        if (Settings.PauseProgressOnHover!.Value)
        {
            Settings.ShowProgressBar = false;
            _toastTimer?.Pause();
        }
    }

    /// <summary>
    /// Resumes the toast countdown if progress should resume on hover exit.
    /// </summary>
    private void TryResumeCountdown()
    {
        if (Settings.PauseProgressOnHover!.Value)
        {
            Settings.ShowProgressBar = true;
            _toastTimer?.Resume();
        }
    }

    /// <summary>
    /// Updates the progress bar value as the toast timer ticks.
    /// </summary>
    /// <param name="percentComplete">The percent of time elapsed (0-100).</param>
    private async Task CalculateProgressAsync(int percentComplete)
    {
        Progress = 100 - percentComplete;
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Invokes the toast's click handler, if set.
    /// </summary>
    private void ToastClick() => Settings.OnClick?.Invoke();

    /// <summary>
    /// Gets the CSS class string for the toast element based on its level and settings.
    /// </summary>
    /// <returns>The CSS class string.</returns>
    private string? ToastClass()
    {
        var className = Level switch
        {
            ToastLevel.Information => "toast-information",
            ToastLevel.Success => "toast-success",
            ToastLevel.Warning => "toast-warning",
            ToastLevel.Error => "toast-error",
            _ => "toast-information",
        };

        return CssBuilder.Pool.Use(builder =>
        {
            return builder
                .AddClass("toast-element")
                .AddClass(className)
                .AddClass(Settings.ClassName)
                .ToString();
        });

    }

    /// <summary>
    /// Gets the CSS style string for the toast element, used for close animation.
    /// </summary>
    /// <returns>The CSS style string, or <c>null</c> if not closing.</returns>
    private string? ToastStyle()
    {
        if (!Closing)
            return null;

        return "opacity: 0;transition: opacity 1s linear;";
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _toastTimer?.Dispose();
        _toastTimer = null;
        GC.SuppressFinalize(this);
    }
}
