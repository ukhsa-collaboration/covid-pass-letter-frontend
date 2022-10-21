using System;
using System.Text;
using System.Text.RegularExpressions;
using CovidLetter.Frontend.Logging;
using Microsoft.Extensions.Logging;

namespace CovidLetter.Frontend.WebApp.Services
{
    public class ObfuscationService : IObfuscationService
    {
        private readonly ILogger<ObfuscationService> _logger;
        public ObfuscationService(ILogger<ObfuscationService> logger)
        {
            _logger = logger;
        }

        const int NumberOfDigitsOfMobileNumberToDisplay = 3;

        public bool TryObfuscateEmail(string emailValue, out string obfuscatedEmail)
        {
            if (string.IsNullOrEmpty(emailValue))
            {
                obfuscatedEmail = string.Empty;
                return false;
            }

            try
            {
                var atSymbolSplitIndex = emailValue.LastIndexOf("@");
                var localPart = emailValue.Substring(0, atSymbolSplitIndex);
                var fullDomain = emailValue.Substring(atSymbolSplitIndex, emailValue.Length - atSymbolSplitIndex).TrimStart('@');
                var domainSplitIndex = fullDomain.IndexOf(".");
                var domainFirstPart = fullDomain.Substring(0, domainSplitIndex);
                var domainSecondPart = fullDomain.Substring(domainSplitIndex, fullDomain.Length - domainSplitIndex);

                var result = new StringBuilder();

                var numberOfCharactersToHideInLocalPart = GetNumberOfCharactersToHideInEmailPart(localPart);
                result.Append(new string('•', numberOfCharactersToHideInLocalPart));
                result.Append(localPart.Substring(numberOfCharactersToHideInLocalPart, localPart.Length - numberOfCharactersToHideInLocalPart));

                result.Append("@");

                var numberOfCharactersToHideInDomain = GetNumberOfCharactersToHideInEmailPart(domainFirstPart);
                result.Append(new string('•', numberOfCharactersToHideInDomain));
                result.Append(domainFirstPart.Substring(numberOfCharactersToHideInDomain, domainFirstPart.Length - numberOfCharactersToHideInDomain));

                result.Append(domainSecondPart);

                obfuscatedEmail = result.ToString();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(
                    AppEventId.ErrorObfuscatingEmail, e, "Error obfuscating email");
                
                obfuscatedEmail = string.Empty;
                return false;
            }
        }

        public string ObfuscatePhone(string phoneValue)
        {
            var phoneNumber = Regex.Replace(phoneValue, @"(\s+|@|&|'|\(|\)|<|>|#|-|)", "");
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return string.Empty;
            }

            if (phoneNumber.Length <= NumberOfDigitsOfMobileNumberToDisplay)
            {
                _logger.LogInformation(
                    AppEventId.ErrorObfuscatingMobile,
                    "Can't show last {NumberOfDigitsOfMobileNumberToDisplay} digits of phone number when length is only {PhoneValueLength}", 
                    NumberOfDigitsOfMobileNumberToDisplay,
                    phoneValue.Length);
                return string.Empty;
            }

            if (phoneNumber.Length < 10 || phoneNumber.Length > 15)
            {
                _logger.LogInformation(
                    AppEventId.ErrorObfuscatingMobile,
                    "Phone number too small or big, digits of the phone number is {PhoneNumberLength} and only allowed between 11 to 15 digits", 
                    phoneNumber.Length);
                
                return string.Empty;
            }
            return phoneNumber.Substring(phoneNumber.Length - NumberOfDigitsOfMobileNumberToDisplay);
        }

        private int GetNumberOfCharactersToHideInEmailPart(string value)
        {
            if (value.Length == 1)
            {
                return 1;
            }

            if (value.Length <= 5)
            {
                return value.Length - 1;
            }

            if (value.Length <= 10)
            {
                return value.Length - 2;
            }

            return value.Length - 3;
        }
    }
}