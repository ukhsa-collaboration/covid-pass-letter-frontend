using CovidLetter.Frontend.WebApp.Models;

namespace CovidLetter.Frontend.WebApp.Services;

public class DigitalModelService
{
    private readonly IObfuscationService _obfuscationService;

    public DigitalModelService(IObfuscationService obfuscationService)
    {
        _obfuscationService = obfuscationService;
    }

    public void PopulateViewModelWithObfuscatedDigitalContactDetails(
        DigitalContactPreferenceViewModel viewModel,
        UserSessionData userSessionData)
    {
        var email = userSessionData.VerifyEmailAddress?.EmailAddress != null ? userSessionData.VerifyEmailAddress?.EmailAddress : userSessionData.VerifyEmail?.EmailAddress;
        var phone = userSessionData.VerifyMobile?.MobileNumber;
        if (phone != null) AddPhoneNumber(phone, viewModel);

        if (email != null) AddEmail(email, viewModel);
    }

    private void AddEmail(string email, DigitalContactPreferenceViewModel viewModel)
    {
            if (_obfuscationService.TryObfuscateEmail(
                    email,
                    out var obfuscatedEmail))
            {
                viewModel.ObfuscatedContactDetails.Add(
                    DigitalContactPreferenceOptions.Email,
                    obfuscatedEmail
                );
            }
    }

    private void AddPhoneNumber(string phone, DigitalContactPreferenceViewModel viewModel)
    {
            var obfuscatedPhoneNumber = _obfuscationService.ObfuscatePhone(phone);

            if (!string.IsNullOrEmpty(obfuscatedPhoneNumber))
            {
                viewModel.ObfuscatedContactDetails.Add(
                    DigitalContactPreferenceOptions.Phone,
                    obfuscatedPhoneNumber);
            }
    }
}