namespace CovidLetter.Frontend.WebApp.Models
{
    public class UserSessionData
    {
        public WhoAreYouRequestingForModel? RequestingFor { get; set; }

        public UserJourneyModel? UserJourney { get; set; }

        public NhsNumberViewModel? NhsNumber { get; set; }

        public NameViewModel? Name { get; set; }

        public DateOfBirthViewModel? DateOfBirth { get; set; }

        public PostcodeViewModel? Postcode { get; set; }

        public VerifyMobileViewModel? VerifyMobile { get; set; }

        public InputEmailViewModel? VerifyEmailAddress { get; set; }

        public EmailOrPhoneNumberChoiceViewModel? EmailOrPhoneNumberChoice { get; set; }

        public VerifyEmailViewModel? VerifyEmail { get; set; }

        public VerifyOtpViewModel? VerifyOtp { get; set; }

        public AlternateLetterTypeModel? AlternateLetterType { get; set; }

        public LanguagesViewModel? Languages { get; set; }

        public ImmediatePassViewModel? ImmediatePass { get; set; }

        public DigitalContactPreferenceViewModel? DigitalContactPreference { get; set; }

        public ContactPreferenceViewModel? LetterContactPreference { get; set; }

        public EligibilityForLetterViewModel? EligibilityForLetter { get; set; }

        public bool? otpMatchSuccess { get; set; }

        public bool userEligibleForImmediatePass { get; set; }
        
        public int? remainingMobileAttempts { get; set; }

        public int? remainingEmailAttempts { get; set; }

        public int? remainingOtpAttempts { get; set; }

        public bool isCoronavirusHelplineUser { get; set; }
    }
}