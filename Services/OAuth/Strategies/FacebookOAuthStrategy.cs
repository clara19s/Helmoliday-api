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
            var name = userInfo.Name.Split(" ");
            var firstname = name[0];
            var lastname = name[1];

            return new UserInfo(
                userInfo.Email,
               firstname, 
               lastname
            );
        }

        private async Task<FacebookAccessTokenResponse> GetAccessToken(string code)
        {
            
            var accessTokenRequest = await _httpClient.GetAsync($"https://graph.facebook.com/v18.0/oauth/access_token?client_id=370977798709160&redirect_uri=http://localhost:5173/oauth/facebook&client_secret=93306bbcba0a87601c97f49426a47fbc&code={code}");
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
            var userInfoRequest = await _httpClient.GetAsync($"https://graph.facebook.com/me?fields=name,email&access_token={accessToken}");
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

         
             [JsonProperty("token_type")]
            public string TokenType { get; set; }


            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }
        }

        private sealed class FacebookUserResponse
        {
            [JsonProperty("id")]
            public string Id { get; set; }
         
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }
        }
    }
}
