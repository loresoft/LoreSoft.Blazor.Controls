#nullable enable

using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace LoreSoft.Blazor.Controls;

public partial class ToastContainer: IDisposable
{
    [Inject]
    protected IToaster Toaster { get; set; } = default!;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    [Parameter]
    public ToastPosition Position { get; set; } = ToastPosition.TopRight;

    [Parameter]
    public int Timeout { get; set; } = 5;

    [Parameter]
    public int MaxToast { get; set; } = 10;

    [Parameter]
    public bool ClearOnNavigation { get; set; }

    [Parameter]
    public bool ShowProgressBar { get; set; } = true;

    [Parameter]
    public bool ShowCloseButton { get; set; } = true;

    [Parameter]
    public bool PauseProgressOnHover { get; set; } = true;

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> Attributes { get; set; } = [];


    private List<ToastInstance> ToastList { get; set; } = [];

    private Queue<ToastInstance> ToastWaitingQueue { get; set; } = new();


    protected override void OnInitialized()
    {
        base.OnInitialized();

        Toaster.OnShow += ShowToast;
        Toaster.OnClear += ClearToast;

        if (ClearOnNavigation)
            NavigationManager.LocationChanged += ClearToasts;
    }

    public void Dispose()
    {
        Toaster.OnShow -= ShowToast;
        Toaster.OnClear -= ClearToast;

        if (ClearOnNavigation)
            NavigationManager.LocationChanged -= ClearToasts;
    }

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
            .Default("toaster-container")
            .AddClass(className)
            .MergeClass(Attributes)
            .ToString();
    }

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

    private void ShowEnqueuedToast()
    {
        InvokeAsync(() =>
        {
            var toast = ToastWaitingQueue.Dequeue();

            ToastList.Add(toast);

            StateHasChanged();
        });
    }

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
