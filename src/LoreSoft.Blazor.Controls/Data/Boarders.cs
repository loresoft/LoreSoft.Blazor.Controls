namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Specifies which borders should be displayed on UI components such as tables, grids, and containers.
/// This enumeration allows for flexible border styling by controlling which edges of an element have visible borders.
/// </summary>
public enum Borders
{
    /// <summary>
    /// No borders are displayed on any edge of the component.
    /// This option creates a clean, borderless appearance suitable for minimalist designs
    /// or when borders are handled through custom CSS styling.
    /// </summary>
    None = 0,

    /// <summary>
    /// Borders are displayed on all edges of the component (top, right, bottom, left).
    /// This is the most common border style, providing complete visual separation
    /// and definition of the component boundaries.
    /// </summary>
    All = 1,

    /// <summary>
    /// Borders are displayed only on the horizontal edges (top and bottom) of the component.
    /// This style is useful for creating row separators in tables or lists,
    /// providing visual separation between horizontal elements while maintaining
    /// a clean vertical flow.
    /// </summary>
    Horizontal = 2,

    /// <summary>
    /// Borders are displayed only on the vertical edges (left and right) of the component.
    /// This style is useful for creating column separators in tables or grids,
    /// providing visual separation between vertical elements while maintaining
    /// a clean horizontal flow.
    /// </summary>
    Vertical = 3,
}
