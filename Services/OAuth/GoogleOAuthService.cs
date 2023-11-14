using System.Text.Json;
using System.Text.Json.Serialization;

namespace HELMoliday.Services.OAuth;
public class GoogleOAuthService
{
    private readonly HttpClient _httpClient;

    public GoogleOAuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetTokenAsync(string code)
    {
        var requestUrl = "https://oauth2.googleapis.com/token";

        var formData = new Dictionary<string, string>
        {
            { "client_id", "357502490301-kdn4nnodt0rdtgl44546s6s3chipr6h4.apps.googleusercontent.com" },
            { "client_secret", "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" },
            { "code", code },
            { "redirect_uri", "http://localhost:5173/oauth/google" },
            { "grant_type", "authorization_code" }
        };

        var content = new FormUrlEncodedContent(formData);

        var response = await _httpClient.PostAsync(requestUrl, content);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(jsonResponse);
        return tokenResponse.AccessToken;
    }

    public async Task<UserInfo> GetUserInfoAsync(string accessToken)
    {
        // Récupérer les informations principales de l'utilisateur
        var requestUrl = $"https://www.googleapis.com/oauth2/v3/userinfo?access_token={accessToken}";

        var response = await _httpClient.GetAsync(requestUrl);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        return JsonSerializer.Deserialize<UserInfo>(jsonResponse, options);
    }

    public record UserInfo(
        string Sub,
        string Name,
        [property: JsonPropertyName("given_name")] string GivenName,
        [property: JsonPropertyName("family_name")] string FamilyName,
        string Picture,
        string Email,
        bool EmailVerified,
        string Locale);

    public record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken);
}
