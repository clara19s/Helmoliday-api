using System.ComponentModel.DataAnnotations;

namespace HELMoliday.Contracts;

public record LoginRequest(
    [EmailAddress] string Email,
    string Password);