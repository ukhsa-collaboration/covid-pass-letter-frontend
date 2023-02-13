using System;
using System.Linq;
using CovidLetter.Frontend.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using CovidLetter.Frontend.Extensions;
using CovidLetter.Frontend.Logging;
using CovidLetter.Frontend.Search;
using CovidLetter.Frontend.WebApp.Configuration;
using CovidLetter.Frontend.WebApp.Constants;
using CovidLetter.Frontend.WebApp.Extensions;
using CovidLetter.Frontend.WebApp.Filters;
using CovidLetter.Frontend.WebApp.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;

namespace CovidLetter.Frontend.WebApp.Controllers
{
    [TypeFilter(typeof(ValidSessionCookieAsyncActionFilter))]
    public class HomeController : Controller
    {
        private static Lazy<int> _sessionExpiresMinutes = new();
        
        private readonly ILogger<HomeController> _logger;
        private readonly ISearchPatientService _searchPatientService;
        private readonly SiteConfiguration _siteConfiguration;
        private readonly IOptions<RequestLocalizationOptions> _localizationOptions;
        private readonly IUrlHelper _urlHelper;

        public HomeController(
            ILogger<HomeController> logger,
            IOptions<AuthenticationConfiguration> authenticationConfigurationOptions,
            ISearchPatientService searchPatientService,
            IOptions<SiteConfiguration> siteConfiguration, 
            IOptions<RequestLocalizationOptions> localizationOptions,
            IUrlHelper urlHelper
        ) {
            _logger = logger;

            if (!_sessionExpiresMinutes.IsValueCreated)
            {
                _sessionExpiresMinutes = new Lazy<int>(authenticationConfigurationOptions.Value.ExpireMinutes);
            }

            _searchPatientService = searchPatientService;
            _localizationOptions = localizationOptions;
            _siteConfiguration = siteConfiguration.Value;
            _urlHelper = urlHelper;
        }

        [HttpGet]
        [Route(UIConstants.Home.InitialRoute)]
        public IActionResult Index()
        {
            // initialise CorrelationID for telemetry purposes
            TempData.GetCorrelationId();

            string returnUrl = QueryHelpers.AddQueryString(_siteConfiguration.StartPage, Request.Query);
            string[] allowList = {"?service=digital", "service=letter"};

            if (_urlHelper.IsLocalUrl(returnUrl) && (Array.IndexOf(allowList, Request.Query) > -1))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Redirect(_siteConfiguration.StartPage);
            }

        }

        /// <param name="serviceBusy">Determine whether to display the warning message if downstream services too busy</param>
        /// <param name="isCoronavirusHelplineUser">Indicate whether the request is from 119</param>
        [HttpGet]
        [Route(UIConstants.Home.RequestLetterForTravelRoute)]
        public IActionResult RequestLetterForTravel(
            bool serviceBusy = false,
            [FromQuery(Name = QueryStringKeys.IsCoronavirusHelplineUser)]
            bool isCoronavirusHelplineUser = false
        ) {
            if (isCoronavirusHelplineUser)
            {
                TempData.Set(new CoronavirusHelplineSessionData());
            }
            
            var model = new RequestLetterForTravelModel();
            if (serviceBusy)
            {
                TempData.Clear();
                TempData.Set(new UserSessionData());
                model.IsServiceBusy = true;
            }
            else
            {
                InitialiseSessionStorage(TempData.Get<UserSessionData>(), TempData);
            }

            var userSessionData = TempData.Get<UserSessionData>();
            if (userSessionData != null)
            {
                userSessionData.isCoronavirusHelplineUser = isCoronavirusHelplineUser;
            }

            ViewBag.IsCoronavirusHelplineUser = isCoronavirusHelplineUser;
            TempData.Set(userSessionData);

            return View(model);
        }

