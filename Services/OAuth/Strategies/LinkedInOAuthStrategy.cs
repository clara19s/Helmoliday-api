using HELMoliday.Exceptions;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace HELMoliday.Services.OAuth.Strategies;
public class LinkedInOAuthStrategy : IOAuthStrategy
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LinkedInOAuthStrategy> _logger;

    public LinkedInOAuthStrategy(HttpClient httpClient, ILogger<LinkedInOAuthStrategy> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UserInfo> AuthenticateAsync(string code)
    {
        var accessToken = await GetAccessToken(code);
        var userInfo = await GetUserInfo(accessToken.AccessToken);

        return new UserInfo(
            userInfo.Email,
            userInfo.FirstName,
            userInfo.LastName
        );
    }

    private async Task<LinkedInAccessTokenResponse> GetAccessToken(string code)
    {
        var formData = new Dictionary<string, string>
        {
            { "client_id", "78baupv9jmxu8s" },
            { "client_secret", "ROktnc96Uva1oBpt" },
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", "http://localhost:5173/oauth/linkedin" }
        };

        var content = new FormUrlEncodedContent(formData);
        var accessTokenRequest = await _httpClient.PostAsync("https://www.linkedin.com/oauth/v2/accessToken", content);
        accessTokenRequest.EnsureSuccessStatusCode();

        var response = await accessTokenRequest.Content.ReadAsStringAsync();

        if (string.IsNullOrEmpty(response))
        {
            var errorMessage = "LinkedIn access token response was empty.";
            _logger.LogError(errorMessage);
            throw new OAuthException(errorMessage);
        }

        return JsonConvert.DeserializeObject<LinkedInAccessTokenResponse>(response);
    }

    private async Task<LinkedInUserResponse> GetUserInfo(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var userInfoRequest = await _httpClient.GetAsync("https://api.linkedin.com/v2/userinfo");
        userInfoRequest.EnsureSuccessStatusCode();

        var response = await userInfoRequest.Content.ReadAsStringAsync();

        if (string.IsNullOrEmpty(response))
        {
            var errorMessage = "LinkedIn user info response was empty.";
            _logger.LogError(errorMessage);
            throw new OAuthException(errorMessage);
        }

        return JsonConvert.DeserializeObject<LinkedInUserResponse>(response);
    }

    private sealed class LinkedInAccessTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }

    private sealed class LinkedInUserResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("given_name")]
        public string FirstName { get; set; }

        [JsonProperty("family_name")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}