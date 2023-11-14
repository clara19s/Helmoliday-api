using System.ComponentModel.DataAnnotations;

namespace HELMoliday.Models;

public class Activity
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public DateTimeOffset StartDate { get; set; }

    [Required]
    public DateTimeOffset EndDate { get; set; }

    [Required]
    public Address Address { get; set; }

    public Holiday Holiday { get; set; }

    public virtual Guid HolidayId { get; set; }
}