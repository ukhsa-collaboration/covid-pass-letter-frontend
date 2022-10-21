using System;
using System.Linq;
using System.Threading.Tasks;
using CovidLetter.Frontend.Logging;
using CovidLetter.Frontend.WebApp.Configuration;
using CovidLetter.Frontend.WebApp.Extensions;
using CovidLetter.Frontend.WebApp.Models;
using CovidLetter.Frontend.WebApp.Services;
using CovidLetter.Frontend.WebApp.Services.Queue;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using CovidLetter.Frontend.WebApp.Constants;

namespace CovidLetter.Frontend.WebApp.Controllers
{
    public class OutcomeController : Controller
    {
        private readonly ILogger<OutcomeController> _logger;
        private readonly LetterRequestService _letterRequestService;
        private readonly OutcomeModelService _outcomeModelService;
        private readonly string _startPage;
        private readonly IStringLocalizer<ContactPreferenceViewModel> _contactPreferenceLocalizer;
        private readonly IStringLocalizer<ImmediatePassViewModel> _immediatePassLocalizer;
        private readonly SiteConfiguration _siteConfiguration;

        public OutcomeController(
            ILogger<OutcomeController> logger,
            LetterRequestService letterRequestService,
            IOptions<SiteConfiguration> siteConfiguration,
            OutcomeModelService outcomeModelService,
            IStringLocalizer<ContactPreferenceViewModel> contactPreferenceLocalizer,
            IStringLocalizer<ImmediatePassViewModel> immediatePassLocalizer)
        {
            _logger = logger;
            _letterRequestService = letterRequestService;
            _outcomeModelService = outcomeModelService;
            _startPage = siteConfiguration.Value.StartPage;
            _contactPreferenceLocalizer = contactPreferenceLocalizer;
            _immediatePassLocalizer = immediatePassLocalizer;
            _siteConfiguration = siteConfiguration.Value;
        }

        [HttpGet]
        [Route(UIConstants.Outcome.NoMatch)]
        public IActionResult NoMatch()
        {
            var userSessionData = TempData.Get<UserSessionData>();

            if (userSessionData == null)
                return Redirect(_startPage);

            ViewBag.IsDigital = userSessionData.UserJourney?.PrePdsJourney == InitUserJourney.Digital;
            TempData.Clear();

            return View();
        }

        [HttpGet]
        [Route(UIConstants.Outcome.AllLanguagesRoute)]
        public IActionResult Languages()
        {
            var userSessionData = TempData.Get<UserSessionData>();
            var userJourneyIsDigital = userSessionData?.UserJourney?.PrePdsJourney == InitUserJourney.Digital;

            if (userSessionData == null)
                return Redirect(_siteConfiguration.StartPage);

            if (!UserIsPostPds())
                return RedirectToAction("CheckAnswers", UIConstants.Home.HomeController);

            if (userJourneyIsDigital)
            {
                SetAccessibleAndAlternateLanguagesChoices(
                    userSessionData: userSessionData,
                    accessibleType: UIConstants.NotRequestedValue,
                    accessibleTypeRequested: false,
                    anotherLanguageRequested: false
                );
                TempData.Set(userSessionData);

                return RedirectToAction("CheckAnswers", UIConstants.Home.HomeController);
            }

            if (RegionServices.PostcodeIsInWales(userSessionData.Postcode?.Postcode))
            {
                SetAccessibleAndAlternateLanguagesChoices(
                    userSessionData: userSessionData,
                    accessibleType: UIConstants.NotRequestedValue,
                    accessibleTypeRequested: false,
                    anotherLanguageRequested: false
                );
                TempData.Set(userSessionData);

                return UserIsPostPds() ? RedirectToAction("ContactPreference") : RedirectToAction("CheckAnswers", UIConstants.Home.HomeController);
            }

            if (userSessionData.AlternateLetterType?.UserRequestedAnotherLanguage == null || userSessionData.AlternateLetterType?.UserRequestedAnotherLanguage.GetValueOrDefault() == false)
            {
                return RedirectToAction("EligibilityForLetter");
            }

            var model = new LanguagesViewModel
            {
                Language = userSessionData.Languages!.Language
            };

            return View(model);
        }

        [HttpPost]
        [Route(UIConstants.Outcome.AllLanguagesRoute)]
        public IActionResult Languages(LanguagesViewModel model)
        {
            var userSessionData = TempData.Get<UserSessionData>();
            if (userSessionData == null)
                return Redirect(_siteConfiguration.StartPage);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            userSessionData.Languages = model;
            TempData.Set(userSessionData);

            return RedirectToAction(nameof(OutcomeController.ContactPreference));
        }

