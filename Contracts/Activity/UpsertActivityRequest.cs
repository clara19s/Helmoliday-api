using HELMoliday.Contracts.Common;
using HELMoliday.Validations;

namespace HELMoliday.Contracts.Activity;
public record UpsertActivityRequest(
    string Name,
    string Description,
    string StartDate,
    [DateValidation("StartDate")] string EndDate,
    AddressDto Address,
    string Category);
