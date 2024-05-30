# nullable enable

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

internal class ToastInstance
{
    public ToastInstance(RenderFragment message, ToastLevel level, ToastSettings toastSettings)
    {
        Message = message;
        Level = level;
        ToastSettings = toastSettings;
    }

    public Guid Id { get; } = Guid.NewGuid();

    public DateTime TimeStamp { get; } = DateTime.Now;

    public RenderFragment? Message { get; set; }

    public ToastLevel Level { get; }

    public ToastSettings ToastSettings { get; }
}
