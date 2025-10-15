namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Specifies the visual variant or theme for a modal dialog.
/// </summary>
/// <remarks>
/// The modal variant determines the visual styling and semantic meaning of the modal dialog,
/// typically mapped to CSS classes that apply appropriate colors and styling.
/// </remarks>
public enum ModalVariant
{
    /// <summary>
    /// Represents the primary or default modal variant.
    /// </summary>
    Primary,

    /// <summary>
    /// Represents an informational modal variant, typically styled in blue.
    /// </summary>
    Information,

    /// <summary>
    /// Represents a success modal variant, typically styled in green.
    /// </summary>
    Success,

    /// <summary>
    /// Represents a warning modal variant, typically styled in yellow or orange.
    /// </summary>
    Warning,

    /// <summary>
    /// Represents a danger or error modal variant, typically styled in red.
    /// </summary>
    Danger
}
