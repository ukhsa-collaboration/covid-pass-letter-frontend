using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;
namespace CovidLetter.Frontend.WebApp.Models.Validation
{
    public class EmailValidator : ValidationAttribute
    {
        private static readonly Regex ValidEmailAddress = new(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~\-]+@([^.@][^@\s]+)$");
        private static readonly Regex ValidEmailHostnamePart = new(@"^(xn|[a-z0-9]+)(-?-[a-z0-9]+)*$", RegexOptions.IgnoreCase);
        private static readonly Regex ValidEmailTldPart = new("^([a-z]{2,63}|xn--([a-z0-9]+-)*[a-z0-9]+)$", RegexOptions.IgnoreCase);
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var contextModelName = validationContext.ObjectType.Name;
            IStringLocalizer? localizer = null;

            if (contextModelName == "InputEmailViewModel")
            {
                localizer = validationContext.GetRequiredService<IStringLocalizer<InputEmailViewModel>>();
            }
            else if (contextModelName == "VerifyEmailViewModel")
            {
                localizer = validationContext.GetRequiredService<IStringLocalizer<VerifyEmailViewModel>>();
            }

            var inlineElementName = new[] { validationContext.MemberName };
            if (string.IsNullOrWhiteSpace(value?.ToString()) && localizer != null)
            {
                return new ValidationResult(localizer["validationMissingEmail"], inlineElementName!);
            }
            var emailAddress = value?.ToString()?.Trim();
            var match = ValidEmailAddress.Match(emailAddress!);
            if ((!match.Success || emailAddress!.Length > 320 || emailAddress.Contains("..")) && localizer != null)
            {
                return new ValidationResult(localizer["validationWrongFormat"], inlineElementName!);
            }
            var hostname = match.Groups[1].Value;
            var idn = new IdnMapping();
            try
            {
                hostname = idn.GetAscii(hostname);
            }
            catch
            {
                // Decoded string is not a valid IDN name.
                return new ValidationResult(localizer?["validationWrongFormat"] ?? "validationWrongFormat", inlineElementName!);
            }
            var hostnameParts = hostname.Split('.');
            if (hostname.Length > 253 || hostnameParts.Length < 2)
            {
                return new ValidationResult(localizer!["validationWrongFormat"], inlineElementName!);
            }
            if (hostnameParts.Any(part =>
                    string.IsNullOrWhiteSpace(part)
                    || part.Length > 63
                    || !ValidEmailHostnamePart.IsMatch(part)))
            {
                return new ValidationResult(localizer!["validationWrongFormat"], inlineElementName!);
            }
            if (!ValidEmailTldPart.IsMatch(hostnameParts[^1]))
            {
                return new ValidationResult(localizer!["validationWrongFormat"], inlineElementName!);
            }
            return ValidationResult.Success;
        }
    }
}