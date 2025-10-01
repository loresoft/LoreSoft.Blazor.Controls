using System;
using System.ComponentModel.DataAnnotations;

namespace Sample.Core.Models;

public class StarShip
{
    [Required]
    [StringLength(16, ErrorMessage = "Identifier too long (16 character limit).")]
    public string Identifier { get; set; } = null!;

    public string? Description { get; set; }

    [Required]
    public string Classification { get; set; } = null!;

    [Range(1, 100000, ErrorMessage = "Accommodation invalid (1-100000).")]
    public int MaximumAccommodation { get; set; }

    [Required]
    [Range(typeof(bool), "true", "true",
        ErrorMessage = "This form disallows unapproved ships.")]
    public bool IsValidatedDesign { get; set; }

    [Required]
    public DateTime ProductionDate { get; set; }

    [Required]
    public Person Captain { get; set; } = new();
}
