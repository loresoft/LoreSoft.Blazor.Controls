# nullable enable

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

public interface IToaster
{
    /// <summary>
    /// A event that will be invoked when showing a toast
    /// </summary>
    event Action<ToastLevel, RenderFragment, Action<ToastSettings>?> OnShow;

    /// <summary>
    /// A event that will be invoked to clear all toasts
    /// </summary>
    event Action<ToastLevel?>? OnClear;

    /// <summary>
    /// Shows a toast using the supplied settings
    /// </summary>
    /// <param name="level">Toast level to display</param>
    /// <param name="message">RenderFragment to display on the toast</param>
    /// <param name="settings">Settings to configure the toast instance</param>
    void Show(ToastLevel level, RenderFragment message, Action<ToastSettings>? settings = null);

    /// <summary>
    /// Removes all toasts
    /// </summary>
    void Clear(ToastLevel? toastLevel = null);
}
