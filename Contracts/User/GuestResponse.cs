namespace HELMoliday.Contracts.User;
public record GuestResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string ProfilePicture);