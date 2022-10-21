namespace CovidLetter.Frontend.AccessTokenCache.Models
{
    public class JwtResponse
    {
        public string Token { get; set; } = string.Empty;

        public long ExpiresAt { get; set; }
    }
}
