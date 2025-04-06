using System.Timers;

using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using Timer = System.Timers.Timer;

namespace LoreSoft.Blazor.Controls;

public class ProgressBar : ComponentBase, IDisposable
{
    private readonly Timer _progressTimer;
    private readonly Timer _completeTimer;

    public ProgressBar()
    {
        _progressTimer = new Timer();
        _progressTimer.Interval = IncrementDuration;
        _progressTimer.AutoReset = true;
        _progressTimer.Elapsed += OnIncrement;

        _completeTimer = new Timer();
        _completeTimer.Interval = AnimationDuration;
        _completeTimer.AutoReset = false;
        _completeTimer.Elapsed += OnComplete;
    }

    [Inject]
    public required ProgressBarState State { get; set; }

    [Parameter]
    public string Color { get; set; } = "#29d";

    [Parameter]
    public int IncrementDuration { get; set; } = 800;

    [Parameter]
    public int AnimationDuration { get; set; } = 200;

    [Parameter]
    public double MinimumProgress { get; set; } = 0.05;

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> Attributes { get; set; } = [];


    protected int Opacity { get; set; }

    protected double Progress { get; set; }

    protected string? ClassName { get; set; }

    protected string ContainerStyle => $"opacity: {Opacity}; transition: opacity {AnimationDuration}ms linear 0s;";

    protected string BarStyle => $"background: {Color}; margin-left: {(-1 + Progress) * 100}%; transition: margin-left {AnimationDuration}ms linear 0s";

    protected string PegStyle => $"box-shadow: 0 0 10px {Color}, 0 0 5px {Color};";

    protected string IconStyle => $"border-color: {Color} transparent transparent {Color};";


    protected override void OnInitialized()
    {
        State.OnChange += OnProgressStateChange;

        _progressTimer.Interval = IncrementDuration;
        _completeTimer.Interval = AnimationDuration;
    }

    protected override void OnParametersSet()
    {
        // update only after parameters are set
        ClassName = new CssBuilder("progress-container")
            .MergeClass(Attributes)
            .ToString();
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", ClassName);
        builder.AddAttribute(2, "style", ContainerStyle);
        builder.AddMultipleAttributes(3, Attributes);

        builder.OpenElement(4, "div");
        builder.AddAttribute(5, "class", "progress-status-bar");
        builder.AddAttribute(6, "style", BarStyle);

        builder.OpenElement(7, "div");
        builder.AddAttribute(8, "class", "progress-status-peg");
        builder.AddAttribute(9, "style", PegStyle);
        builder.CloseElement(); // peg

        builder.CloseElement(); // bar

        builder.OpenElement(10, "div");
        builder.AddAttribute(11, "class", "progress-status-spinner");

        builder.OpenElement(12, "div");
        builder.AddAttribute(13, "class", "progress-status-spinner-icon");
        builder.AddAttribute(14, "style", IconStyle);
        builder.CloseElement(); // icon

        builder.CloseElement(); // spinner

        builder.CloseElement(); // container
    }

    private void OnComplete(object? sender, ElapsedEventArgs e)
    {
        InvokeAsync(() =>
        {
            Opacity = 0;
            StateHasChanged();
        });
    }

    private void OnIncrement(object? sender, ElapsedEventArgs e)
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
            else if (!State.Loading)
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

        GC.SuppressFinalize(this);
    }
}