        [HttpPost]
        [Route(UIConstants.Home.RequestLetterForTravelRoute)]
        public IActionResult RequestLetterForTravel(RequestLetterForTravelModel model)
        {
            var userSessionData = TempData.Get<UserSessionData>();
            if (userSessionData == null)
            {
                return Redirect(_siteConfiguration.StartPage);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            userSessionData.DateOfBirth = null;

            TempData.Set(userSessionData);

            return RedirectToAction("WhoAreYouRequestingFor");
        }

        [HttpGet]
        [Route(UIConstants.Home.WhoAreYouRequestingForRoute)]
        public IActionResult WhoAreYouRequestingFor()
        {
            InitialiseSessionStorage(TempData.Get<UserSessionData>(), TempData);
            var userSessionData = TempData.Get<UserSessionData>();

            string userJourney = Request.Query["service"].ToString().ToLower();
            userSessionData.UserJourney = new UserJourneyModel();

            switch (userJourney)
            {
                case "digital":
                    userSessionData.UserJourney.PrePdsJourney = InitUserJourney.Digital;
                    break;
                default:
                    userSessionData.UserJourney.PrePdsJourney = InitUserJourney.Letter;
                    break;
            }

            var httpRequestCookies = Request.Cookies;
            var httpLanguageCookie = Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];

            if (httpRequestCookies != null && httpLanguageCookie != null)
            {
                var currentLanguageCultureIsWelsh = httpLanguageCookie.ToString().Contains(UIConstants.WelshLanguageCulture);
                if (currentLanguageCultureIsWelsh && userJourney == "digital")
                {
                    setUserLanguage(Response, UIConstants.EnglishLanguageCulture);
                    /* user is redirected to the same page to force a view refresh which will re-draw the correct language */
                    return RedirectToAction("WhoAreYouRequestingFor", new { service = "digital" });
                }
            }

            TempData.Set(userSessionData);

            var model = new WhoAreYouRequestingForModel
            {
                RequestingFor = userSessionData.RequestingFor?.RequestingFor,
                IsParentOrGuardian = userSessionData.RequestingFor?.IsParentOrGuardian ?? false
            };

            ViewBag.IsDigital = userJourney == "digital";
            ViewBag.IsCoronavirusHelplineUser = GetIsCoronavirusHelplineUser(userSessionData);
            return View(model);
        }

        [HttpPost]
        [Route(UIConstants.Home.WhoAreYouRequestingForRoute)]
        public IActionResult WhoAreYouRequestingFor(WhoAreYouRequestingForModel model)
        {
            var userSessionData = TempData.Get<UserSessionData>();
            if (userSessionData == null)
                return Redirect(_siteConfiguration.StartPage);

            if (!ModelState.IsValid)
            {
                ViewBag.IsDigital = GetIsDigitalRoute(userSessionData);
                ViewBag.IsCoronavirusHelplineUser = GetIsCoronavirusHelplineUser(userSessionData);
                return View(model);
            }

            userSessionData.RequestingFor = model;
            userSessionData.DateOfBirth = null;
            TempData.Set(userSessionData);

            return RedirectToAction("DateOfBirth");
        }
        
        [HttpGet]
        [Route(UIConstants.Home.NhsNumber)]
        public IActionResult NhsNumber()
        {
            var userSessionData = TempData.Get<UserSessionData>();

            if (userSessionData == null)
                return Redirect(_siteConfiguration.StartPage);

            var model = new NhsNumberViewModel
            {
                KnowsNhsNumber = userSessionData.NhsNumber?.KnowsNhsNumber,
                NhsNumber = userSessionData.NhsNumber?.NhsNumber
            };

            ViewBag.IsDigital = GetIsDigitalRoute(userSessionData);
            ViewBag.IsCoronavirusHelplineUser = GetIsCoronavirusHelplineUser(userSessionData);

            return View(model);
        }

