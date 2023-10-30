using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace HELMoliday.Models;

[Owned]
public class Address
{
    [Required(ErrorMessage = "The {0} field is required.")]
    [StringLength(255)]
    public string Street { get; set; }

    [Required(ErrorMessage = "The {0} field is required.")]
    [Range(0, int.MaxValue)]
    [Display(Name = "Street number")]
    public string StreetNumber { get; set; }

    [Required(ErrorMessage = "The {0} field is required.")]
    [StringLength(10, MinimumLength = 4,
        ErrorMessage = "The {0} field must be at least {2} characters long and at most {1} characters long.")]
    [Display(Name = "Postal code")]
    public string PostalCode { get; set; }

    [Required(ErrorMessage = "The {0} field is required.")]
    [StringLength(100, MinimumLength = 5,
        ErrorMessage = "The {0} field must be at least {2} characters long and at most {1} characters long.")]
    [Display(Name = "City")]
    public string City { get; set; }

    [Required(ErrorMessage = "The {0} field is required.")]
    [StringLength(100, MinimumLength = 5,
        ErrorMessage = "The {0} field must be at least {2} characters long and at most {1} characters long.")]
    public string Country { get; set; }

    public override string? ToString()
    {
        return $"{StreetNumber} {Street}, {PostalCode} {City.ToUpper()} ({Country.ToUpper()})";
    }

    
}