        [HttpGet]
        [Route(UIConstants.Outcome.FailureContactPreference)]
        public IActionResult ContactPreference()
        {
            var userSessionData = TempData.Get<UserSessionData>();

            if (userSessionData == null)
                return Redirect(_startPage);

            if (!UserIsPostPds())
                return RedirectToAction("CheckAnswers", UIConstants.Home.HomeController);

            if ((userSessionData!.UserJourney?.PrePdsJourney == InitUserJourney.Digital) ||
                (!RegionServices.PostcodeIsInWales(userSessionData.Postcode?.Postcode) && (userSessionData.AlternateLetterType == null))
                )
                return Redirect(_startPage);

            var viewModel = new ContactPreferenceViewModel();
            var contactDetailsData = TempData.Get<SearchResultData>();

            _outcomeModelService.PopulateViewModelWithObfuscatedContactDetails(
                viewModel,
                contactDetailsData!,
                (userSessionData.AlternateLetterType!.UserRequestedAccessibleFormat.GetValueOrDefault() ||
                 userSessionData.AlternateLetterType.UserRequestedAnotherLanguage.GetValueOrDefault()),
                userSessionData.DateOfBirth!
            );

            viewModel.userRequestAlternativeLanguage = userSessionData.AlternateLetterType.UserRequestedAnotherLanguage.GetValueOrDefault();
            viewModel.postcodeIsWelsh = RegionServices.PostcodeIsInWales(userSessionData.Postcode?.Postcode);
            return View(viewModel);
        }

        [HttpPost]
        [Route(UIConstants.Outcome.FailureContactPreference)]
        public async Task<IActionResult> ContactPreference(ContactPreferenceViewModel model)
        {
            var userSessionData = TempData.Get<UserSessionData>();
            var contactDetailsData = TempData.Get<SearchResultData>();

            if (userSessionData == null || contactDetailsData == null)
                return Redirect(_startPage);

            if (userSessionData!.DateOfBirth == null || !RequestIsForOver12(userSessionData!.DateOfBirth) || (!contactDetailsData.Emails.Any() && !contactDetailsData.Mobiles.Any()))
            {
                model.Selection = Guid.Empty;
            }
            else if (model.Selection == null)
            {
                _outcomeModelService.
                PopulateViewModelWithObfuscatedContactDetails(
                    model,
                    contactDetailsData,
                    (userSessionData.AlternateLetterType!.UserRequestedAccessibleFormat.GetValueOrDefault() ||
                     userSessionData.AlternateLetterType.UserRequestedAnotherLanguage.GetValueOrDefault()),
                    userSessionData.DateOfBirth);

                ModelState.AddModelError("Selection", _contactPreferenceLocalizer["noSelection"]);
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                _outcomeModelService.
                PopulateViewModelWithObfuscatedContactDetails(
                    model,
                    contactDetailsData,
                    (userSessionData.AlternateLetterType!.UserRequestedAccessibleFormat.GetValueOrDefault() ||
                     userSessionData.AlternateLetterType.UserRequestedAnotherLanguage.GetValueOrDefault()),
                    userSessionData.DateOfBirth!);

                return View(model);
            }

            if (!(contactDetailsData.IsValidKey(model.Selection!.Value) || model.Selection == Guid.Empty))
            {
                return Redirect(_startPage);
            }

            var correlationId = TempData.GetCorrelationId();
            var queueMessage = CreateCoreQueueMessage(
                userSessionData,
                contactDetailsData,
                correlationId);

            // confirm the posted value is one of theirs
            if (model.Selection == Guid.Empty)
            {
                // Do not contact option selected
                queueMessage.ContactMethodSettable = ContactMethodEnum.DoNotContact;

                await _letterRequestService.SendLetterRequest(queueMessage);
            }
            else if (contactDetailsData.Emails.TryGetValue(model.Selection!.Value, out var email))
            {
                queueMessage.ContactMethodSettable = ContactMethodEnum.Email;
                queueMessage.EmailAddress = email;

                await _letterRequestService.SendLetterRequest(queueMessage);
            }
            else if (contactDetailsData.Mobiles.TryGetValue(model.Selection!.Value, out var mobile))
            {
                queueMessage.ContactMethodSettable = ContactMethodEnum.SMS;
                queueMessage.MobileNumber = mobile;

                await _letterRequestService.SendLetterRequest(queueMessage);
            }
            else
            {
                _logger.LogInformation(
                    AppEventId.ContactDetailsNotFound,
                    "The user selected value for contact details did not match a value we have stored. Returning them to the screen with an error message");

                ModelState.AddModelError(nameof(model.Selection), _contactPreferenceLocalizer["validationSelectionNotMatchRecords"]);

                _outcomeModelService.
                PopulateViewModelWithObfuscatedContactDetails(
                    model,
                    contactDetailsData,
                    (userSessionData.AlternateLetterType!.UserRequestedAccessibleFormat.GetValueOrDefault() ||
                     userSessionData.AlternateLetterType.UserRequestedAnotherLanguage.GetValueOrDefault()),
                    userSessionData.DateOfBirth!);

                return View(model);
            }

            userSessionData.LetterContactPreference = model;
            TempData.Set(userSessionData);

            return RedirectToAction(nameof(Submitted));
        }

