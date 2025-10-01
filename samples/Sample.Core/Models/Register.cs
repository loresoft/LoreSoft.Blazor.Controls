using System;
using System.ComponentModel.DataAnnotations;

namespace Sample.Core.Models;

public class Register
{
    [Required]
    [Display(Name = "Display Name")]
    public string DisplayName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string EmailAddress { get; set; } = null!;

    [Required]
    [Display(Name = "Manager")]
    public Person Manager { get; set; } = null!;

    [Required]
    [Range(typeof(bool), "true", "true",
        ErrorMessage = "Must agree to terms to register.")]
    [Display(Name = "Agree to Terms")]
    public bool Agree { get; set; }

    [Required]
    public DateTime? Birthday { get; set; }
}
