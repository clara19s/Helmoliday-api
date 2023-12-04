namespace HELMoliday.Contracts.User;

public record UserInfoResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string ProfilePicture);