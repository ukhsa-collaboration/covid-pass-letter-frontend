using CovidLetter.Frontend.Extensions;
using CovidLetter.Frontend.Search;
using CovidLetter.Frontend.WebApp.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CovidLetter.Frontend.WebApp.Models
{
    public class VerifyMobileViewModel : IValidatableObject
    {
        private string _mobileNumber = "";
        private int? _remainingMobileAttempts;

        [Required(ErrorMessage = "missingMobileNumber")]
        public string MobileNumber
        {
            get => _mobileNumber;
            set => _mobileNumber = value != null ? value.Trim() : string.Empty;
        }

        public int? RemainingMobileAttempts
        {
            get => _remainingMobileAttempts;
            set => _remainingMobileAttempts = value ?? 0;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var localizer = validationContext.GetRequiredService<IStringLocalizer<VerifyMobileViewModel>>();
            var httpContext = validationContext.GetRequiredService<IHttpContextAccessor>().HttpContext;
            var tempData = httpContext.GetTempData();
            var pdsMobileNumbersDictionary = tempData?.Get<SearchResultData>()?.Mobiles;

            if (!(SearchPatientService.IsValidUkMobilePhoneNumber(MobileNumber) || SearchPatientService.IsValidInternationalPhoneNumber(MobileNumber)))
            {
                yield return new(localizer["noneValidMobileNumber"], new[] { nameof(MobileNumber) });
                yield break;
            }

            var matchFound = false;
            if (pdsMobileNumbersDictionary != null)
            {
                foreach (var pdsNumber in pdsMobileNumbersDictionary.Values)
                {
                    if (SearchPatientService.NumbersAreSame(pdsNumber, MobileNumber))
                    {
                        matchFound = true;
                        break;
                    }
                }
            }

            if (!matchFound)
            {
                var userSessionData = tempData?.Get<UserSessionData>();
                if (userSessionData != null && tempData != null)
                {
                    userSessionData.remainingMobileAttempts -= 1;
                    tempData.Set(userSessionData);
                    yield return new(localizer["noneMatchingMobileNumber"], new[] { nameof(MobileNumber) });
                }
            }
        }
    }
}