        [HttpGet]
        [Route(UIConstants.Outcome.Submitted)]
        public IActionResult Submitted()
        {
            var userSessionData = TempData.Get<UserSessionData>();
            
            if (userSessionData == null)
                return Redirect(_startPage);

            if (!UserIsPostPds())
                return RedirectToAction("CheckAnswers", UIConstants.Home.HomeController);

            if (userSessionData.UserJourney == null ||userSessionData.LetterContactPreference == null || userSessionData.LetterContactPreference.Selection == null || userSessionData.UserJourney.PrePdsJourney == InitUserJourney.Digital)
                return Redirect(_startPage);
            var model = new SubmittedModel
            {
                IsRequestForSomeoneElse = userSessionData.RequestingFor?.RequestingFor == RequestFor.SomeoneElse,
                Is16OrOver = !UserServices.UserIsUnder16(userSessionData.DateOfBirth?.Day, userSessionData.DateOfBirth?.Month, userSessionData.DateOfBirth?.Year),
                IsInWales = RegionServices.PostcodeIsInWales(userSessionData.Postcode?.Postcode)
            };
            TempData.Clear();
            return View(model);
        }
        
        [HttpGet]
        [Route(UIConstants.Outcome.ImmediatePass)]
        public IActionResult ImmediatePass()
        {
            var userSessionData = TempData.Get<UserSessionData>();
            var contactDetailsData = TempData.Get<SearchResultData>();

            if (userSessionData == null)
                return Redirect(_startPage);

            if (!UserIsPostPds())
                return RedirectToAction("CheckAnswers", UIConstants.Home.HomeController);

            if (contactDetailsData == null ||
                (userSessionData!.UserJourney?.PrePdsJourney == InitUserJourney.Digital) ||
                (RegionServices.PostcodeIsInWales(userSessionData.Postcode?.Postcode)) ||
                (!(contactDetailsData.Mobiles.Any() || contactDetailsData.Emails.Any()))
                )
                return Redirect(_startPage);

            var userHasMobileNumber = TempData.Get<SearchResultData>()!.Mobiles.Any();
            var userHasEmail = TempData.Get<SearchResultData>()!.Emails.Any();

            ImmediatePassViewModel model = new ImmediatePassViewModel()
            {
                RequestFor = userSessionData.RequestingFor?.RequestingFor,
                UserHasMobileOnPdsRecord = userHasMobileNumber,
                UserHasEmailOnPdsRecord = userHasEmail,
                verifyMethod = userHasMobileNumber && userHasEmail ? ImmediateVerifyMethod.email : userHasMobileNumber ? ImmediateVerifyMethod.phone : ImmediateVerifyMethod.email,
                UserRequestedDigitalPass = true
            };

            userSessionData.ImmediatePass = model;
            TempData.Set(userSessionData);

            return View(model);
        }

