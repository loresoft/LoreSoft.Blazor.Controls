using System.ComponentModel.DataAnnotations;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Specifies the default image mode to use for Gravatar when an email address has no associated image.
/// </summary>
public enum GravatarMode
{
    /// <summary>
    /// Returns a 404 error if no image is found for the email address.
    /// </summary>
    [Display(Name = "404")]
    NotFound,

    /// <summary>
    /// Displays the "mystery man" default Gravatar image.
    /// </summary>
    [Display(Name = "Mm")]
    Mm,

    /// <summary>
    /// Displays a geometric pattern based on a hash of the email address.
    /// </summary>
    [Display(Name = "Identicon")]
    Identicon,

    /// <summary>
    /// Displays a generated "monster" image based on a hash of the email address.
    /// </summary>
    [Display(Name = "Monsterid")]
    Monsterid,

    /// <summary>
    /// Displays a generated "wavatar" image based on a hash of the email address.
    /// </summary>
    [Display(Name = "Wavatar")]
    Wavatar,

    /// <summary>
    /// Displays a generated "retro" 8-bit style image based on a hash of the email address.
    /// </summary>
    [Display(Name = "Retro")]
    Retro,

    /// <summary>
    /// Displays a blank image if no Gravatar is found.
    /// </summary>
    [Display(Name = "Blank")]
    Blank
}
