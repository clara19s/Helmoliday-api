﻿using System.ComponentModel.DataAnnotations;

namespace HELMoliday.Models;

public partial class Holiday
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

    public virtual ICollection<Invitation> Inviters { get; set; } = new List<Invitation>();

    public virtual ICollection<Unfolding> Unfoldings { get; set; } = new List<Unfolding>();
}