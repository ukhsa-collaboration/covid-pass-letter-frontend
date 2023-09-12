using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CovidLetter.Frontend.WebApp.Models
{
    public class VerifyOtpViewModel : IValidatableObject
    {

        private string? _otpCode;
        private int? _remainingOtpAttempts;

        public string? OtpCode
        {
            get => _otpCode;
            set => _otpCode = value != null ? value.Trim() : string.Empty;
        }

        public int? RemainingOtpAttempts
        {
            get => _remainingOtpAttempts;
            set => _remainingOtpAttempts = value ?? 0;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var localizer = validationContext.GetRequiredService<IStringLocalizer<VerifyOtpViewModel>>();

            if (OtpCode?.Length == 0)
            {
                yield return new(localizer["missingOtp"], new[] { nameof(OtpCode) });
                yield break;
            }
            if (OtpCode?.Length < 6)
            {
                yield return new(localizer["otpEntryTooShort"], new[] { nameof(OtpCode) });
                yield break;
            }
            if (OtpCode?.Length > 6)
            {
                yield return new(localizer["otpEntryTooLong"], new[] { nameof(OtpCode) });
                yield break;
            }

            string regExPattern = @"^[0-9]+$";
            Regex pattern = new Regex(regExPattern);
            var otpIsOnlyDigits = pattern.IsMatch(OtpCode);

            if (!otpIsOnlyDigits)
            {
                yield return new(localizer["noneDigitsInOtp"], new[] { nameof(OtpCode) });
            }
        }
    }
}