using HELMoliday.Exceptions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace HELMoliday.Services.OAuth.Strategies
{
    public class FacebookOAuthStrategy : IOAuthStrategy
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FacebookOAuthStrategy> _logger;

        public FacebookOAuthStrategy(HttpClient httpClient, ILogger<FacebookOAuthStrategy> logger)
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

        private async Task<FacebookAccessTokenResponse> GetAccessToken(string code)
        {
            var formData = new Dictionary<string, string>
            {
                { "client_id", "370977798709160" },
                { "client_secret", "93306bbcba0a87601c97f49426a47fbc" },
                { "code", code },
                { "redirect_uri", "http://localhost:5173/oauth/facebook" },
            };

            var content = new FormUrlEncodedContent(formData);
            var accessTokenRequest = await _httpClient.PostAsync("https://connect.facebook.net/en_US/sdk.js", content);
            accessTokenRequest.EnsureSuccessStatusCode();

            var response = await accessTokenRequest.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(response))
            {
                var errorMessage = "Facebook access token response was empty.";
                _logger.LogError(errorMessage);
                throw new OAuthException(errorMessage);
            }

            return JsonConvert.DeserializeObject<FacebookAccessTokenResponse>(response);
        }

        private async Task<FacebookUserResponse> GetUserInfo(string accessToken)
        {
            var userInfoRequest = await _httpClient.GetAsync($"https://graph.facebook.com/v13.0/me?fields=id,first_name,last_name,email&access_token={accessToken}");
            userInfoRequest.EnsureSuccessStatusCode();

            var response = await userInfoRequest.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(response))
            {
                var errorMessage = "Facebook user info response was empty.";
                _logger.LogError(errorMessage);
                throw new OAuthException(errorMessage);
            }

            return JsonConvert.DeserializeObject<FacebookUserResponse>(response);
        }

        private sealed class FacebookAccessTokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }
        }

        private sealed class FacebookUserResponse
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("first_name")]
            public string FirstName { get; set; }

            [JsonProperty("last_name")]
            public string LastName { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }
        }
    }
}
