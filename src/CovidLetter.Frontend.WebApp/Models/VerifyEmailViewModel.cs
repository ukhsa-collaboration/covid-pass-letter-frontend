using CovidLetter.Frontend.WebApp.Models.Validation;

namespace CovidLetter.Frontend.WebApp.Models
{
    public class VerifyEmailViewModel
    {
        private string _emailAddress = "";
        private string _emailAddressConfirmation = "";
        private int? _remainingEmailAttempts;

        [EmailValidator]
        public string EmailAddress
        {
            get => _emailAddress;
            set => _emailAddress = value != null ? value.Trim() : string.Empty;
        }

        public int? RemainingEmailAttempts
        {
            get => _remainingEmailAttempts;
            set => _remainingEmailAttempts = value ?? 0;
        }

        public string EmailAddressConfirmation
        {
            get => _emailAddressConfirmation;
            set => _emailAddressConfirmation = value != null ? value.Trim() : string.Empty;
        }
    }
}