using CovidLetter.Frontend.WebApp.Constants;

namespace CovidLetter.Frontend.WebApp.Configuration
{
    public class SiteConfiguration
    {
        public static string Identifier = "SiteConfiguration";

        public string StartPage => UIConstants.Home.RequestLetterForTravelRoute;

        public string DigitalAccessibilityStatementPage { get; set; } = "";

        public string HelpAndSupportPage { get; set; } = "";

        public string PrivacyPolicyPage { get; set; } = "";

        public string FiveToElevenPrivacyPolicyPage { get; set; } = "";

        public string UnderSixteenPrivacyPolicyPage { get; set; } = "";

        public string TermsAndConditionsPage { get; set; } = "";
    }
}