        [HttpPost]
        [Route(UIConstants.Outcome.ImmediatePass)]
        public IActionResult ImmediatePass(ImmediatePassViewModel model)
        {
            var userSessionData = TempData.Get<UserSessionData>();
            var postPdsUserJourneyForDigitalUserData = TempData.Get<PostPdsJourneyData>();
            var userHasMobileNumber = TempData.Get<SearchResultData>()!.Mobiles.Any();
            var userHasEmail = TempData.Get<SearchResultData>()!.Emails.Any();

            if (userSessionData == null)
                return Redirect(_startPage);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (userSessionData.ImmediatePass != null)
            {
                
                userSessionData.ImmediatePass.verifyMethod = model.verifyMethod;
                TempData.Set(userSessionData);

                if (model.UserRequestedDigitalPass == true)
                {
                    if (userSessionData.UserJourney != null)
                    {
                        /* LETTER journey user switches to the DIGITAL journey */
                        userSessionData.UserJourney.PrePdsJourney = InitUserJourney.Digital;
                        TempData.Set(userSessionData);
                    }

                    var currentLanguageCultureIsWelsh = Request.Cookies[CookieRequestCultureProvider.DefaultCookieName]?.ToString().Contains(UIConstants.WelshLanguageCulture);

                    if (currentLanguageCultureIsWelsh == true)
                        SetUserLanguage(Response, UIConstants.EnglishLanguageCulture);

                    if (model.verifyMethod == ImmediateVerifyMethod.noneSelected)
                    {
                        if (userHasMobileNumber && userHasEmail)
                        {
                            model.UserHasEmailOnPdsRecord = userHasEmail;
                            model.UserHasMobileOnPdsRecord = userHasMobileNumber;
                            ModelState.AddModelError("verifyMethod", _immediatePassLocalizer["noSelection"]);
                            return View(model);
                        }

                        if (userHasMobileNumber)
                        {
                            model.verifyMethod = ImmediateVerifyMethod.phone;
                        }

                        if (userHasEmail)
                        {
                            model.verifyMethod = ImmediateVerifyMethod.email;
                        }
                    }

                    if (model.verifyMethod == ImmediateVerifyMethod.phone)
                    { if (postPdsUserJourneyForDigitalUserData != null)
                        {
                            postPdsUserJourneyForDigitalUserData.UserJourney.PostPdsDigitalUserJourney = PostPdsUserJourneyForDigitalUser.VerifyMobileNumberWithGpRecord;
                            TempData.Set(postPdsUserJourneyForDigitalUserData);
                        }
                        return RedirectToAction("VerifyMobile", UIConstants.Digital.DigitalController);
                    }
                    if (model.verifyMethod == ImmediateVerifyMethod.email)
                    {
                        if (postPdsUserJourneyForDigitalUserData != null)
                        {
                            postPdsUserJourneyForDigitalUserData.UserJourney.PostPdsDigitalUserJourney = PostPdsUserJourneyForDigitalUser.VerifyEmailWithGpRecord;
                            TempData.Set(postPdsUserJourneyForDigitalUserData);
                        }
                        return RedirectToAction("VerifyEmail", UIConstants.Digital.DigitalController);
                    }


                }
            }

            if (userSessionData.UserJourney != null)
            {
                /* User must persist on the LETTER journey */
                userSessionData.UserJourney.PrePdsJourney = InitUserJourney.Letter;
                TempData.Set(userSessionData);
            }

            return RedirectToAction("EligibilityForLetter", UIConstants.Outcome.OutcomeController);
        }

        [HttpGet]
        [Route(UIConstants.Outcome.LetterAlternativeType)]
        public IActionResult EligibilityForLetter()
        {
            var userSessionData = TempData.Get<UserSessionData>();

            if (userSessionData == null)
                return Redirect(_startPage);

            if (!UserIsPostPds())
                return RedirectToAction("CheckAnswers", UIConstants.Home.HomeController);

            if (
                (userSessionData!.UserJourney?.PrePdsJourney == InitUserJourney.Digital) ||
                (RegionServices.PostcodeIsInWales(userSessionData.Postcode?.Postcode))
                )
                return Redirect(_startPage);

            EligibilityForLetterViewModel model = new EligibilityForLetterViewModel()
            {
                RequestedLetterType = userSessionData.EligibilityForLetter?.RequestedLetterType
            };

            userSessionData.EligibilityForLetter = model;
            TempData.Set(userSessionData);

            ViewBag.userEligibleForImmediatePass = userSessionData.userEligibleForImmediatePass;
            return View(model);
        }

