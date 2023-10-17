using HELMoliday.Models;

namespace HELMoliday.Contracts.Common
{
    public record AddressDto(
        string Street, 
        string StreetNumber,
        string PostalCode,
        string City,
        string Country
     );

    public class AddressConverter
    {
        public static Address CreateFromDto(AddressDto addressDto) => new Address
        {
            Street = addressDto.Street,
            StreetNumber = int.Parse(addressDto.StreetNumber),
            City = addressDto.City,
            Country = addressDto.Country,
            PostalCode = addressDto.PostalCode,
        };

        public static AddressDto CreateFromModel(Address model) => new AddressDto(
             model.Street,
             model.StreetNumber.ToString(),
             model.City,
             model.Country,
             model.PostalCode
        );
    }
}
