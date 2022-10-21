using System.Threading.Tasks;
using CovidLetter.Frontend.WebApp.Configuration;
using CovidLetter.Frontend.WebApp.Constants;
using CovidLetter.Frontend.WebApp.Extensions;
using CovidLetter.Frontend.WebApp.Models;
using CovidLetter.Frontend.WebApp.Services;
using CovidLetter.Frontend.WebApp.Services.Queue;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CovidLetter.Frontend.WebApp.Controllers
{
    public class DigitalController : Controller
    {
        private readonly ILogger<DigitalController> _logger;
        private readonly LetterRequestService _letterRequestService;
        private readonly DigitalModelService _digitalModelService;
        private readonly string _startPage;
        private readonly SiteConfiguration _siteConfiguration;
        private readonly IStringLocalizer<ContactPreferenceViewModel> _contactPreferenceLocalizer;
        private readonly IStringLocalizer<VerifyOtpViewModel> _verifyOtpLocalizer;
        private readonly IOtpService _otpService;
        private readonly IStringLocalizer<InputEmailViewModel> _inputEmailLocalizer;
        private readonly IStringLocalizer<DigitalContactPreferenceViewModel> _digitalContactPreferenceViewModelLocalizer;
        private readonly IStringLocalizer<VerifyEmailViewModel> _verifyEmailLocalizer;

        public DigitalController(
            ILogger<DigitalController> logger,
            LetterRequestService letterRequestService,
            IOptions<SiteConfiguration> siteConfiguration,
            OutcomeModelService outcomeModelService,
            IStringLocalizer<ContactPreferenceViewModel> contactPreferenceLocalizer,
            IOtpService otpService,
            IStringLocalizer<VerifyOtpViewModel> verifyOtpLocalizer,
            DigitalModelService digitalModelService,
            IStringLocalizer<InputEmailViewModel> inputEmailLocalizer,
            IStringLocalizer<DigitalContactPreferenceViewModel> digitalContactPreferenceViewModelLocalizer,
            IStringLocalizer<VerifyEmailViewModel> verifyEmailLocalizer)
        {
            _logger = logger;
            _letterRequestService = letterRequestService;
            _digitalModelService = digitalModelService;
            _startPage = siteConfiguration.Value.StartPage;
            _siteConfiguration = siteConfiguration.Value;
            _contactPreferenceLocalizer = contactPreferenceLocalizer;
            _inputEmailLocalizer = inputEmailLocalizer;
            _digitalContactPreferenceViewModelLocalizer = digitalContactPreferenceViewModelLocalizer;
            _verifyEmailLocalizer = verifyEmailLocalizer;
            _otpService = otpService;
            _verifyOtpLocalizer = verifyOtpLocalizer;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewBag.IsDigital = true;
        }

        [HttpGet]
        [Route(UIConstants.Digital.EmailOrPhoneNumberChoice)]
        public IActionResult EmailOrPhoneNumberChoice()
        {
            var userSessionData = TempData.Get<UserSessionData>();
            var postPdsJourney = TempData.Get<PostPdsJourneyData>()?.UserJourney.PostPdsDigitalUserJourney;
            var userHasEmailAndPhoneNumber = TempData.Get<PostPdsJourneyData>()?.UserJourney.userHasEmailAndPhoneNumber;
            if (userSessionData == null)
                return Redirect(_startPage);

            if (!userIsPostPds())
                return RedirectToAction("CheckAnswers", UIConstants.Home.HomeController);

            if (postPdsJourney == null)
                return Redirect(_startPage);

            if (userHasEmailAndPhoneNumber != true)
            {
                if (postPdsJourney == PostPdsUserJourneyForDigitalUser.VerifyEmailWithGpRecord)
                    return RedirectToAction(nameof(DigitalController.VerifyEmail));

                if (postPdsJourney == PostPdsUserJourneyForDigitalUser.VerifyMobileNumberWithGpRecord)
                    return RedirectToAction(nameof(DigitalController.VerifyMobile));
            }

            userSessionData!.UserJourney!.PostPdsDigitalUserJourney = postPdsJourney;

            var model = new EmailOrPhoneNumberChoiceViewModel()
            {
                verifyMethod = VerifyMethod.Email
            };

            TempData.Set(userSessionData);
            return View(model);
        }

        [HttpPost]
        [Route(UIConstants.Digital.EmailOrPhoneNumberChoice)]
        public IActionResult WhoAreYouRequestingFor(EmailOrPhoneNumberChoiceViewModel model)
        {
            var userSessionData = TempData.Get<UserSessionData>();
            if (userSessionData == null)
                return Redirect(_siteConfiguration.StartPage);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            userSessionData.EmailOrPhoneNumberChoice = model;

            TempData.Set(userSessionData);

            var postPdsJourneyData = TempData.Get<PostPdsJourneyData>();
            var digitalUserJourney = postPdsJourneyData?.UserJourney.PostPdsDigitalUserJourney;
            if (postPdsJourneyData != null)
            {
                if (userSessionData.EmailOrPhoneNumberChoice.verifyMethod == VerifyMethod.PhoneNumber)
                {
                    postPdsJourneyData.UserJourney.PostPdsDigitalUserJourney = PostPdsUserJourneyForDigitalUser.VerifyMobileNumberWithGpRecord;
                    TempData!.Set(postPdsJourneyData);
                    return RedirectToAction(actionName: nameof(DigitalController.VerifyMobile));
                }
                postPdsJourneyData.UserJourney.PostPdsDigitalUserJourney = PostPdsUserJourneyForDigitalUser.VerifyEmailWithGpRecord;
                TempData!.Set(postPdsJourneyData);
            }

            return RedirectToAction(actionName: nameof(DigitalController.VerifyEmail));
        }


        [HttpGet]
        [Route(UIConstants.Digital.OtpVerifyMobile)]
        public IActionResult VerifyMobile()
        {
            var userSessionData = TempData.Get<UserSessionData>();
            var postPdsJourney = TempData.Get<PostPdsJourneyData>()?.UserJourney.PostPdsDigitalUserJourney;

            if (userSessionData == null)
                return Redirect(_startPage);
            
            if (!userIsPostPds())
                return RedirectToAction("CheckAnswers", UIConstants.Home.HomeController);

            if (postPdsJourney == null)
                return Redirect(_startPage);
            
            if (postPdsJourney == PostPdsUserJourneyForDigitalUser.VerifyEmailWithGpRecord)
                return RedirectToAction(nameof(DigitalController.VerifyEmail));

            if (userSessionData.UserJourney != null)
            {
                userSessionData.UserJourney.PostPdsDigitalUserJourney = postPdsJourney;
            }

            if (userSessionData.remainingMobileAttempts == null)
            {
                userSessionData.remainingMobileAttempts = UIConstants.Digital.InitialAllowedAttempts;
            }

            var model = new VerifyMobileViewModel() 
            {
                MobileNumber = userSessionData.VerifyMobile?.MobileNumber
            };

            model.RemainingMobileAttempts = userSessionData.remainingMobileAttempts;

            TempData.Set(userSessionData);
            ViewBag.defaultAttempts = UIConstants.Digital.InitialAllowedAttempts;
            return View(model);
        }

        [HttpPost]
        [Route(UIConstants.Digital.OtpVerifyMobile)]
        public async Task<IActionResult> VerifyMobile(VerifyMobileViewModel model)
        {
            ViewBag.defaultAttempts = UIConstants.Digital.InitialAllowedAttempts;

            var userSessionData = TempData.Get<UserSessionData>();
            if (userSessionData == null)
                return Redirect(_siteConfiguration.StartPage);

            if (!ModelState.IsValid)
            {
                if (userSessionData.remainingMobileAttempts == null || userSessionData.remainingMobileAttempts == 0)
                {
                    return RedirectToAction(nameof(DigitalController.MaximumAttemptsReached));
                }
                model.RemainingMobileAttempts = userSessionData.remainingMobileAttempts;
                return View(model);
            } 
            
            userSessionData.remainingMobileAttempts = UIConstants.Digital.InitialAllowedAttempts;
            userSessionData.VerifyMobile = model;
            
            TempData.Set(userSessionData);
            RequestOtpResult result;

            result = await _otpService.RequestOtp(userSessionData.VerifyMobile.MobileNumber, TempData.GetCorrelationId());
            return result.Accept(new RequestOtpResultVisitor(TempData, _logger));
        }

        [HttpGet]
        [Route(UIConstants.Digital.MaximumAttemptsReached)]
        public IActionResult MaximumAttemptsReached()
        {
            var userSessionData = TempData.Get<UserSessionData>();

            if (userSessionData == null)
                return Redirect(_startPage);
            
            if (!userIsPostPds())
                return RedirectToAction("CheckAnswers", UIConstants.Home.HomeController);
            
            if (userSessionData!.UserJourney?.PrePdsJourney == InitUserJourney.Letter)
                return Redirect(_startPage);

            TempData.Clear();
            return View();
        }

        [HttpGet]
        [Route(UIConstants.Digital.MaximumOtpAttemptsReached)]
        public IActionResult MaximumOtpAttemptsReached()
        {
            var userSessionData = TempData.Get<UserSessionData>();
            
            if (userSessionData == null)
                return Redirect(_startPage);

            if (!userIsPostPds())
                return RedirectToAction("CheckAnswers", UIConstants.Home.HomeController);

            if (userSessionData!.UserJourney?.PrePdsJourney == InitUserJourney.Letter)
                return Redirect(_startPage);

            TempData.Clear();
            ViewBag.defaultAttempts = UIConstants.Digital.InitialAllowedOtpAttempts;

            return View();
        }


        [HttpGet]
        [Route(UIConstants.Digital.MaximumOtpsGenerated)]
        public IActionResult MaximumOtpsGenerated()
        {
            var userSessionData = TempData.Get<UserSessionData>();

            if (userSessionData == null)
                return Redirect(_startPage);

            if (!userIsPostPds())
                return RedirectToAction("CheckAnswers", UIConstants.Home.HomeController);

            if (userSessionData!.UserJourney?.PrePdsJourney == InitUserJourney.Letter)
                return Redirect(_startPage);

            TempData.Clear();
            ViewBag.maximumAllowedOtpGenerations = UIConstants.Digital.MaximumAllowedOtpGenerations;
            return View();
        }

        [HttpGet]
        [Route(UIConstants.Digital.RequestNewOtp)]
        public IActionResult RequestNewOtpCode()
        {
            var userSessionData = TempData.Get<UserSessionData>();
            var postPdsJourney = TempData.Get<PostPdsJourneyData>()?.UserJourney.PostPdsDigitalUserJourney;

            if (userSessionData == null)
                return Redirect(_startPage);

            if (!userIsPostPds())
                return RedirectToAction("CheckAnswers", UIConstants.Home.HomeController);

            if (userSessionData!.UserJourney?.PrePdsJourney == InitUserJourney.Letter)
                return Redirect(_startPage);

            if (postPdsJourney == PostPdsUserJourneyForDigitalUser.VerifyEmailWithGpRecord)
                return RedirectToAction(nameof(DigitalController.VerifyEmail));

            return View();
        }

        [HttpPost]
        [Route(UIConstants.Digital.RequestNewOtp)]
        public async Task<IActionResult> RequestNewOtp()
        {
            var userSessionData = TempData.Get<UserSessionData>();

            if (userSessionData == null)
                return Redirect(_siteConfiguration.StartPage);

            RequestOtpResult result;
            result = await _otpService.RequestOtp(userSessionData!.VerifyMobile!.MobileNumber, TempData.GetCorrelationId());

            return result.Accept(new RequestOtpResultVisitor(TempData, _logger));
        }

        [HttpGet]
        [Route(UIConstants.Digital.OtpVerifyCode)]
        public IActionResult VerifyOtp()
        {
            var userSessionData = TempData.Get<UserSessionData>();
            var postPdsJourney = TempData.Get<PostPdsJourneyData>()?.UserJourney.PostPdsDigitalUserJourney;

            if (userSessionData == null)
                return Redirect(_siteConfiguration.StartPage);
            
            if (!userIsPostPds())
                return RedirectToAction("CheckAnswers", UIConstants.Home.HomeController);

            if (postPdsJourney == null)
                return Redirect(_startPage);
            
            if (postPdsJourney == PostPdsUserJourneyForDigitalUser.VerifyEmailWithGpRecord)
                return RedirectToAction(nameof(DigitalController.VerifyEmail));

            if (userSessionData.remainingOtpAttempts == null)
            {
                userSessionData.remainingOtpAttempts = UIConstants.Digital.InitialAllowedOtpAttempts;
            }

            var model = new VerifyOtpViewModel()
            {
                OtpCode = userSessionData.VerifyOtp?.OtpCode,
            };

            userSessionData.VerifyOtp = model;

            model.RemainingOtpAttempts = userSessionData.remainingOtpAttempts;

            TempData.Set(userSessionData);

            ViewBag.defaultAttempts = UIConstants.Digital.InitialAllowedOtpAttempts;

            return View(model);
        }

        [HttpPost]
        [Route(UIConstants.Digital.OtpVerifyCode)]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            var userSessionData = TempData.Get<UserSessionData>();

            if (userSessionData == null)
                return Redirect(_siteConfiguration.StartPage);

            userSessionData.VerifyOtp = model;
            ViewBag.defaultAttempts = UIConstants.Digital.InitialAllowedOtpAttempts;

            if (!ModelState.IsValid)
            {
                if (userSessionData.remainingOtpAttempts == null || userSessionData.remainingOtpAttempts == 0)
                {
                    return RedirectToAction(nameof(DigitalController.MaximumAttemptsReached));
                }
                model.RemainingOtpAttempts = userSessionData.remainingOtpAttempts;

                return View(model);
            }

            var request = new VerifyOtpMatchParams(userSessionData!.VerifyMobile!.MobileNumber, userSessionData!.VerifyOtp!.OtpCode);
            VerifyOtpResult result = await _otpService.VerifyOtpMatch(request, TempData.GetCorrelationId());

            if (result.otpSuccessfulMatch)
            {
                userSessionData.otpMatchSuccess = true;
                userSessionData.remainingOtpAttempts = ViewBag.defaultAttempts;

                TempData.Set(userSessionData);
                return RedirectToAction(nameof(DigitalController.EmailOtpInput));
            }
            if (result.expiredOtp == true)
            {
                ModelState.AddModelError(key: nameof(model.OtpCode), errorMessage: _verifyOtpLocalizer["expiredOtp"]);
                return View(model);
            }
            if (result.remainingAttempts > 0)   
            {
                ModelState.AddModelError(key: nameof(model.OtpCode), errorMessage: _verifyOtpLocalizer["noneMatchingOtp"]);

                userSessionData.remainingOtpAttempts = result.remainingAttempts;
                model.RemainingOtpAttempts = result.remainingAttempts;

                TempData.Set(userSessionData);
                return View(model);
            }

            return RedirectToAction(nameof(DigitalController.MaximumOtpAttemptsReached));
        }

        [HttpGet]
        [Route(UIConstants.Digital.VerifyEmail)]
        public IActionResult VerifyEmail()
        {
            var userSessionData = TempData.Get<UserSessionData>();
            var postPdsJourney = TempData.Get<PostPdsJourneyData>()?.UserJourney.PostPdsDigitalUserJourney;

            if (userSessionData == null)
                return Redirect(_startPage);
            
            if (!userIsPostPds())
                return RedirectToAction("CheckAnswers", UIConstants.Home.HomeController);

            if (postPdsJourney == null)
                return Redirect(_startPage);

            if (postPdsJourney == PostPdsUserJourneyForDigitalUser.VerifyMobileNumberWithGpRecord)
                return RedirectToAction(nameof(DigitalController.VerifyMobile));

            /* LETTER user who is eligible for a digital pass should not be able to access this page until they have decided to uplift
               (i.e. selected the 'YES' radio button on the ImmediatePass page) */
            var verifyMethod = userSessionData.ImmediatePass?.verifyMethod;
            var userHasUpliftedFromLetter = verifyMethod == ImmediateVerifyMethod.phone || verifyMethod == ImmediateVerifyMethod.email;
            if (!userHasUpliftedFromLetter && (userSessionData.UserJourney == null || userSessionData!.UserJourney.PrePdsJourney == InitUserJourney.Letter))
                return Redirect(_startPage);

            userSessionData!.UserJourney!.PostPdsDigitalUserJourney = postPdsJourney;

            if (userSessionData.remainingEmailAttempts == null)
            {
                userSessionData.remainingEmailAttempts = UIConstants.Digital.InitialAllowedAttempts;
            }

            var model = new VerifyEmailViewModel
            {
                EmailAddress = userSessionData.VerifyEmail?.EmailAddress
            };

            TempData.Set(userSessionData);
            model.RemainingEmailAttempts = userSessionData.remainingEmailAttempts;
            ViewBag.defaultAttempts = UIConstants.Digital.InitialAllowedAttempts;
            return View(model);
        }

        [HttpPost]
        [Route(UIConstants.Digital.VerifyEmail)]
        public IActionResult VerifyEmail(VerifyEmailViewModel model)
        {
            var userSessionData = TempData.Get<UserSessionData>();
            var contactDetailsData = TempData.Get<SearchResultData>();

            if (userSessionData == null || contactDetailsData == null)
                return Redirect(_startPage);

            ViewBag.defaultAttempts = UIConstants.Digital.InitialAllowedAttempts;

            if (ModelState.IsValid)
            {
                /* no match against the confirmation field */
                if (model.EmailAddress != model.EmailAddressConfirmation)
                {
                    ModelState.AddModelError("EmailAddress", _verifyEmailLocalizer["validationProvidedEmailsNoMatch"]);
                    ModelState.AddModelError("EmailAddress", _verifyEmailLocalizer["validationWrongFormat"]);
                    /* default placeholder to be removed from the 'there is a problem' box - see _NhsErrorSummary.cshtml shared view */
                    ModelState.AddModelError("EmailAddressConfirmation", "invalid-hidden-field-remove-label");
                    return View(model);
                }

                /* no match against PDS */
                if (!contactDetailsData.Emails.ContainsValue(model.EmailAddress))
                {
                    ModelState.AddModelError("EmailAddress", _verifyEmailLocalizer["validationPDSNoMatch"]);
                    ModelState.AddModelError("EmailAddressConfirmation", "invalid-hidden-field-remove-label");

                    if (userSessionData.remainingEmailAttempts != null)
                        userSessionData.remainingEmailAttempts--;

                    if (userSessionData.remainingEmailAttempts == null || userSessionData.remainingEmailAttempts == 0)
                    {
                        return RedirectToAction(nameof(DigitalController.MaximumAttemptsReached));
                    }

                    model.RemainingEmailAttempts = userSessionData.remainingEmailAttempts;
                    
                    TempData.Set(userSessionData);
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }

            userSessionData.remainingEmailAttempts = UIConstants.Digital.InitialAllowedAttempts;
            userSessionData.VerifyEmail = model;

            TempData.Set(userSessionData);
            return RedirectToAction(nameof(DigitalController.DigitalContactPreference));
        }

        [HttpGet]
        [Route(UIConstants.Digital.EmailInput)]
        public IActionResult EmailOtpInput()
        {
            var userSessionData = TempData.Get<UserSessionData>();
            var postPdsJourney = TempData.Get<PostPdsJourneyData>()?.UserJourney.PostPdsDigitalUserJourney;

            if (userSessionData == null)
                return Redirect(_startPage);
            
            if (!userIsPostPds())
                return RedirectToAction("CheckAnswers", UIConstants.Home.HomeController);

            if ((userSessionData.otpMatchSuccess == null || userSessionData.otpMatchSuccess == false) && postPdsJourney == PostPdsUserJourneyForDigitalUser.VerifyMobileNumberWithGpRecord)
                return RedirectToAction(nameof(DigitalController.VerifyMobile));

            if (postPdsJourney == PostPdsUserJourneyForDigitalUser.VerifyEmailWithGpRecord)
                return RedirectToAction(nameof(DigitalController.VerifyEmail));

            if (userSessionData.otpMatchSuccess == null || userSessionData.otpMatchSuccess == false)
                return Redirect(_startPage);

            var model = new InputEmailViewModel
            {
                EmailAddress = userSessionData.VerifyEmailAddress?.EmailAddress
            };

            return View(model);
        }

        [HttpPost]
        [Route(UIConstants.Digital.EmailInput)]
        public IActionResult EmailOtpInput(InputEmailViewModel model)
        {
            var userSessionData = TempData.Get<UserSessionData>();
            var contactDetailsData = TempData.Get<SearchResultData>();

            if (userSessionData == null || contactDetailsData == null)
                return Redirect(_startPage);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            /* no match against the confirmation field */
            if (model.EmailAddress != model.EmailAddressConfirm)
            {
                ModelState.AddModelError("EmailAddress", _inputEmailLocalizer["doNotMatch"]);
                ModelState.AddModelError("EmailAddressConfirm", "invalid-hidden-field-remove-label");

                return View(model);
            }

            userSessionData.VerifyEmailAddress = model;
            TempData.Set(userSessionData);

            return RedirectToAction(nameof(DigitalController.DigitalContactPreference));
        }

        [HttpGet]
        [Route(UIConstants.Digital.HowToContact)]
        public IActionResult DigitalContactPreference()
        {
            var userSessionData = TempData.Get<UserSessionData>();
            var contactDetailsData = TempData.Get<SearchResultData>();

            if (userSessionData == null || contactDetailsData == null)
                return Redirect(_startPage);

            if (!userIsPostPds())
                return RedirectToAction("CheckAnswers", UIConstants.Home.HomeController);

            if (userSessionData!.UserJourney == null || (userSessionData.UserJourney.PrePdsJourney == InitUserJourney.Letter) ||
                (userSessionData.UserJourney.PostPdsDigitalUserJourney == PostPdsUserJourneyForDigitalUser.VerifyEmailWithGpRecord && userSessionData.VerifyEmail == null) ||
                (userSessionData.UserJourney.PostPdsDigitalUserJourney == PostPdsUserJourneyForDigitalUser.VerifyMobileNumberWithGpRecord && (userSessionData.VerifyMobile == null || userSessionData.otpMatchSuccess == false || userSessionData.VerifyEmailAddress == null))
                )
                return Redirect(_startPage);

            var model = new DigitalContactPreferenceViewModel();
            _digitalModelService.PopulateViewModelWithObfuscatedDigitalContactDetails(model, userSessionData);

            var postPdsJourney = userSessionData.UserJourney.PostPdsDigitalUserJourney;

            if (postPdsJourney == null)
                return Redirect(_startPage);
            
            if (postPdsJourney == PostPdsUserJourneyForDigitalUser.VerifyEmailWithGpRecord)
                ViewBag.BackLink = Url.Action("VerifyEmail", UIConstants.Digital.DigitalController);
            else
                ViewBag.BackLink = Url.Action("EmailOtpInput", UIConstants.Digital.DigitalController);
            
            return View(model);
        }

        [HttpPost]
        [Route(UIConstants.Digital.HowToContact)]
        public async Task<IActionResult> DigitalContactPreference(DigitalContactPreferenceViewModel model)
        {
            var userSessionData = TempData.Get<UserSessionData>();
            var contactDetailsData = TempData.Get<SearchResultData>();

            if (userSessionData == null || contactDetailsData == null)
                return Redirect(_siteConfiguration.StartPage);

            if (!ModelState.IsValid)
            {
                model = new DigitalContactPreferenceViewModel();
                _digitalModelService.PopulateViewModelWithObfuscatedDigitalContactDetails(model, userSessionData);

                var postPdsJourney = userSessionData!.UserJourney?.PostPdsDigitalUserJourney;

                if (postPdsJourney == null)
                    return Redirect(_startPage);

                if (postPdsJourney == PostPdsUserJourneyForDigitalUser.VerifyEmailWithGpRecord)
                    ViewBag.BackLink = Url.Action("VerifyEmail", UIConstants.Digital.DigitalController);
                else
                    ViewBag.BackLink = Url.Action("EmailOtpInput", UIConstants.Digital.DigitalController);

                return View(model);
            }

            var emailInput = userSessionData.VerifyEmailAddress?.EmailAddress != null ? userSessionData.VerifyEmailAddress?.EmailAddress : userSessionData.VerifyEmail?.EmailAddress;
            var phoneInput = userSessionData.VerifyMobile?.MobileNumber;

            var correlationId = TempData.GetCorrelationId();
            var queueMessage = CreatePdfQueueMessage(
                contactDetailsData,
                emailInput ?? string.Empty,
                phoneInput ?? string.Empty,
                correlationId);

            if (model.Selection == DigitalContactPreferenceOptions.NoContact)
            {
                queueMessage.ContactMethodSettable = ContactMethodEnum.DoNotContact;

                await _letterRequestService.SendPDFRequest(queueMessage);
            }
            else if (model.Selection == DigitalContactPreferenceOptions.Email)
            {
                queueMessage.ContactMethodSettable = ContactMethodEnum.Email;

                await _letterRequestService.SendPDFRequest(queueMessage);
            }
            else if (model.Selection == DigitalContactPreferenceOptions.Phone)
            {
                queueMessage.ContactMethodSettable = ContactMethodEnum.SMS;

                await _letterRequestService.SendPDFRequest(queueMessage);
            }

            userSessionData.DigitalContactPreference = model;
            TempData.Set(userSessionData);

            return RedirectToAction(nameof(DigitalController.DigitalSubmitted));
        }

        [HttpGet]
        [Route(UIConstants.Digital.Submitted)]
        public IActionResult DigitalSubmitted()
        {
            var userSessionData = TempData.Get<UserSessionData>();

            if (userSessionData == null)
                return Redirect(_startPage);
            
            if (!userIsPostPds())
                return RedirectToAction("CheckAnswers", UIConstants.Home.HomeController);
            
            if (userSessionData.DigitalContactPreference == null || userSessionData.DigitalContactPreference.Selection == null || userSessionData.UserJourney == null || userSessionData.UserJourney.PrePdsJourney == InitUserJourney.Letter)
                return Redirect(_startPage);

            TempData.Clear();
            var model = new SubmittedModel
            {
                Is16OrOver = !UserServices.UserIsUnder16(userSessionData.DateOfBirth!.Day, userSessionData.DateOfBirth.Month, userSessionData.DateOfBirth.Year)
            };
            return View(model);
        }

        [HttpGet]
        [Route(UIConstants.Digital.UserFoundWithoutContactDetails)]
        public IActionResult NoContactDetailsFound()
        {
            var userSessionData = TempData.Get<UserSessionData>();

            if (userSessionData == null)
                return Redirect(_startPage);

            if (!userIsPostPds())
                return RedirectToAction("CheckAnswers", UIConstants.Home.HomeController);

            if (userSessionData.UserJourney == null || userSessionData.UserJourney.PrePdsJourney == InitUserJourney.Letter)
                return Redirect(_startPage);

            ViewBag.IsDigital = userSessionData.UserJourney.PrePdsJourney == InitUserJourney.Digital;

            TempData.Clear();
            return View();
        }

        [HttpGet]
        [Route(UIConstants.Digital.UserNotEligibleForDigitalFlow)]
        public IActionResult UserNotEligibleForDigitalFlow()
        {
            var userSessionData = TempData.Get<UserSessionData>();

            if (userSessionData == null)
                return Redirect(_startPage);

            if (!userIsPostPds())
                return RedirectToAction("CheckAnswers", UIConstants.Home.HomeController);

            if (userSessionData.UserJourney == null || userSessionData.UserJourney.PrePdsJourney == InitUserJourney.Letter)
                return Redirect(_startPage);

            ViewBag.IsDigital = userSessionData.UserJourney.PrePdsJourney == InitUserJourney.Digital;

            TempData.Clear();
            return View();
        }

        /* Helper methods */
        private UnattendedPdfRequest CreatePdfQueueMessage(SearchResultData searchResultData, string emailToSendTo, string mobileNumber, string correlationId)
        {
            Identifier i = new();
            i.System = "NHS-number";
            i.Value = searchResultData.UserDetail.NHSNumber.Trim().Replace(" ", "");

            HumanName n = new();
            n.Family = searchResultData.UserDetail.LastName;
            FhirString givenName = new(searchResultData.UserDetail.FirstName);
            n.GivenElement.Add(givenName);

            Patient p = new();
            p.Identifier.Add(i);
            p.Name.Add(n);
            p.BirthDate = searchResultData.UserDetail.DoB;

            FhirJsonSerializer fhirJsonSerializer = new FhirJsonSerializer();
            var pString = fhirJsonSerializer.SerializeToString(p);
            return new UnattendedPdfRequest(pString, emailToSendTo,mobileNumber, correlationId);
        }

        private bool userIsPostPds()
        {
            return TempData.Get<UserPdsStatusModel>()?.UserJourney.UserHasTravelledThroughPds != null ? TempData.Get<UserPdsStatusModel>()!.UserJourney.UserHasTravelledThroughPds : false;
        }
    }
}
