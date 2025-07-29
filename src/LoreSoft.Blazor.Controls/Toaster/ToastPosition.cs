# nullable enable

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Specifies the position of toast notifications on the screen in applications.
/// Used to control where toast messages appear within the viewport.
/// </summary>
public enum ToastPosition
{
    /// <summary>
    /// Displays toast notifications in the top-left corner of the screen.
    /// </summary>
    TopLeft,

    /// <summary>
    /// Displays toast notifications in the top-right corner of the screen.
    /// </summary>
    TopRight,

    /// <summary>
    /// Displays toast notifications at the top-center of the screen.
    /// </summary>
    TopCenter,

    /// <summary>
    /// Displays toast notifications in the bottom-left corner of the screen.
    /// </summary>
    BottomLeft,

    /// <summary>
    /// Displays toast notifications in the bottom-right corner of the screen.
    /// </summary>
    BottomRight,

    /// <summary>
    /// Displays toast notifications at the bottom-center of the screen.
    /// </summary>
    BottomCenter
}
