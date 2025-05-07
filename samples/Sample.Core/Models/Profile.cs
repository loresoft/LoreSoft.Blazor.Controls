using System.ComponentModel.DataAnnotations;

namespace Sample.Core.Models;

public class Profile
{
    [Required]
    [Display(Name = "Display Name")]
    public string DisplayName { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string EmailAddress { get; set; }

    [Required]
    public string ProfileImage { get; set; }
}
