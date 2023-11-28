using HELMoliday.Services.OAuth.Strategies;

namespace HELMoliday.Services.OAuth;
public class OAuthStrategyFactory
{
    private readonly IServiceProvider _serviceProvider;

    public OAuthStrategyFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IOAuthStrategy GetStrategy(string platform)
    {
        return platform.ToLower() switch
        {
            "linkedin" => _serviceProvider.GetRequiredService<LinkedInOAuthStrategy>(),
            "google" => _serviceProvider.GetRequiredService<GoogleOAuthStrategy>(),
            "facebook" => _serviceProvider.GetRequiredService<FacebookOAuthStrategy>(),
            _ => throw new ArgumentException($"Unsupported platform: {platform}"),
        };
    }
}