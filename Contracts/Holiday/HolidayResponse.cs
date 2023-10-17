using HELMoliday.Contracts.Common;

namespace HELMoliday.Contracts.Holiday
{
    public record HolidayResponse(
        string Name,
        string Description,
        string StartDate,
        string EndDate, 
        AddressDto Address, 
        bool published,
        IEnumerable<string> guests, 
        IEnumerable<string> activities
        );
}
