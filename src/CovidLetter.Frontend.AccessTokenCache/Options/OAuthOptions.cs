namespace CovidLetter.Frontend.AccessTokenCache.Options
{
    public class OAuthOptions
    {
        public string Kid { get; set; } = string.Empty;

        public string TokenEndpoint { get; set; } = string.Empty;

        public string IssuerKey { get; set; } = string.Empty;

        public string PrivateKey { get; set; } = string.Empty;
    }
}
