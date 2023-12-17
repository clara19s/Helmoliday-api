using HELMoliday.Contracts.Common;
using HELMoliday.Validations;
using System.ComponentModel.DataAnnotations;

namespace HELMoliday.Contracts.Holiday;
public record HolidayRequest(
    [Required][StringLength(150, MinimumLength = 1)] string Name,
    string? Description,
    [Required] string StartDate,
    [Required] [DateValidation("StartDate")] string EndDate,
    AddressDto Address,
    bool Published);

public record HolidayFilter(
    string? Query,
    string? StartDate,
    string? EndDate);
