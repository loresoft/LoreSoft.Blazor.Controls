namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Defines standard responsive breakpoint sizes used for responsive design in web applications.
/// These breakpoints correspond to common device viewport widths and follow Bootstrap conventions.
/// </summary>
/// <remarks>
/// The enum values represent the priority order of breakpoints, with smaller breakpoints having lower values.
/// This ordering is useful for comparisons and determining which breakpoint takes precedence.
///
/// Default breakpoint pixel values:
/// <list type="bullet">
/// <item><description>xs: 0px - Extra small devices (portrait phones)</description></item>
/// <item><description>sm: 576px - Small devices (landscape phones)</description></item>
/// <item><description>md: 768px - Medium devices (tablets)</description></item>
/// <item><description>lg: 992px - Large devices (desktops)</description></item>
/// <item><description>xl: 1200px - Extra large devices (large desktops)</description></item>
/// <item><description>xxl: 1400px - Extra extra large devices (larger desktops)</description></item>
/// </list>
/// </remarks>
public enum Breakpoints
{
    /// <summary>
    /// Extra small breakpoint for devices with viewport width of 0px and up.
    /// Typically represents portrait phones and the smallest mobile devices.
    /// </summary>
    xs = 0,

    /// <summary>
    /// Small breakpoint for devices with viewport width of 576px and up.
    /// Typically represents landscape phones and small tablets.
    /// </summary>
    sm = 1,

    /// <summary>
    /// Medium breakpoint for devices with viewport width of 768px and up.
    /// Typically represents tablets in portrait mode and small laptops.
    /// </summary>
    md = 2,

    /// <summary>
    /// Large breakpoint for devices with viewport width of 992px and up.
    /// Typically represents desktop computers and laptops.
    /// </summary>
    lg = 3,

    /// <summary>
    /// Extra large breakpoint for devices with viewport width of 1200px and up.
    /// Typically represents large desktop monitors and wide screens.
    /// </summary>
    xl = 4,

    /// <summary>
    /// Extra extra large breakpoint for devices with viewport width of 1400px and up.
    /// Typically represents very large desktop monitors and ultra-wide displays.
    /// </summary>
    xxl = 5
}
