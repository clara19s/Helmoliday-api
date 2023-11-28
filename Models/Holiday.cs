using HELMoliday.Services.Cal;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HELMoliday.Models;

public partial class Holiday : IEvent
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(150, MinimumLength = 1)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public DateTimeOffset StartDate { get; set; }

    [Required]
    public DateTimeOffset EndDate { get; set; }

    [Required]
    public Address Address { get; set; }

    public bool Published { get; set; } = false;

    public virtual ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();

    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();

    public Person ContactPerson { get; set; }
}