        [HttpPost]
        [Route(UIConstants.Outcome.LetterAlternativeType)]
        public IActionResult EligibilityForLetter(EligibilityForLetterViewModel model)
        {
            var userSessionData = TempData.Get<UserSessionData>();
            ViewBag.userEligibleForImmediatePass = userSessionData?.userEligibleForImmediatePass;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (userSessionData?.EligibilityForLetter != null)
            {
                userSessionData.EligibilityForLetter.RequestedLetterType = model.RequestedLetterType;
            }

            switch (model.RequestedLetterType)
            {
                case LetterTypes.Audio:
                    SetAccessibleAndAlternateLanguagesChoices(
                        userSessionData: userSessionData!,
                        accessibleType: "Audio",
                        accessibleTypeRequested: true,
                        anotherLanguageRequested: false
                    );
                    break;
                case LetterTypes.BigPrint:
                    SetAccessibleAndAlternateLanguagesChoices(
                        userSessionData: userSessionData!,
                        accessibleType: "Large print",
                        accessibleTypeRequested: true,
                        anotherLanguageRequested: false
                    );
                    break;
                case LetterTypes.Braille:
                    SetAccessibleAndAlternateLanguagesChoices(
                        userSessionData: userSessionData!,
                        accessibleType: "Braille",
                        accessibleTypeRequested: true,
                        anotherLanguageRequested: false
                    );
                    break;
                case LetterTypes.AnotherLanguage:
                    SetAccessibleAndAlternateLanguagesChoices(
                        userSessionData: userSessionData!,
                        accessibleType: UIConstants.NotRequestedValue,
                        accessibleTypeRequested: false,
                        anotherLanguageRequested: true
                    );

                    TempData.Set(userSessionData);
                    return RedirectToAction("Languages");
                case LetterTypes.None:
                    SetAccessibleAndAlternateLanguagesChoices(
                        userSessionData: userSessionData!,
                        accessibleType: UIConstants.NotRequestedValue,
                        accessibleTypeRequested: false,
                        anotherLanguageRequested: false
                    );
                    break;
            }

            TempData.Set(userSessionData);
            return RedirectToAction("ContactPreference");
        }

        /* Helper methods */
        private static void SetAccessibleAndAlternateLanguagesChoices(UserSessionData userSessionData, string accessibleType, bool accessibleTypeRequested, bool anotherLanguageRequested)
        {
            AlternateLetterTypeModel alternateLetterOptions = new AlternateLetterTypeModel()
            {
                UserRequestedAccessibleFormat = accessibleTypeRequested,
                UserRequestedAnotherLanguage = anotherLanguageRequested,
                AccessibleFormatType = accessibleType
            };

            userSessionData.AlternateLetterType = alternateLetterOptions;
            userSessionData.Languages = new LanguagesViewModel();
        }

        private LetterRequestMessage CreateCoreQueueMessage(
            UserSessionData userSessionData,
            SearchResultData searchResultData,
            string correlationId)
        {
            var validAddress = searchResultData.Addresses.First().Value;
            var validAddressLines = validAddress.Lines.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            return new LetterRequestMessage
            {
                NhsNumber = searchResultData.UserDetail.NHSNumber.Trim().Replace(" ", ""),
                Title = searchResultData.UserDetail.Title,
                FirstName = searchResultData.UserDetail.FirstName,
                LastName = searchResultData.UserDetail.LastName,
                DateOfBirth = searchResultData.UserDetail.DoB,
                Postcode = validAddress.PostalCode,
                AddressLine1 = validAddressLines.ElementAtOrDefault(0)!,
                AddressLine2 = validAddressLines.ElementAtOrDefault(1)!,
                AddressLine3 = validAddressLines.ElementAtOrDefault(2)!,
                AddressLine4 = validAddressLines.ElementAtOrDefault(3)!,
                AccessibilityNeeds = userSessionData.AlternateLetterType!.AccessibleFormatType!,
                AlternateLanguage = userSessionData.Languages!.GetLanguageOrDefault(),
                CorrelationId = correlationId,
                Region = RegionServices.GetRegionFromPostcode(userSessionData.Postcode?.Postcode),
                LetterType = new[] { "VaccineLetter" }
            };
        }

        private bool UserIsPostPds()
        {
            return TempData.Get<UserPdsStatusModel>()?.UserJourney.UserHasTravelledThroughPds != null && TempData.Get<UserPdsStatusModel>()!.UserJourney.UserHasTravelledThroughPds;
        }

        private static void SetUserLanguage(HttpResponse response, string languageCulture)
        {
            response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(languageCulture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), Secure = true }
            );
        }

        private static bool RequestIsForOver12(DateOfBirthViewModel dateOfBirthViewModel)
        {
            var dateOfBirth = dateOfBirthViewModel.ToDateTime();

            return DateTime.UtcNow.AddYears(-12).Date >= dateOfBirth.Date;
        }
    }
}