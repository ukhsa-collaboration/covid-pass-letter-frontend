using System.Linq;
using CovidLetter.Frontend.Logging;
using CovidLetter.Frontend.Search;
using CovidLetter.Frontend.WebApp.Constants;
using CovidLetter.Frontend.WebApp.Controllers;
using CovidLetter.Frontend.WebApp.Extensions;
using CovidLetter.Frontend.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;

namespace CovidLetter.Frontend.WebApp.Services
{
    internal class SearchPatientResultVisitor : ISearchPatientResultVisitor<IActionResult>
    {
        private readonly ITempDataDictionary _tempData;
        private readonly ILogger _logger;

        public SearchPatientResultVisitor(ITempDataDictionary tempData, ILogger logger)
        {
            _tempData = tempData;
            _logger = logger;
        }

        public IActionResult Visit(SearchPatientResult.ImmediatePass result)
        {
            var logMessage = $"Successful - Emails: {result.Emails.Count()}, Mobiles: {result.Mobiles.Count()}, Addresses: {result.Addresses.Count()}";
            LogInformation(true, logMessage);

            var searchResultData = new SearchResultData(result.UserDetail, result.Emails, result.Mobiles, result.Addresses);

            var journeyData = new PostPdsJourneyData(new UserJourneyModel()
            {
                PostPdsDigitalUserJourney = result.Mobiles.Any() ? PostPdsUserJourneyForDigitalUser.VerifyMobileNumberWithGpRecord : PostPdsUserJourneyForDigitalUser.VerifyEmailWithGpRecord
            });

            var userPdsStatus = new UserPdsStatusModel(new UserJourneyModel() { UserHasTravelledThroughPds = true });

            ResetSessionAttemptCounters(result.UserDetail.NHSNumber);
            ResetEligibilityForImmediatePass(true);
            _tempData.Set(searchResultData);
            _tempData.Set(journeyData);
            _tempData.Set(userPdsStatus);

            return new RedirectToActionResult(nameof(OutcomeController.ImmediatePass), UIConstants.Outcome.OutcomeController, null);
        }

        public IActionResult Visit(SearchPatientResult.SuccessWithoutAccessibility result)
        {
            var logMessage = $"Successful - Emails: {result.Emails.Count()}, Mobiles: {result.Mobiles.Count()}, Addresses: {result.Addresses.Count()}";
            LogInformation(true, logMessage);

            var searchResultData = new SearchResultData(result.UserDetail, result.Emails, result.Mobiles, result.Addresses);

            var userPdsStatus = new UserPdsStatusModel(new UserJourneyModel() { UserHasTravelledThroughPds = true });

            ResetEligibilityForImmediatePass(false);
            _tempData.Set(searchResultData);
            _tempData.Set(userPdsStatus);

            return new RedirectToActionResult(nameof(OutcomeController.ContactPreference), UIConstants.Outcome.OutcomeController, null);
        }

        public IActionResult Visit(SearchPatientResult.SuccessWithAccessibility result)
        {
            var logMessage = $"Successful - Emails: {result.Emails.Count()}, Mobiles: {result.Mobiles.Count()}, Addresses: {result.Addresses.Count()}";
            LogInformation(true, logMessage);

            var searchResultData = new SearchResultData(result.UserDetail, result.Emails, result.Mobiles, result.Addresses);

            var userPdsStatus = new UserPdsStatusModel(new UserJourneyModel() { UserHasTravelledThroughPds = true });

            ResetEligibilityForImmediatePass(false);
            _tempData.Set(searchResultData);
            _tempData.Set(userPdsStatus);

            return new RedirectToActionResult(nameof(OutcomeController.EligibilityForLetter), UIConstants.Outcome.OutcomeController, null);
        }

