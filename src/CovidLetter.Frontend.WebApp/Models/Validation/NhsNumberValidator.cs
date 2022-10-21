using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace CovidLetter.Frontend.WebApp.Models.Validation
{
    public class NhsNumberValidator : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var model = (NhsNumberViewModel)validationContext.ObjectInstance;
            var knowsNhsNumber = model.KnowsNhsNumber;
            var localizer = validationContext.GetRequiredService<IStringLocalizer<NhsNumberViewModel>>();

            if (!knowsNhsNumber.GetValueOrDefault())
            {
                return ValidationResult.Success;
            }

            var memberNames = new[] {nameof(NhsNumberViewModel.NhsNumber)};
            if (value is not string stringValue || stringValue.Trim().Length == 0)
            {
                return new ValidationResult(localizer["validationRequired"], memberNames);
            }

            var nhsString = stringValue.Replace(" ", "");
            if (nhsString.Length != 10 || !long.TryParse(nhsString, out var nhsNoAsLong) || nhsNoAsLong <= 0)
            {
                return new ValidationResult(localizer["validationInvalid"], memberNames);
            }

            var ckDigit = nhsString[9..];
            var ckTotal = 0;
            for (var i = 0; i < 9; i++)
            {
                ckTotal += (int.Parse(nhsString.Substring(i, 1)) * (10 - i));
            }

            var chk = 11 - ckTotal % 11;
            if (chk == 11)
            {
                chk = 0;
            }

            return ckDigit == chk.ToString() 
                ? ValidationResult.Success 
                : new ValidationResult(localizer["validationInvalid"], memberNames);
        }
    }
}
