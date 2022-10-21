using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using CovidLetter.Frontend.WebApp.Constants;
using CovidLetter.Frontend.WebApp.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace CovidLetter.Frontend.WebApp.Models
{
    public class DateOfBirthViewModel : IValidatableObject
    {
        [Display(Name = "day")]
        [Required(ErrorMessage = "validationMissingDay")]
        [Range(1, 31, ErrorMessage = "validationInvalidDay")]
        public int? Day { get; set; }

        [Display(Name = "month")]
        [Required(ErrorMessage = "validationMissingMonth")]
        [Range(1, 12, ErrorMessage = "validationInvalidMonth")]
        public int? Month { get; set; }

        [Display(Name = "year")]
        [Required(ErrorMessage = "validationMissingYear")]
        [RegularExpression(UIConstants.FourDigitYear, ErrorMessage = "validationInvalidYear")]
        public int? Year { get; set; }


        public InitUserJourney? UserJourney { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var localizer = validationContext.GetRequiredService<IStringLocalizer<DateOfBirthViewModel>>();
            var httpContext = validationContext.GetRequiredService<IHttpContextAccessor>().HttpContext;
            var requestingForMyself = httpContext?.RequestingForMyself() ?? true;
            var parentalOrGuardianConsent = httpContext?.ParentalOrGuardianConsent() ?? false;

            if (DateTime.TryParseExact(
                    $"{Day}/{Month}/{Year}",
                    "d/M/yyyy",
                    CultureInfo.CurrentCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var parsedDate))
            {
                if (parsedDate > DateTime.UtcNow.Date)
                {
                    yield return new(localizer["validationDateNotInPast"], new[] { nameof(Day) });
                }
                else if (parsedDate < new DateTime(1900, 1, 1))
                {
                    yield return new(localizer["validationInvalidDate"], new[] { nameof(Day) });
                }
                else if (parsedDate > DateTime.UtcNow.AddYears(-5).Date)
                {
                    yield return new(UserJourney == InitUserJourney.Letter ? 
                        localizer["validationMinimumAgeLetter"] : 
                        localizer["validationMinimumAgeDigital"],
                        new[] { nameof(Day) });
                }
                else if (requestingForMyself && parsedDate > DateTime.UtcNow.AddYears(-12).Date)
                {
                    yield return new(UserJourney == InitUserJourney.Letter ?
                        localizer["validationMinimumAgeSelfLetter"] :
                        localizer["validationMinimumAgeSelfDigital"],
                        new[] { nameof(Day) });
                }
                else if (!requestingForMyself && parsedDate > DateTime.UtcNow.AddYears(-16).Date && !parentalOrGuardianConsent)
                {
                    yield return new(localizer["validationMissingConsent"], new[] { nameof(Day) });
                }
                else if ((parentalOrGuardianConsent && parsedDate <= DateTime.UtcNow.AddYears(-16).Date))
                {
                    yield return new(UserJourney == InitUserJourney.Letter ?
                        localizer["validationMinimumAgeNotSelfLetter"] :
                        localizer["validationMinimumAgeNotSelfDigital"],
                        new[] { nameof(Day) });
                }
            }
            else
            {
                yield return new(localizer["validationInvalidDate"], new[] { nameof(Day) });
            }
        }
    }
}
