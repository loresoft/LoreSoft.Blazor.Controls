#nullable enable

using System;
using System.Diagnostics;

using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

public partial class ToastElement : IDisposable
{
    [CascadingParameter]
    private ToastContainer ToastContainer { get; set; } = default!;

    [Parameter, EditorRequired]
    public Guid ToastId { get; set; }

    [Parameter, EditorRequired]
    public ToastSettings Settings { get; set; } = default!;

    [Parameter]
    public ToastLevel? Level { get; set; }

    [Parameter]
    public RenderFragment? Message { get; set; }

    private int Progress { get; set; } = 100;

    private ToastTimer? _toastTimer;

    private bool Closing { get; set; }

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

    public void Close()
    {
        _toastTimer?.Pause();

        // start close css transition
        Closing = true;

        // delay removing toast to allow css transition to finish
        Task.Delay(TimeSpan.FromSeconds(1))
            .ContinueWith(_ => ToastContainer.RemoveToast(ToastId));
    }

    private void TryPauseCountdown()
    {
        if (Settings.PauseProgressOnHover!.Value)
        {
            Settings.ShowProgressBar = false;
            _toastTimer?.Pause();
        }
    }

    private void TryResumeCountdown()
    {
        if (Settings.PauseProgressOnHover!.Value)
        {
            Settings.ShowProgressBar = true;
            _toastTimer?.Resume();
        }
    }

    private async Task CalculateProgressAsync(int percentComplete)
    {
        Progress = 100 - percentComplete;
        await InvokeAsync(StateHasChanged);
    }

    private void ToastClick() => Settings.OnClick?.Invoke();

    private string ToastClass()
    {
        var className = Level switch
        {
            ToastLevel.Information => "toast-information",
            ToastLevel.Success => "toast-success",
            ToastLevel.Warning => "toast-warning",
            ToastLevel.Error => "toast-error",
            _ => "toast-information",
        };

        return CssBuilder
            .Default("toast")
            .AddClass(className)
            .AddClass(Settings.ClassName)
            .ToString();

    }

    private string? ToastStyle()
    {
        if (!Closing)
            return null;

        return "opacity: 0;transition: opacity 1s linear;";
    }

    public void Dispose()
    {
        _toastTimer?.Dispose();
        _toastTimer = null;
    }

}
