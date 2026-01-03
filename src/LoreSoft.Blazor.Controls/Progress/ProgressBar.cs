using System.Timers;

using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using Timer = System.Timers.Timer;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// A component that displays a progress bar with animated transitions.
/// Progress is managed via a <see cref="ProgressBarState"/> service.
/// </summary>
public class ProgressBar : ComponentBase, IDisposable
{
    private readonly Timer _progressTimer;
    private readonly Timer _completeTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressBar"/> class.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the <see cref="ProgressBarState"/> used to control the progress bar.
    /// </summary>
    [Inject]
    public required ProgressBarState State { get; set; }

    /// <summary>
    /// Gets or sets the color of the progress bar. Default is a shade of blue (#29d).
    /// </summary>
    [Parameter]
    public string Color { get; set; } = "#29d";

    /// <summary>
    /// Gets or sets the interval (in milliseconds) between progress increments. Default is 800ms.
    /// </summary>
    [Parameter]
    public int IncrementDuration { get; set; } = 800;

    /// <summary>
    /// Gets or sets the duration (in milliseconds) for progress bar animation transitions. Default is 200ms.
    /// </summary>
    [Parameter]
    public int AnimationDuration { get; set; } = 200;

    /// <summary>
    /// Gets or sets the minimum progress value when the bar starts. Default is 0.05 (5%).
    /// </summary>
    [Parameter]
    public double MinimumProgress { get; set; } = 0.05;

    /// <summary>
    /// Additional attributes to be applied to the root container element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> Attributes { get; set; } = [];

    /// <summary>
    /// Gets or sets the opacity of the progress bar container.
    /// </summary>
    protected int Opacity { get; set; }

    /// <summary>
    /// Gets or sets the current progress value (0.0 to 1.0).
    /// </summary>
    protected double Progress { get; set; }

    /// <summary>
    /// Gets or sets the CSS class name for the container.
    /// </summary>
    protected string? ClassName { get; set; }

    /// <summary>
    /// Gets the CSS style for the progress bar container.
    /// </summary>
    protected string ContainerStyle => $"opacity: {Opacity}; transition: opacity {AnimationDuration}ms linear 0s;";

    /// <summary>
    /// Gets the CSS style for the progress bar.
    /// </summary>
    protected string BarStyle => $"background: {Color}; margin-left: {(-1 + Progress) * 100}%; transition: margin-left {AnimationDuration}ms linear 0s";

    /// <summary>
    /// Gets the CSS style for the progress bar peg.
    /// </summary>
    protected string PegStyle => $"box-shadow: 0 0 10px {Color}, 0 0 5px {Color};";

    /// <summary>
    /// Gets the CSS style for the spinner icon.
    /// </summary>
    protected string IconStyle => $"border-color: {Color} transparent transparent {Color};";

    /// <summary>
    /// Initializes the component and subscribes to progress state changes.
    /// </summary>
    protected override void OnInitialized()
    {
        State.OnChange += OnProgressStateChange;

        _progressTimer.Interval = IncrementDuration;
        _completeTimer.Interval = AnimationDuration;
    }

    /// <summary>
    /// Updates the CSS class name after parameters are set.
    /// </summary>
    protected override void OnParametersSet()
    {
        // update only after parameters are set
        ClassName = CssBuilder.Pool.Use(builder => builder
            .AddClass("progress-container")
            .MergeClass(Attributes)
            .ToString()
        );
    }

    /// <summary>
    /// Builds the render tree for the progress bar component.
    /// </summary>
    /// <param name="builder">The <see cref="RenderTreeBuilder"/> used to build the component's render tree.</param>
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

    /// <summary>
    /// Handles the completion of the progress bar animation.
    /// </summary>
    private void OnComplete(object? sender, ElapsedEventArgs e)
    {
        InvokeAsync(() =>
        {
            Opacity = 0;
            StateHasChanged();
        });
    }

    /// <summary>
    /// Handles progress increments for the bar animation.
    /// </summary>
    private void OnIncrement(object? sender, ElapsedEventArgs e)
    {
        InvokeAsync(() =>
        {
            Progress += (1 - Progress) * 0.2;
            StateHasChanged();
        });
    }

    /// <summary>
    /// Handles changes in the progress bar state.
    /// </summary>
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

    /// <summary>
    /// Disposes timers and unsubscribes from state changes.
    /// </summary>
    public void Dispose()
    {
        State.OnChange -= OnProgressStateChange;

        _progressTimer?.Dispose();
        _completeTimer?.Dispose();

        GC.SuppressFinalize(this);
    }
}
