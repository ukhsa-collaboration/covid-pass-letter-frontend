namespace CovidLetter.Frontend.WebApp.Configuration
{
    public class AuthenticationConfiguration
    {
        public const string AuthenticationSectionName = "Authentication";

        public int ExpireMinutes { get; set; } = 10;

        public int SessionExpireNotificationMinutes { get; set; }
    }
}