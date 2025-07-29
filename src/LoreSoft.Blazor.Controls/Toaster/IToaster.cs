# nullable enable

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Defines the contract for a toaster service that displays toast notifications.
/// Provides events and methods for showing and clearing toasts with customizable settings.
/// </summary>
public interface IToaster
{
    /// <summary>
    /// Occurs when a toast notification is shown.
    /// </summary>
    /// <remarks>
    /// The event provides the toast level, content to display, and an optional action to configure toast settings.
    /// </remarks>
    event Action<ToastLevel, RenderFragment, Action<ToastSettings>?> OnShow;

    /// <summary>
    /// Occurs when all toasts are cleared.
    /// </summary>
    /// <remarks>
    /// The event may specify a toast level to clear only toasts of that level.
    /// </remarks>
    event Action<ToastLevel?>? OnClear;

    /// <summary>
    /// Shows a toast notification using the supplied settings.
    /// </summary>
    /// <param name="level">The <see cref="ToastLevel"/> to display (e.g., Information, Success, Warning, Error).</param>
    /// <param name="message">The <see cref="RenderFragment"/> content to display in the toast.</param>
    /// <param name="settings">An optional action to configure the <see cref="ToastSettings"/> for this toast instance.</param>
    void Show(ToastLevel level, RenderFragment message, Action<ToastSettings>? settings = null);

    /// <summary>
    /// Removes all toast notifications, optionally filtered by toast level.
    /// </summary>
    /// <param name="toastLevel">The <see cref="ToastLevel"/> to clear, or <c>null</c> to clear all toasts.</param>
    void Clear(ToastLevel? toastLevel = null);
}