        public IActionResult Visit(SearchPatientResult.SuccessWithMobile result)
        {
            var logMessage = $"Successful - Emails: {result.Emails.Count()}, Mobiles: {result.Mobiles.Count()}, Addresses: {result.Addresses.Count()}";
            LogInformation(true, logMessage);

            var searchResultData = new SearchResultData(result.UserDetail, result.Emails, result.Mobiles, result.Addresses);

            var journeyData = new PostPdsJourneyData(new UserJourneyModel()
            {
                PostPdsDigitalUserJourney = PostPdsUserJourneyForDigitalUser.VerifyMobileNumberWithGpRecord
            });

            var userPdsStatus = new UserPdsStatusModel(new UserJourneyModel() { UserHasTravelledThroughPds = true });

            ResetSessionAttemptCounters(result.UserDetail.NHSNumber);
            ResetEligibilityForImmediatePass(false);
            _tempData.Set(searchResultData);
            _tempData.Set(journeyData);
            _tempData.Set(userPdsStatus);

            return new RedirectToActionResult(nameof(DigitalController.VerifyMobile), UIConstants.Digital.DigitalController, null);
        }

        public IActionResult Visit(SearchPatientResult.SuccessWithEmail result)
        {
            var logMessage = $"Successful - Emails: {result.Emails.Count()}, Mobiles: {result.Mobiles.Count()}, Addresses: {result.Addresses.Count()}";
            LogInformation(true, logMessage);

            var searchResultData = new SearchResultData(result.UserDetail, result.Emails, result.Mobiles, result.Addresses);

            var journeyData = new PostPdsJourneyData(new UserJourneyModel()
            {
                PostPdsDigitalUserJourney = PostPdsUserJourneyForDigitalUser.VerifyEmailWithGpRecord
            });

            var userPdsStatus = new UserPdsStatusModel(new UserJourneyModel() { UserHasTravelledThroughPds = true });

            ResetSessionAttemptCounters(result.UserDetail.NHSNumber);
            ResetEligibilityForImmediatePass(false);
            _tempData.Set(searchResultData);
            _tempData.Set(journeyData);
            _tempData.Set(userPdsStatus);

            return new RedirectToActionResult(nameof(DigitalController.VerifyEmail), UIConstants.Digital.DigitalController, null);
        }

        public IActionResult Visit(SearchPatientResult.SuccessWithMobileAndEmail result)
        {
            var logMessage = $"Successful - Emails: {result.Emails.Count()}, Mobiles: {result.Mobiles.Count()}, Addresses: {result.Addresses.Count()}";
            LogInformation(true, logMessage);

            var searchResultData = new SearchResultData(result.UserDetail, result.Emails, result.Mobiles, result.Addresses);

            var journeyData = new PostPdsJourneyData(new UserJourneyModel()
            {
                userHasEmailAndPhoneNumber = true,
                PostPdsDigitalUserJourney = PostPdsUserJourneyForDigitalUser.NotYetDecided
            });

            var userPdsStatus = new UserPdsStatusModel(new UserJourneyModel() { UserHasTravelledThroughPds = true });

            ResetSessionAttemptCounters(result.UserDetail.NHSNumber);
            ResetEligibilityForImmediatePass(false);
            _tempData.Set(searchResultData);
            _tempData.Set(journeyData);
            _tempData.Set(userPdsStatus);

            return new RedirectToActionResult(nameof(DigitalController.EmailOrPhoneNumberChoice), UIConstants.Digital.DigitalController, null);
        }

        public IActionResult Visit(SearchPatientResult.MoreThanOneMatch result)
        {
            LogInformation(false, "Unsuccessful - more than one match");
            return new RedirectToActionResult(nameof(OutcomeController.NoMatch), UIConstants.Outcome.OutcomeController, null);
        }

        public IActionResult Visit(SearchPatientResult.MatchFoundButNoContactDetails result)
        {
            LogInformation(false, "Unsuccessful - match found but no contact details");

            var userPdsStatus = new UserPdsStatusModel(new UserJourneyModel() { UserHasTravelledThroughPds = true });
            ResetEligibilityForImmediatePass(false);
            _tempData.Set(userPdsStatus);

            return new RedirectToActionResult(nameof(DigitalController.NoContactDetailsFound), UIConstants.Digital.DigitalController, null);
        }

