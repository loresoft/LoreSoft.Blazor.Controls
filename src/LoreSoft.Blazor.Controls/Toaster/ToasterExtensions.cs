# nullable enable

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// <see cref="IToaster"/> extension methods
/// </summary>
public static class ToasterExtensions
{
    /// <summary>
    /// Shows a information toast
    /// </summary>
    /// <param name="toaster">The toaster to send toast with</param>
    /// <param name="message">Text to display on the toast</param>
    /// <param name="settings">Settings to configure the toast instance</param>
    public static void ShowInformation(this IToaster toaster, string message, Action<ToastSettings>? settings = null)
        => toaster?.Show(ToastLevel.Information, builder => builder.AddContent(0, message), settings);

    /// <summary>
    /// Shows a information toast
    /// </summary>
    /// <param name="toaster">The toaster to send toast with</param>
    /// <param name="message">RenderFragment to display on the toast</param>
    /// <param name="settings">Settings to configure the toast instance</param>
    public static void ShowInformation(this IToaster toaster, RenderFragment message, Action<ToastSettings>? settings = null)
        => toaster?.Show(ToastLevel.Information, message, settings);

    /// <summary>
    /// Shows a success toast
    /// </summary>
    /// <param name="toaster">The toaster to send toast with</param>
    /// <param name="message">Text to display on the toast</param>
    /// <param name="settings">Settings to configure the toast instance</param>
    public static void ShowSuccess(this IToaster toaster, string message, Action<ToastSettings>? settings = null)
        => toaster?.Show(ToastLevel.Success, builder => builder.AddContent(0, message), settings);

    /// <summary>
    /// Shows a success toast
    /// </summary>
    /// <param name="toaster">The toaster to send toast with</param>
    /// <param name="message">RenderFragment to display on the toast</param>
    /// <param name="settings">Settings to configure the toast instance</param>
    public static void ShowSuccess(this IToaster toaster, RenderFragment message, Action<ToastSettings>? settings = null)
        => toaster?.Show(ToastLevel.Success, message, settings);

    /// <summary>
    /// Shows a warning toast
    /// </summary>
    /// <param name="toaster">The toaster to send toast with</param>
    /// <param name="message">Text to display on the toast</param>
    /// <param name="settings">Settings to configure the toast instance</param>
    public static void ShowWarning(this IToaster toaster, string message, Action<ToastSettings>? settings = null)
        => toaster?.Show(ToastLevel.Warning, builder => builder.AddContent(0, message), settings);

    /// <summary>
    /// Shows a warning toast
    /// </summary>
    /// <param name="toaster">The toaster to send toast with</param>
    /// <param name="message">RenderFragment to display on the toast</param>
    /// <param name="settings">Settings to configure the toast instance</param>
    public static void ShowWarning(this IToaster toaster, RenderFragment message, Action<ToastSettings>? settings = null)
        => toaster?.Show(ToastLevel.Warning, message, settings);

    /// <summary>
    /// Shows a error toast
    /// </summary>
    /// <param name="toaster">The toaster to send toast with</param>
    /// <param name="message">Text to display on the toast</param>
    /// <param name="settings">Settings to configure the toast instance</param>
    public static void ShowError(this IToaster toaster, string message, Action<ToastSettings>? settings = null)
        => toaster?.Show(ToastLevel.Error, builder => builder.AddContent(0, message), settings);

    /// <summary>
    /// Shows a error toast
    /// </summary>
    /// <param name="toaster">The toaster to send toast with</param>
    /// <param name="message">RenderFragment to display on the toast</param>
    /// <param name="settings">Settings to configure the toast instance</param>
    public static void ShowError(this IToaster toaster, RenderFragment message, Action<ToastSettings>? settings = null)
        => toaster?.Show(ToastLevel.Error, message, settings);

    /// <summary>
    /// Shows a toast using the supplied settings
    /// </summary>
    /// <param name="toaster">The toaster to send toast with</param>
    /// <param name="level">Toast level to display</param>
    /// <param name="message">Text to display on the toast</param>
    /// <param name="settings">Settings to configure the toast instance</param>
    public static void Show(this IToaster toaster, ToastLevel level, string message, Action<ToastSettings>? settings = null)
        => toaster?.Show(level, builder => builder.AddContent(0, message), settings);
}
