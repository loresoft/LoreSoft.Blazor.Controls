using System.ComponentModel.DataAnnotations;

namespace LoreSoft.Blazor.Controls;

public enum GravatarMode
{
    [Display(Name = "404")]
    NotFound,
    [Display(Name = "Mm")]
    Mm,
    [Display(Name = "Identicon")]
    Identicon,
    [Display(Name = "Monsterid")]
    Monsterid,
    [Display(Name = "Wavatar")]
    Wavatar,
    [Display(Name = "Retro")]
    Retro,
    [Display(Name = "Blank")]
    Blank
}
