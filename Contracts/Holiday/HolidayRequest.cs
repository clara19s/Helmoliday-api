using HELMoliday.Contracts.Common;

namespace HELMoliday.Contracts.Holiday
{
    public record HolidayRequest
    (
       string Name,
       string? Description,
       string StartDate,
       string EndDate,
       AddressDto Address,
        bool published
     );

    public record HolidayFilter(
        string? query,
        string? StartDate,
       string? EndDate);
}
