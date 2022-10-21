using Newtonsoft.Json;

namespace CovidLetter.Frontend.AccessTokenCache.Models
{
    public class AccessTokenDto
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; } = string.Empty;
    }
}
