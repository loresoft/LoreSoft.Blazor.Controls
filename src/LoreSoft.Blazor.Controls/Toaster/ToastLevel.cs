# nullable enable

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Specifies the severity or type of a toast notification in applications.
/// Used to determine the visual style and intent of the toast message.
/// </summary>
public enum ToastLevel
{
    /// <summary>
    /// Represents an informational toast notification.
    /// </summary>
    Information,

    /// <summary>
    /// Represents a success toast notification.
    /// </summary>
    Success,

    /// <summary>
    /// Represents a warning toast notification.
    /// </summary>
    Warning,

    /// <summary>
    /// Represents an error toast notification.
    /// </summary>
    Error
}