        [HttpPost]
        [Route(UIConstants.Home.NhsNumber)]
        public IActionResult NhsNumber(NhsNumberViewModel model)
        {
            var userSessionData = TempData.Get<UserSessionData>();
            if (userSessionData == null)
                return Redirect(_siteConfiguration.StartPage);

            if (!ModelState.IsValid)
            {
                ViewBag.IsDigital = GetIsDigitalRoute(userSessionData);
                ViewBag.IsCoronavirusHelplineUser = GetIsCoronavirusHelplineUser(userSessionData);
                return View(model);
            }

            userSessionData.NhsNumber = model;
            TempData.Set(userSessionData);

            if (model.KnowsNhsNumber.GetValueOrDefault())
                return RedirectToAction("Postcode");

            userSessionData.NhsNumber.NhsNumber = "";
            userSessionData.Name = null;
            TempData.Set(userSessionData);

            return RedirectToAction("Name");
        }

        [HttpGet]
        [Route(UIConstants.Home.Name)]
        public IActionResult Name()
        {
            var userSessionData = TempData.Get<UserSessionData>();

            if (userSessionData == null)
                return Redirect(_siteConfiguration.StartPage);

            var model = new NameViewModel
            {
                FirstName = userSessionData.Name?.FirstName,
                LastName = userSessionData.Name?.LastName
            };

            ViewBag.IsDigital = GetIsDigitalRoute(userSessionData);
            ViewBag.IsCoronavirusHelplineUser = GetIsCoronavirusHelplineUser(userSessionData);

            return View(model);
        }

        [HttpPost]
        [Route(UIConstants.Home.Name)]
        public IActionResult Name(NameViewModel model)
        {
            var userSessionData = TempData.Get<UserSessionData>();
            if (userSessionData == null)
                return Redirect(_siteConfiguration.StartPage);

            if (!ModelState.IsValid)
            {
                ViewBag.IsDigital = GetIsDigitalRoute(userSessionData);
                ViewBag.IsCoronavirusHelplineUser = GetIsCoronavirusHelplineUser(userSessionData);
                return View(model);
            }

            userSessionData.Name = model;
            TempData.Set(userSessionData);

            return RedirectToAction("Postcode");
        }

        [HttpGet]
        [Route(UIConstants.Home.DateOfBirth)]
        public IActionResult DateOfBirth()
        {
            var userSessionData = TempData.Get<UserSessionData>();

            if (userSessionData == null)
                return Redirect(_siteConfiguration.StartPage);

            var model = new DateOfBirthViewModel
            {
                Day = userSessionData.DateOfBirth?.Day,
                Month = userSessionData.DateOfBirth?.Month,
                Year = userSessionData.DateOfBirth?.Year,
                UserJourney = userSessionData.UserJourney?.PrePdsJourney
            };

            ViewBag.IsDigital = GetIsDigitalRoute(userSessionData);
            ViewBag.IsCoronavirusHelplineUser = GetIsCoronavirusHelplineUser(userSessionData);
            return View(model);
        }

        [HttpPost]
        [Route(UIConstants.Home.DateOfBirth)]
        public IActionResult DateOfBirth(DateOfBirthViewModel model)
        {
            var userSessionData = TempData.Get<UserSessionData>();
            if (userSessionData == null)
                return Redirect(_siteConfiguration.StartPage);

            if (!ModelState.IsValid)
            {
                ViewBag.IsDigital = GetIsDigitalRoute(userSessionData);
                ViewBag.IsCoronavirusHelplineUser = GetIsCoronavirusHelplineUser(userSessionData);
                return View(model);
            }

            userSessionData.DateOfBirth = model;
            TempData.Set(userSessionData);

            return RedirectToAction("NhsNumber");
        }

