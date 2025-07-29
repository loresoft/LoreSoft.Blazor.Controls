#nullable enable

using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// A component that displays toast notifications.
/// Manages toast queueing, display, removal, and clearing, with support for navigation events and customizable appearance.
/// </summary>
public partial class ToastContainer : IDisposable
{
    /// <summary>
    /// Gets or sets the injected <see cref="IToaster"/> service for showing and clearing toasts.
    /// </summary>
    [Inject]
    protected IToaster Toaster { get; set; } = default!;

    /// <summary>
    /// Gets or sets the injected <see cref="NavigationManager"/> for handling navigation events.
    /// </summary>
    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>
    /// Gets or sets the position of the toast container on the screen. Default is <see cref="ToastPosition.TopRight"/>.
    /// </summary>
    [Parameter]
    public ToastPosition Position { get; set; } = ToastPosition.TopRight;

    /// <summary>
    /// Gets or sets the default timeout (in seconds) for toast notifications. Default is 5 seconds.
    /// </summary>
    [Parameter]
    public int Timeout { get; set; } = 5;

    /// <summary>
    /// Gets or sets the maximum number of toasts displayed at once.
    /// Additional toasts are queued until space is available. Default is 10.
    /// </summary>
    [Parameter]
    public int MaxToast { get; set; } = 10;

    /// <summary>
    /// Gets or sets a value indicating whether to clear all toasts when navigation occurs.
    /// </summary>
    [Parameter]
    public bool ClearOnNavigation { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show a progress bar on each toast. Default is <c>true</c>.
    /// </summary>
    [Parameter]
    public bool ShowProgressBar { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show a close button on each toast. Default is <c>true</c>.
    /// </summary>
    [Parameter]
    public bool ShowCloseButton { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to pause the progress bar when the user hovers over a toast. Default is <c>true</c>.
    /// </summary>
    [Parameter]
    public bool PauseProgressOnHover { get; set; } = true;

    /// <summary>
    /// Gets or sets additional attributes to be applied to the toast container element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> Attributes { get; set; } = [];

    /// <summary>
    /// Gets the list of currently displayed toast instances.
    /// </summary>
    private List<ToastInstance> ToastList { get; set; } = [];

    /// <summary>
    /// Gets the queue of waiting toast instances when the maximum display count is reached.
    /// </summary>
    private Queue<ToastInstance> ToastWaitingQueue { get; set; } = new();

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();

        Toaster.OnShow += ShowToast;
        Toaster.OnClear += ClearToast;

        if (ClearOnNavigation)
            NavigationManager.LocationChanged += ClearToasts;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Toaster.OnShow -= ShowToast;
        Toaster.OnClear -= ClearToast;

        if (ClearOnNavigation)
            NavigationManager.LocationChanged -= ClearToasts;
    }

    /// <summary>
    /// Removes a toast notification by its unique identifier.
    /// </summary>
    /// <param name="toastId">The <see cref="Guid"/> of the toast to remove.</param>
    public void RemoveToast(Guid toastId)
    {
        InvokeAsync(() =>
        {
            var toastInstance = ToastList.Find(x => x.Id == toastId);

            if (toastInstance is not null)
            {
                ToastList.Remove(toastInstance);
                StateHasChanged();
            }

            if (ToastWaitingQueue.Count > 0)
                ShowEnqueuedToast();
        });
    }

    /// <summary>
    /// Gets the CSS class string for the toast container based on its position.
    /// </summary>
    /// <param name="position">The <see cref="ToastPosition"/> to use.</param>
    /// <returns>The CSS class string for the container.</returns>
    private string ContainerClass(ToastPosition? position)
    {
        var className = position switch
        {
            ToastPosition.TopLeft => "toaster-top-left",
            ToastPosition.TopRight => "toaster-top-right",
            ToastPosition.TopCenter => "toaster-top-center",
            ToastPosition.BottomLeft => "toaster-bottom-left",
            ToastPosition.BottomRight => "toaster-bottom-right",
            ToastPosition.BottomCenter => "toaster-bottom-center",
            _ => "toaster-top-right"
        };

        return CssBuilder
            .Default("toaster")
            .AddClass(className)
            .MergeClass(Attributes)
            .ToString();
    }

    /// <summary>
    /// Handles the event to show a new toast notification.
    /// </summary>
    /// <param name="level">The <see cref="ToastLevel"/> of the toast.</param>
    /// <param name="message">The <see cref="RenderFragment"/> content of the toast.</param>
    /// <param name="configure">An optional action to configure the <see cref="ToastSettings"/>.</param>
    private void ShowToast(ToastLevel level, RenderFragment message, Action<ToastSettings>? configure)
    {
        InvokeAsync(() =>
        {
            var settings = new ToastSettings
            {
                ShowProgressBar = ShowProgressBar,
                ShowCloseButton = ShowCloseButton,
                PauseProgressOnHover = PauseProgressOnHover,
                Position = Position,
                Timeout = Timeout,
            };

            configure?.Invoke(settings);

            var toast = new ToastInstance(message, level, settings);

            if (ToastList.Count < MaxToast)
            {
                ToastList.Add(toast);

                StateHasChanged();
            }
            else
            {
                ToastWaitingQueue.Enqueue(toast);
            }
        });
    }

    /// <summary>
    /// Displays the next toast in the waiting queue if space is available.
    /// </summary>
    private void ShowEnqueuedToast()
    {
        InvokeAsync(() =>
        {
            var toast = ToastWaitingQueue.Dequeue();

            ToastList.Add(toast);

            StateHasChanged();
        });
    }

    /// <summary>
    /// Clears all toasts when a navigation event occurs.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="args">The <see cref="LocationChangedEventArgs"/> for the navigation event.</param>
    private void ClearToasts(object? sender, LocationChangedEventArgs args)
    {
        InvokeAsync(() =>
        {
            ToastList.Clear();
            StateHasChanged();

            if (ToastWaitingQueue.Count > 0)
                ShowEnqueuedToast();
        });
    }

    /// <summary>
    /// Clears toast notifications, optionally filtered by toast level.
    /// </summary>
    /// <param name="toastLevel">The <see cref="ToastLevel"/> to clear, or <c>null</c> to clear all toasts.</param>
    private void ClearToast(ToastLevel? toastLevel = null)
    {
        InvokeAsync(() =>
        {
            if (toastLevel != null)
                ToastList.RemoveAll(x => x.Level == toastLevel);
            else
                ToastList.Clear();

            ToastWaitingQueue.Clear();

            StateHasChanged();
        });
    }
}
