using System.ComponentModel.DataAnnotations;

namespace HELMoliday.Contracts;

public record RegisterRequest(
    string FirstName,
    string LastName,
    [EmailAddress] string Email,
    string Password);