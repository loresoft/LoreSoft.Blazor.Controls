# nullable enable

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents an individual toast notification instance in a application.
/// Stores the message, level, settings, and metadata for display and management.
/// </summary>
internal class ToastInstance
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ToastInstance"/> class with the specified message, level, and settings.
    /// </summary>
    /// <param name="message">The <see cref="RenderFragment"/> content to display in the toast.</param>
    /// <param name="level">The <see cref="ToastLevel"/> of the toast (e.g., Information, Success, Warning, Error).</param>
    /// <param name="toastSettings">The <see cref="ToastSettings"/> for configuring the toast's appearance and behavior.</param>
    public ToastInstance(RenderFragment message, ToastLevel level, ToastSettings toastSettings)
    {
        Message = message;
        Level = level;
        ToastSettings = toastSettings;
    }

    /// <summary>
    /// Gets the unique identifier for this toast instance.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the timestamp when this toast instance was created.
    /// </summary>
    public DateTime TimeStamp { get; } = DateTime.Now;

    /// <summary>
    /// Gets or sets the content to display in the toast notification.
    /// </summary>
    public RenderFragment? Message { get; set; }

    /// <summary>
    /// Gets the level of the toast notification (e.g., Information, Success, Warning, Error).
    /// </summary>
    public ToastLevel Level { get; }

    /// <summary>
    /// Gets the settings for this toast notification, such as timeout, position, and appearance.
    /// </summary>
    public ToastSettings ToastSettings { get; }
}