        public IActionResult Visit(SearchPatientResult.NoMatches result)
        {
            LogInformation(false, "Unsuccessful - no matches");
            return new RedirectToActionResult(nameof(OutcomeController.NoMatch), UIConstants.Outcome.OutcomeController, null);
        }

        public IActionResult Visit(SearchPatientResult.MatchFoundButWelshPostcode result)
        {
            LogInformation(false, "Unsuccessful - match found but 12+ user has welsh postcode");

            var userPdsStatus = new UserPdsStatusModel(new UserJourneyModel() { UserHasTravelledThroughPds = true });
            ResetEligibilityForImmediatePass(false);
            _tempData.Set(userPdsStatus);

            return new RedirectToActionResult(nameof(DigitalController.UserNotEligibleForDigitalFlow), UIConstants.Digital.DigitalController, null);
        }

        public IActionResult Visit(SearchPatientResult.MatchedButDeceased result)
        {
            LogInformation(false, "Unsuccessful - deceased");
            return new RedirectToActionResult(nameof(OutcomeController.NoMatch), UIConstants.Outcome.OutcomeController, null);
        }

        public IActionResult Visit(SearchPatientResult.NoContactDetailsFound result)
        {
            LogInformation(false, "Unsuccessful - no contact details found");
            return new RedirectToActionResult(nameof(OutcomeController.NoMatch), UIConstants.Outcome.OutcomeController, null);
        }

        public IActionResult Visit(SearchPatientResult.NoValidNameDetailsFound result)
        {
            LogInformation(false, "Unsuccessful - no valid name details found");
            return new RedirectToActionResult(nameof(OutcomeController.NoMatch), UIConstants.Outcome.OutcomeController, null);
        }

        public IActionResult Visit(SearchPatientResult.MatchedButNoNhsNumber result)
        {
            LogInformation(false, "Unsuccessful - no NHS number");
            return new RedirectToActionResult(nameof(OutcomeController.NoMatch), UIConstants.Outcome.OutcomeController, null);
        }

        public IActionResult Visit(SearchPatientResult.TooManyRequests result)
        {
            LogInformation(false, "Unsuccessful - too many requests, PDS rate limit exceeded");
            return new RedirectToActionResult(nameof(HomeController.NhsNumber), UIConstants.Home.HomeController, new { serviceBusy = true });
        }

        /* helper methods */
        private void LogInformation(bool isSuccessfulMatch, string logMessage)
        {
            _logger.LogInformation(isSuccessfulMatch ? AppEventId.SuccessfulPdsMatch : AppEventId.FailedPdsMatch,
                logMessage);
        }

        private void ResetSessionAttemptCounters(string searchResultNhsNumber)
        {
            var prevSessionSearchResultData = _tempData.Get<SearchResultData>();
            var userSessionData = _tempData.Get<UserSessionData>();

            if (prevSessionSearchResultData?.UserDetail.NHSNumber != searchResultNhsNumber)
            {
                if (userSessionData != null)
                {
                    userSessionData.remainingEmailAttempts = UIConstants.Digital.InitialAllowedAttempts;
                    userSessionData.remainingOtpAttempts = UIConstants.Digital.InitialAllowedAttempts;
                    userSessionData.remainingMobileAttempts = UIConstants.Digital.InitialAllowedAttempts;
                }
            }

            _tempData.Set(userSessionData);
        }

        private void ResetEligibilityForImmediatePass(bool userIsEligible)
        {
            var userSessionData = _tempData.Get<UserSessionData>();

            if (userSessionData == null)
            {
                return;

            }

            userSessionData.userEligibleForImmediatePass = userIsEligible;
            _tempData.Set(userSessionData);
        }
    }
}