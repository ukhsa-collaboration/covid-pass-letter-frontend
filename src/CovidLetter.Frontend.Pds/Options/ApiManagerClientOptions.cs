namespace CovidLetter.Frontend.Pds.Options
{
    public class ApiManagerClientOptions
    {
        public string BaseUrl { get; set; } = string.Empty;

        public string SubscriptionKey { get; set; } = string.Empty;

        public string GetOperation { get; set; } = string.Empty;
        
        public string SearchOperation { get; set; } = string.Empty;
        
        public string KeyHeaderName { get; set; } = default;
    }
}
