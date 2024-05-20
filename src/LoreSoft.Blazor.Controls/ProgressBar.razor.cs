using System.Timers;

using Microsoft.AspNetCore.Components;

using Timer = System.Timers.Timer;

namespace LoreSoft.Blazor.Controls;

public partial class ProgressBar : ComponentBase, IDisposable
{
    private Timer _progressTimer;
    private Timer _completeTimer;

    [Parameter]
    public string Color { get; set; } = "#29d";

    [Parameter]
    public int IncrementDuration { get; set; } = 800;

    [Parameter]
    public int AnimationDuration { get; set; } = 200;

    [Parameter]
    public double MinimumProgress { get; set; } = 0.05;

    [Inject]
    protected ProgressBarState State { get; set; }


    protected int Opacity { get; set; }

    protected double Progress { get; set; }


    protected string ContainerStyle => $"opacity: {Opacity}; transition: opacity {AnimationDuration}ms linear 0s;";

    protected string BarStyle => $"background: {Color}; margin-left: {(-1 + Progress) * 100}%; transition: margin-left {AnimationDuration}ms linear 0s";

    protected string PegStyle => $"box-shadow: 0 0 10px {Color}, 0 0 5px {Color};";

    protected string IconStyle => $"border-color: {Color} transparent transparent {Color};";


    protected override void OnInitialized()
    {
        State.OnChange += OnProgressStateChange;

        _progressTimer = new Timer();
        _progressTimer.Interval = IncrementDuration;
        _progressTimer.AutoReset = true;
        _progressTimer.Elapsed += OnIncrement;

        _completeTimer = new Timer();
        _completeTimer.Interval = AnimationDuration;
        _completeTimer.AutoReset = false;
        _completeTimer.Elapsed += OnComplete;

    }

    private void OnComplete(object sender, ElapsedEventArgs e)
    {
        InvokeAsync(() =>
        {
            Opacity = 0;
            StateHasChanged();
        });
    }

    private void OnIncrement(object sender, ElapsedEventArgs e)
    {
        InvokeAsync(() =>
        {
            Progress += (1 - Progress) * 0.2;
            StateHasChanged();
        });
    }

    private void OnProgressStateChange()
    {
        InvokeAsync(() =>
        {
            if (State.Loading && Opacity != 1)
            {
                // reset bar
                Progress = MinimumProgress;
                Opacity = 1;

                // timer to progress bar
                _progressTimer.Start();
            }
            else if (State.Loading == false)
            {
                // progress to 100%
                _progressTimer.Stop();
                Progress = 1;

                // delay hiding
                _completeTimer.Start();
            }

            StateHasChanged();
        });
    }

    public void Dispose()
    {
        State.OnChange -= OnProgressStateChange;
        _progressTimer?.Dispose();
        _completeTimer?.Dispose();
    }
}
