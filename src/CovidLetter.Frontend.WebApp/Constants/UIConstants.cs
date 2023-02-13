namespace CovidLetter.Frontend.WebApp.Constants
{
    public static class UIConstants
    {
        public const string FourDigitYear = @"^(\d{4})$";
        public const string NotRequestedValue = "Not requested";
        public const string EnglishLanguageCulture = "en-GB";
        public const string WelshLanguageCulture = "cy-GB";

        /* PRE-PDS routes */
        public static class Home
        {
            public const string InitialRoute = "/";
            public const string HomeController = "Home";
            public const string CookiePolicy = "cookie-policy";
            public const string SetLanguage = "set-language";
            public const string AccessibilityStatement = "accessibility-statement";
            /* pre-pds pages */
            public const string RequestLetterForTravelRoute = "request-letter-for-travel";
            public const string WhoAreYouRequestingForRoute = "who-are-you-requesting-for";
            public const string DateOfBirth = "what-is-your-dob";
            public const string NhsNumber = "do-you-know-nhs-number";
            public const string Name = "what-is-your-name";
            public const string Postcode = "what-is-your-postcode";
            public const string CheckYourAnswers = "check-your-answers";
            /* end of flow pages */
            public const string Error = "error";
            public const string SessionExpiredPath = "/expired";
        }

        /* LETTER flow routes */
        public static class Outcome
        {
            public const string OutcomeController = "Outcome";
            /* post-pds end of flow pages */
            public const string NoMatch = "no-match";
            /* post-pds onward journey pages */
            public const string LetterAlternativeType = "do-you-need-an-accessible-format";
            public const string AllLanguagesRoute = "languages";
            public const string ImmediatePass = "immediate-covid-pass";
            public const string FailureContactPreference = "how-would-you-like-to-be-contacted";
            /* end of flow pages */
            public const string Submitted = "submitted";
        }

        /* DIGITAL flow routes */
        public static class Digital
        {
            public const string DigitalController = "Digital";
            public const int InitialAllowedAttempts = 3;
            public const int InitialAllowedOtpAttempts = 5;
            public const int MaximumAllowedOtpGenerations = 5;
            /* post-pds onward journey pages */
            public const string EmailOrPhoneNumberChoice = "email-or-phone-number-choice";
            public const string OtpVerifyMobile = "verify-mobile";
            public const string OtpVerifyCode = "verify-otp";
            public const string RequestNewOtp = "request-new-otp-code";
            public const string EmailInput = "email-input";
            public const string VerifyEmail = "verify-email";
            public const string HowToContact = "digital-how-would-you-like-to-be-contacted";
            /* end of flow pages */
            public const string UserNotEligibleForDigitalFlow = "user-not-eligible-for-digital-flow";
            public const string UserFoundWithoutContactDetails = "gp-record-found-without-contact-details";
            public const string Submitted = "digital-submitted";
            public const string MaximumAttemptsReached = "maximum-attempts-reached";
            public const string MaximumOtpAttemptsReached = "maximum-otp-attempts-reached";
            public const string MaximumOtpsGenerated = "maximum-otps-generated";
        }
    }
}