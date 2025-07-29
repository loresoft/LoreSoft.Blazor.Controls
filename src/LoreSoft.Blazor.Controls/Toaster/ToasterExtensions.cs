# nullable enable

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Provides extension methods for the <see cref="IToaster"/> service to simplify showing toast notifications in Blazor applications.
/// </summary>
public static class ToasterExtensions
{
    /// <summary>
    /// Shows an information toast notification with the specified text message.
    /// </summary>
    /// <param name="toaster">The <see cref="IToaster"/> service to display the toast.</param>
    /// <param name="message">The text to display in the toast notification.</param>
    /// <param name="settings">An optional action to configure the <see cref="ToastSettings"/> for this toast instance.</param>
    public static void ShowInformation(this IToaster toaster, string message, Action<ToastSettings>? settings = null)
        => toaster?.Show(ToastLevel.Information, builder => builder.AddContent(0, message), settings);

    /// <summary>
    /// Shows an information toast notification with the specified <see cref="RenderFragment"/> content.
    /// </summary>
    /// <param name="toaster">The <see cref="IToaster"/> service to display the toast.</param>
    /// <param name="message">The <see cref="RenderFragment"/> content to display in the toast notification.</param>
    /// <param name="settings">An optional action to configure the <see cref="ToastSettings"/> for this toast instance.</param>
    public static void ShowInformation(this IToaster toaster, RenderFragment message, Action<ToastSettings>? settings = null)
        => toaster?.Show(ToastLevel.Information, message, settings);

    /// <summary>
    /// Shows a success toast notification with the specified text message.
    /// </summary>
    /// <param name="toaster">The <see cref="IToaster"/> service to display the toast.</param>
    /// <param name="message">The text to display in the toast notification.</param>
    /// <param name="settings">An optional action to configure the <see cref="ToastSettings"/> for this toast instance.</param>
    public static void ShowSuccess(this IToaster toaster, string message, Action<ToastSettings>? settings = null)
        => toaster?.Show(ToastLevel.Success, builder => builder.AddContent(0, message), settings);

    /// <summary>
    /// Shows a success toast notification with the specified <see cref="RenderFragment"/> content.
    /// </summary>
    /// <param name="toaster">The <see cref="IToaster"/> service to display the toast.</param>
    /// <param name="message">The <see cref="RenderFragment"/> content to display in the toast notification.</param>
    /// <param name="settings">An optional action to configure the <see cref="ToastSettings"/> for this toast instance.</param>
    public static void ShowSuccess(this IToaster toaster, RenderFragment message, Action<ToastSettings>? settings = null)
        => toaster?.Show(ToastLevel.Success, message, settings);

    /// <summary>
    /// Shows a warning toast notification with the specified text message.
    /// </summary>
    /// <param name="toaster">The <see cref="IToaster"/> service to display the toast.</param>
    /// <param name="message">The text to display in the toast notification.</param>
    /// <param name="settings">An optional action to configure the <see cref="ToastSettings"/> for this toast instance.</param>
    public static void ShowWarning(this IToaster toaster, string message, Action<ToastSettings>? settings = null)
        => toaster?.Show(ToastLevel.Warning, builder => builder.AddContent(0, message), settings);

    /// <summary>
    /// Shows a warning toast notification with the specified <see cref="RenderFragment"/> content.
    /// </summary>
    /// <param name="toaster">The <see cref="IToaster"/> service to display the toast.</param>
    /// <param name="message">The <see cref="RenderFragment"/> content to display in the toast notification.</param>
    /// <param name="settings">An optional action to configure the <see cref="ToastSettings"/> for this toast instance.</param>
    public static void ShowWarning(this IToaster toaster, RenderFragment message, Action<ToastSettings>? settings = null)
        => toaster?.Show(ToastLevel.Warning, message, settings);

    /// <summary>
    /// Shows an error toast notification with the specified text message.
    /// </summary>
    /// <param name="toaster">The <see cref="IToaster"/> service to display the toast.</param>
    /// <param name="message">The text to display in the toast notification.</param>
    /// <param name="settings">An optional action to configure the <see cref="ToastSettings"/> for this toast instance.</param>
    public static void ShowError(this IToaster toaster, string message, Action<ToastSettings>? settings = null)
        => toaster?.Show(ToastLevel.Error, builder => builder.AddContent(0, message), settings);

    /// <summary>
    /// Shows an error toast notification with the specified <see cref="RenderFragment"/> content.
    /// </summary>
    /// <param name="toaster">The <see cref="IToaster"/> service to display the toast.</param>
    /// <param name="message">The <see cref="RenderFragment"/> content to display in the toast notification.</param>
    /// <param name="settings">An optional action to configure the <see cref="ToastSettings"/> for this toast instance.</param>
    public static void ShowError(this IToaster toaster, RenderFragment message, Action<ToastSettings>? settings = null)
        => toaster?.Show(ToastLevel.Error, message, settings);

    /// <summary>
    /// Shows a toast notification with the specified level and text message.
    /// </summary>
    /// <param name="toaster">The <see cref="IToaster"/> service to display the toast.</param>
    /// <param name="level">The <see cref="ToastLevel"/> to display (e.g., Information, Success, Warning, Error).</param>
    /// <param name="message">The text to display in the toast notification.</param>
    /// <param name="settings">An optional action to configure the <see cref="ToastSettings"/> for this toast instance.</param>
    public static void Show(this IToaster toaster, ToastLevel level, string message, Action<ToastSettings>? settings = null)
        => toaster?.Show(level, builder => builder.AddContent(0, message), settings);
}
