using HELMoliday.Contracts.Common;

namespace HELMoliday.Contracts.Activity;
public record UpsertActivityRequest(
    string Name,
    string Description,
    string StartDate,
    string EndDate,
    AddressDto Address,
    string Category);
