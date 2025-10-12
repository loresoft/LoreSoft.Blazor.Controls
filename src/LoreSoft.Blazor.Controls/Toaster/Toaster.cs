using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Internal implementation of the <see cref="IToaster"/> service for toast notifications.
/// Manages events for showing and clearing toasts, and provides methods to trigger these events.
/// </summary>
internal class Toaster : IToaster
{
    /// <summary>
    /// Occurs when a toast notification is shown.
    /// </summary>
    public event Action<ToastLevel, RenderFragment, Action<ToastSettings>?>? OnShow;

    /// <summary>
    /// Occurs when all toasts are cleared.
    /// </summary>
    public event Action<ToastLevel?>? OnClear;

    /// <summary>
    /// Shows a toast notification using the supplied settings.
    /// Triggers the <see cref="OnShow"/> event.
    /// </summary>
    /// <param name="level">The <see cref="ToastLevel"/> to display (e.g., Information, Success, Warning, Error).</param>
    /// <param name="message">The <see cref="RenderFragment"/> content to display in the toast.</param>
    /// <param name="settings">An optional action to configure the <see cref="ToastSettings"/> for this toast instance.</param>
    public void Show(ToastLevel level, RenderFragment message, Action<ToastSettings>? settings = null)
    {
        OnShow?.Invoke(level, message, settings);
    }

    /// <summary>
    /// Removes all toast notifications, optionally filtered by toast level.
    /// Triggers the <see cref="OnClear"/> event.
    /// </summary>
    /// <param name="toastLevel">The <see cref="ToastLevel"/> to clear, or <c>null</c> to clear all toasts.</param>
    public void Clear(ToastLevel? toastLevel = null)
    {
        OnClear?.Invoke(toastLevel);
    }
}
