namespace CovidLetter.Frontend.WebApp.Configuration;

public class OtpServiceOptions
{
    public string BaseUrl { get; set; } = string.Empty;

    public string SubscriptionKey { get; set; } = string.Empty;

    public string RequestOperation { get; set; } = string.Empty;

    public string VerifyOperation { get; set; } = string.Empty;

    public string? KeyHeaderName { get; set; } = default;
}