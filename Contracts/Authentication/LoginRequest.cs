using System.ComponentModel.DataAnnotations;

namespace HELMoliday.Contracts.Authentication;

public record LoginRequest(
    [EmailAddress] string Email,
    string Password);