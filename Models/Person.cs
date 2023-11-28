using Microsoft.EntityFrameworkCore;

namespace HELMoliday.Models
{
    [Owned]
    public class Person
    {
        public string? FirstName { get; set; }
       
        public string? LastName { get; set; } 
        public string? phoneNumber { get; set; }

    }
}
