# nullable enable

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents configuration settings for an individual toast notification in applications.
/// Allows customization of appearance, behavior, timeout, and position.
/// </summary>
public class ToastSettings
{
    /// <summary>
    /// Gets or sets additional CSS classes to apply to the toast component.
    /// </summary>
    /// <remarks>
    /// Use this property to customize the appearance of the toast notification by specifying one or more CSS class names separated by spaces.
    /// </remarks>
    public string? ClassName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show a progress bar on the toast.
    /// Provides visual feedback on the remaining display time.
    /// </summary>
    public bool? ShowProgressBar { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the toast's timeout should pause when the user hovers over it.
    /// </summary>
    /// <remarks>
    /// Useful for giving users more time to read the notification. Can be combined with extended timeout logic for improved usability.
    /// </remarks>
    public bool? PauseProgressOnHover { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show a close button on the toast notification.
    /// </summary>
    public bool? ShowCloseButton { get; set; }

    /// <summary>
    /// Gets or sets an optional action to invoke when the user clicks the toast notification.
    /// </summary>
    /// <remarks>
    /// Allows you to define custom behavior, such as navigation or other actions, when the toast is clicked.
    /// </remarks>
    public Action? OnClick { get; set; }

    /// <summary>
    /// Gets or sets the duration, in seconds, that the toast notification will be displayed before automatically closing.
    /// </summary>
    /// <remarks>
    /// Controls how long the notification remains visible to the user.
    /// </remarks>
    public int Timeout { get; set; }

    /// <summary>
    /// Gets or sets the position of the toast notification on the screen.
    /// Overrides the global toast position if set.
    /// </summary>
    public ToastPosition? Position { get; set; }
}
