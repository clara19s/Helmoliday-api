namespace HELMoliday.Options;
public sealed class JwtSettings
{
    public static string SectionName { get; } = "JwtSettings";
    public string Secret { get; init; }
    public int ExpiryMinutes { get; init; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
}
