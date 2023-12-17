using HELMoliday.Models;
using System.ComponentModel.DataAnnotations;

namespace HELMoliday.Contracts.Common
{
    public record AddressDto(
        [Required] string Street, 
        [Required] string StreetNumber,
        [Required] string PostalCode,
        [Required] string City,
        [Required] string Country
     );

    public class AddressConverter
    {
        public static Address CreateFromDto(AddressDto addressDto) => new()
        {
            Street = addressDto.Street,
            StreetNumber = addressDto.StreetNumber,
            City = addressDto.City,
            Country = addressDto.Country,
            PostalCode = addressDto.PostalCode,
        };

        public static AddressDto CreateFromModel(Address model) => new(
             model.Street,
             model.StreetNumber.ToString(),
             model.PostalCode,
             model.City,
             model.Country
        );
    }
}
