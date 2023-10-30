using HELMoliday.Contracts.Common;
using HELMoliday.Contracts.User;

namespace HELMoliday.Contracts.Holiday;
public record HolidayResponse(
    Guid Id,
    string Name,
    string Description,
    string StartDate,
    string EndDate,
    AddressDto Address,
    bool Published,
    IEnumerable<GuestResponse> Guests,
    IEnumerable<string> Activities);
