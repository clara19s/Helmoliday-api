namespace HELMoliday.Contracts.Authentication;
public record RefreshToken(
    string Token,
    DateTime Created,
    DateTime Expires);
