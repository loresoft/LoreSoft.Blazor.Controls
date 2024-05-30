# nullable enable

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

internal class Toaster : IToaster
{
    public event Action<ToastLevel, RenderFragment, Action<ToastSettings>?>? OnShow;

    public event Action<ToastLevel?>? OnClear;


    public void Show(ToastLevel level, RenderFragment message, Action<ToastSettings>? settings = null)
    {
        OnShow?.Invoke(level, message, settings);
    }

    public void Clear(ToastLevel? toastLevel = null)
    {
        OnClear?.Invoke(toastLevel);
    }
}