        [HttpGet]
        [Route(UIConstants.Home.Postcode)]
        public IActionResult Postcode()
        {
            var userSessionData = TempData.Get<UserSessionData>();

            if (userSessionData == null)
                return Redirect(_siteConfiguration.StartPage);

            var model = new PostcodeViewModel
            {
                Postcode = userSessionData.Postcode?.Postcode
            };
            
            ViewBag.BackLink = userSessionData.NhsNumber?.KnowsNhsNumber ?? false
                ? Url.Action("NhsNumber")!
                : Url.Action("Name")!;
            ViewBag.IsDigital = GetIsDigitalRoute(userSessionData);
            ViewBag.IsCoronavirusHelplineUser = GetIsCoronavirusHelplineUser(userSessionData);

            return View(model);
        }

        [HttpPost]
        [Route(UIConstants.Home.Postcode)]
        public IActionResult Postcode(PostcodeViewModel model)
        {
            var userSessionData = TempData.Get<UserSessionData>();
            if (userSessionData == null)
                return Redirect(_siteConfiguration.StartPage);

            if (!ModelState.IsValid)
            {
                var knowsNhsNumber = (userSessionData.NhsNumber?.KnowsNhsNumber).GetValueOrDefault();
                ViewBag.BackLink = knowsNhsNumber ? Url.Action("NhsNumber") : Url.Action("Name");
                ViewBag.IsDigital = GetIsDigitalRoute(userSessionData);
                ViewBag.IsCoronavirusHelplineUser = GetIsCoronavirusHelplineUser(userSessionData);

                return View(model);
            }

            userSessionData.Postcode = model;

            var targetPage = nameof(CheckAnswers);

            if (RegionServices.PostcodeIsInWales(model.Postcode))
            {
                SetAccessibleAndAlternateLanguagesChoicesToNotSelected(userSessionData);

                targetPage = nameof(CheckAnswers);
            }

            TempData.Set(userSessionData);

            return RedirectToAction(targetPage);
        }

        [HttpGet]
        [Route(UIConstants.Home.CheckYourAnswers)]
        public IActionResult CheckAnswers()
        {
            var userSessionData = TempData.Get<UserSessionData>();

            if (userSessionData == null)
                return Redirect(_siteConfiguration.StartPage);

            if (IsUserSessionCorrupted(userSessionData, nameof(CheckAnswers)))
            {
                var returnPage = GetRedirectActionForCorruptUserSessionOrDefault(userSessionData, "Postcode");
                if (returnPage == nameof(WhoAreYouRequestingFor))
                    return RedirectToAction(returnPage, new { service = GetIsDigitalRoute(userSessionData) ? "digital" : "letter", });
                else
                    return RedirectToAction(returnPage);
            }

            var postcodeIsInWales = RegionServices.PostcodeIsInWales(userSessionData.Postcode?.Postcode);
            var userJourneyIsDigital = userSessionData!.UserJourney?.PrePdsJourney == InitUserJourney.Digital;

            if (postcodeIsInWales || userJourneyIsDigital)
            {
                SetAccessibleAndAlternateLanguagesChoicesToNotSelected(userSessionData);
            }

            var model = new CheckAnswersViewModel
            {
                NhsNumber = userSessionData!.NhsNumber!.NhsNumber,
                Name = string.Join(" ", userSessionData.Name?.FirstName, userSessionData.Name?.LastName).Trim(),
                DateOfBirth = userSessionData.DateOfBirth!.ToCustomDateString(),
                Postcode = userSessionData.Postcode!.Postcode,
                KnowsNhsNumber = (userSessionData.NhsNumber?.KnowsNhsNumber).GetValueOrDefault(),
                BackLink = nameof(Postcode)
            };

            ViewBag.IsDigital = GetIsDigitalRoute(userSessionData);
            ViewBag.IsCoronavirusHelplineUser = GetIsCoronavirusHelplineUser(userSessionData);
            return View("CheckAnswers", model);
        }

