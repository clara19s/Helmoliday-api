﻿using System.ComponentModel.DataAnnotations;

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
    public Address Address { get; set; }

    public virtual ICollection<Unfolding> Unfoldings { get; set; } = new List<Unfolding>();
}