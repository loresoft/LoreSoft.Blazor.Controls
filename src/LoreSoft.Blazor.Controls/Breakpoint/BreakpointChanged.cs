namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents data about a breakpoint change event, containing information about the current and previous breakpoints along with the viewport width.
/// This class is used to pass breakpoint change information from JavaScript to .NET components.
/// </summary>
public class BreakpointChanged
{
    /// <summary>
    /// Gets or sets the current breakpoint name based on the viewport width.
    /// This represents the active breakpoint after the change occurred.
    /// Default value is "xs".
    /// </summary>
    /// <value>
    /// A string representing the current breakpoint name (e.g., "xs", "sm", "md", "lg", "xl", "xxl").
    /// </value>
    public string Current { get; set; } = "xs";

    /// <summary>
    /// Gets or sets the previous breakpoint name before the change occurred.
    /// This value will be null if this is the initial breakpoint detection.
    /// </summary>
    /// <value>
    /// A string representing the previous breakpoint name, or null if no previous breakpoint exists.
    /// </value>
    public string? Previous { get; set; }

    /// <summary>
    /// Gets or sets the current viewport width in pixels that triggered the breakpoint change.
    /// </summary>
    /// <value>
    /// An integer representing the viewport width in pixels.
    /// </value>
    public int? Width { get; set; }
}
