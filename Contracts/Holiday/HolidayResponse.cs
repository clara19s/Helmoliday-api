using HELMoliday.Contracts.Common;

namespace HELMoliday.Contracts.Holiday
{
    public record HolidayResponse(
        Guid Id,
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
