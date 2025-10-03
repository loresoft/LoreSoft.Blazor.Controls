using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// A Blazor component that provides responsive breakpoint monitoring and notifications.
/// This component tracks viewport width changes and notifies subscribers when breakpoints are crossed.
/// </summary>
public partial class BreakpointProvider : IAsyncDisposable
{
    private readonly List<Action<BreakpointChanged>> _subscribers = [];

    private IJSObjectReference? _module;
    private IJSObjectReference? _monitor;
    private DotNetObjectReference<BreakpointProvider>? _dotNetRef;

    /// <summary>
    /// Gets or sets the JavaScript runtime instance used for interop operations.
    /// </summary>
    [Inject]
    public required IJSRuntime JavaScript { get; set; }

    /// <summary>
    /// Gets or sets the child content to be rendered within this component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets custom breakpoint definitions as a dictionary of breakpoint names and their pixel values.
    /// If not provided, default breakpoints will be used (xs: 0, sm: 576, md: 768, lg: 992, xl: 1200, xxl: 1400).
    /// </summary>
    /// <example>
    /// <code>
    /// var customBreakpoints = new Dictionary&lt;string, int&gt;
    /// {
    ///     { "mobile", 0 },
    ///     { "tablet", 768 },
    ///     { "desktop", 1024 }
    /// };
    /// </code>
    /// </example>
    [Parameter]
    public Dictionary<string, int>? Breakpoints { get; set; }

    /// <summary>
    /// Gets or sets the debounce delay in milliseconds for resize events.
    /// This helps prevent excessive breakpoint change notifications during window resizing.
    /// Default value is 250 milliseconds.
    /// </summary>
    [Parameter]
    public int Debounce { get; set; } = 250;

    /// <summary>
    /// Gets the current breakpoint name based on the viewport width.
    /// </summary>
    public string Current { get; private set; } = string.Empty;

    /// <summary>
    /// Initializes the breakpoint monitoring system after the component is rendered.
    /// This method sets up JavaScript interop and starts monitoring viewport changes.
    /// </summary>
    /// <param name="firstRender">True if this is the first time the component is being rendered.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        _module = await JavaScript.InvokeAsync<IJSObjectReference>(
            "import", "./_content/LoreSoft.Blazor.Controls/js/breakpoint.js");

        // Pass custom breakpoints to JavaScript or null for defaults
        var breakpointsToUse = Breakpoints != null ? (object)Breakpoints : null;

        _monitor = await _module.InvokeAsync<IJSObjectReference>(
            "createMonitor", breakpointsToUse, Debounce);

        _dotNetRef = DotNetObjectReference.Create(this);
        await _monitor.InvokeVoidAsync("registerDotNetHelper", _dotNetRef);

        var current = await _monitor.InvokeAsync<string>("getCurrent");
        OnBreakpointChanged(new() { Current = current });

        await _monitor.InvokeVoidAsync("start");

        StateHasChanged();
    }

    /// <summary>
    /// Handles breakpoint change notifications from JavaScript.
    /// This method is called by JavaScript when a breakpoint change is detected.
    /// </summary>
    /// <param name="data">The breakpoint change data containing current breakpoint, previous breakpoint, and width information.</param>
    [JSInvokable]
    public void OnBreakpointChanged(BreakpointChanged data)
    {
        if (data.Current == Current)
            return;

        Current = data.Current;

        // Notify all subscribers
        foreach (var subscriber in _subscribers)
            subscriber.Invoke(data);

        StateHasChanged();
    }

    /// <summary>
    /// Subscribes a callback function to receive breakpoint change notifications.
    /// The callback will be invoked whenever a breakpoint change occurs.
    /// </summary>
    /// <param name="callback">The callback function to be invoked on breakpoint changes.</param>
    public void Subscribe(Action<BreakpointChanged> callback)
    {
        if (!_subscribers.Contains(callback))
            _subscribers.Add(callback);
    }

    /// <summary>
    /// Unsubscribes a callback function from breakpoint change notifications.
    /// </summary>
    /// <param name="callback">The callback function to remove from the subscription list.</param>
    public void Unsubscribe(Action<BreakpointChanged> callback)
    {
        _subscribers.Remove(callback);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
    /// This method cleans up JavaScript interop references and clears all subscriptions.
    /// </summary>
    /// <returns>A task representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_subscribers.Count > 0)
            _subscribers.Clear();

        if (_monitor != null)
        {
            await _monitor.InvokeVoidAsync("dispose");
            await _monitor.DisposeAsync();
        }

        if (_module != null)
            await _module.DisposeAsync();

        _dotNetRef?.Dispose();

        GC.SuppressFinalize(this);
    }
}
