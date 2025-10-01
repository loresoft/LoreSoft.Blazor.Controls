using System.ComponentModel.DataAnnotations;

namespace Sample.Core.Models.GitHub;

public class NameSearch
{
    [Required]
    [Display(Name = "Name")]
    public string Name { get; set; } = null!;

}