        [HttpPost]
        [Route(UIConstants.Home.CheckYourAnswers)]
        public async Task<IActionResult> CheckAnswers(UserDetailsSearchViewModel model)
        {
            var userSessionData = TempData.Get<UserSessionData>();
            if (userSessionData == null)
                return Redirect(_siteConfiguration.StartPage);

            if (IsUserSessionCorrupted(userSessionData, nameof(CheckAnswers)))
            {
                return RedirectToAction("CheckAnswers");
            }

            SearchPatientResult result;
            bool userHasWelshPostcode = RegionServices.PostcodeIsInWales(userSessionData!.Postcode?.Postcode);
            bool userIsDigitalJourney = userSessionData.UserJourney?.PrePdsJourney == InitUserJourney.Digital;

            if (userSessionData.NhsNumber!.KnowsNhsNumber.GetValueOrDefault())
            {
                result = await _searchPatientService.Get(
                    userSessionData!.NhsNumber.NhsNumber.Replace(" ", ""),
                    userSessionData.DateOfBirth!.ToDateTime(),
                    userSessionData!.Postcode!.Postcode,
                    TempData.GetCorrelationId(),
                    userHasWelshPostcode,
                    userIsDigitalJourney);

                return result.Accept(new SearchPatientResultVisitor(TempData, _logger));
            }

            var request = new SearchPatientRequest
            {                                         
                FirstName = userSessionData.Name?.FirstName,
                LastName = userSessionData.Name?.LastName,
                DateOfBirth = userSessionData.DateOfBirth?.ToDateTime(),
                Postcode = userSessionData.Postcode?.Postcode,
                userHasWelshPostcode = userHasWelshPostcode,
                userIsDigitalJourney = userIsDigitalJourney
            };

            result = await _searchPatientService.Search(request, TempData.GetCorrelationId());
            return result.Accept(new SearchPatientResultVisitor(TempData, _logger));
        }

        [HttpGet]
        [Route(UIConstants.Home.CookiePolicy)]
        public IActionResult CookiePolicy()
        {
            ViewBag.HideFooter = true;
            var userSessionData = TempData.Get<UserSessionData>();
            
            if (userSessionData != null)
            {
                ViewBag.IsDigital = GetIsDigitalRoute(userSessionData);
                ViewBag.IsCoronavirusHelplineUser = GetIsCoronavirusHelplineUser(userSessionData);
            }
            
            return View();
        }

        [HttpGet]
        [Route(UIConstants.Home.AccessibilityStatement)]
        public IActionResult AccessibilityStatement()
        {
            ViewBag.HideFooter = true;
            return View();
        }

        /// <summary>
        /// Called via Javascript to extend the current user session.
        /// </summary>
        /// <returns>Simply returns true as this is a 'Keep alive' pulse only and has no other use to the client.</returns>
        [Route(nameof(KeepSessionAlive))]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult KeepSessionAlive()
        {
            // Set our custom cookie to check when session times out
            CookieExtensions.SetNhsSessionTimeoutCookie(HttpContext, _sessionExpiresMinutes.Value);
            return Content("true");
        }

        /// <summary>
        /// Display the expired screen after the user has been logged out due to inactivity.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(UIConstants.Home.SessionExpiredPath)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Expired()
        {
            TempData.Clear();
            TempData.Set<UserSessionData>(null!);

            if (HttpContext != null)
            {
                foreach (var cookie in HttpContext.Request.Cookies)
                {
                    if (cookie.Key == CookieExtensions.NhsSessionTimeoutCookieName)
                    {
                        Response.Cookies.Delete(cookie.Key);
                        /* user is redirected to the same page to force a view refresh which will re-draw the correct language */
                        return RedirectToAction("Expired");
                    }
                }
            }

            return View();
        }

        [HttpGet]
        [Route(UIConstants.Home.Error)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var userSessionData = TempData.Get<UserSessionData>();

            if (userSessionData != null)
            {
                ViewBag.IsDigital = GetIsDigitalRoute(userSessionData);
                ViewBag.IsCoronavirusHelplineUser = GetIsCoronavirusHelplineUser(userSessionData);
            }

            return View();
        }
        
