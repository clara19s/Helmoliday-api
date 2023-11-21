using HELMoliday.Models;
using System.ComponentModel.DataAnnotations;

namespace HELMoliday.Services.Cal
{
    public interface IEvent
    {
        public string Name { get; set;} 

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTimeOffset StartDate { get; set; }

        [Required]
        public DateTimeOffset EndDate { get; set; }

        [Required]
        public Address Address { get; set; }

    }
}