        [HttpPost]
        [Route(UIConstants.Home.SetLanguage)]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            if (_localizationOptions.Value.SupportedCultures?.Select(c => c.Name).Contains(culture) != true)
            {
                return LocalRedirect(returnUrl);
            }

            setUserLanguage(Response, culture);
            return LocalRedirect(returnUrl);
        }

        /* helper methods */
        private static void SetAccessibleAndAlternateLanguagesChoicesToNotSelected(UserSessionData userSessionData)
        {
            AlternateLetterTypeModel alternateLetterOptions = new AlternateLetterTypeModel()
            { 
                UserRequestedAccessibleFormat = false,
                UserRequestedAnotherLanguage = false,
                AccessibleFormatType = UIConstants.NotRequestedValue
            };

            userSessionData.AlternateLetterType = alternateLetterOptions;
            userSessionData.Languages = new LanguagesViewModel();
        }

        /// <summary>
        /// As per CHRP-17269 & CHRP-17273, data appears to be corrupted when loading TempData from cookie.
        /// The exact cause of this is unknown, so for the time being log the state of the request only.
        /// </summary>
        /// <param name="userSessionData">The malformed value retrieved from TempData</param>
        /// <param name="action">The source action from where value is being checked</param>
        /// <returns>True if the data appears to be corrupted; otherwise false</returns>
        private bool IsUserSessionCorrupted(UserSessionData userSessionData, string action)
        {
            var knowsNhsNumber = userSessionData.NhsNumber != null &&
                                 userSessionData.NhsNumber.KnowsNhsNumber.GetValueOrDefault();
            if (userSessionData.NhsNumber == null
                || userSessionData.DateOfBirth == null
                || userSessionData.Postcode == null
                || (!knowsNhsNumber && userSessionData.Name == null))
            {
                _logger.LogError(
                    AppEventId.InvalidUserSessionData,
                    "Unexpected missing data from {TempData} in {Action}: NhsNumber null = {NhsNumberNull}, " +
                    "DateOfBirth null = {DateOfBirthNull}, Postcode null = {PostcodeNull}, " +
                    "KnowsNhsNumber = {KnowsNhsNumber}, Name null = {NameNull}",
                    nameof(UserSessionData),
                    action,
                    userSessionData.NhsNumber == null,
                    userSessionData.DateOfBirth == null,
                    userSessionData.Postcode == null,
                    knowsNhsNumber,
                    userSessionData.Name == null);

                return true;
            }

            return false;
        }

        private string GetRedirectActionForCorruptUserSessionOrDefault(
            UserSessionData userSessionData,
            string defaultActionNameForNonCorruptData)
        {
            if (userSessionData.RequestingFor == null) return nameof(WhoAreYouRequestingFor);
            if (userSessionData.DateOfBirth == null) return nameof(DateOfBirth);
            if (userSessionData.NhsNumber == null) return nameof(NhsNumber);
            if (userSessionData.Postcode == null) return nameof(Postcode);

            var knowsNhsNumber = userSessionData.NhsNumber != null &&
                                 userSessionData.NhsNumber.KnowsNhsNumber.GetValueOrDefault();
            if (!knowsNhsNumber && userSessionData.Name == null) return nameof(Name);

            return defaultActionNameForNonCorruptData;
        }

        private bool GetIsDigitalRoute(UserSessionData userSessionData)
        {
            return userSessionData?.UserJourney?.PrePdsJourney == InitUserJourney.Digital;
        }

        private bool GetIsCoronavirusHelplineUser(UserSessionData userSessionData)
        {
            return userSessionData?.isCoronavirusHelplineUser == true;
        }

        private static void setUserLanguage(HttpResponse response, string languageCulture)
        {
            response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(languageCulture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), Secure = true }
            );
        }

        private static void InitialiseSessionStorage(UserSessionData userSessionData, ITempDataDictionary tempData)
        {
            if (userSessionData == null)
                tempData.Set(new UserSessionData());
        }
    }
}
