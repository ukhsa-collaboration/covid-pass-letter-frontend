using System;
using System.Linq;
using CovidLetter.Frontend.Logging;
using CovidLetter.Frontend.Search;
using CovidLetter.Frontend.WebApp.Constants;
using CovidLetter.Frontend.WebApp.Controllers;
using CovidLetter.Frontend.WebApp.Extensions;
using CovidLetter.Frontend.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace CovidLetter.Frontend.WebApp.Services
{
    internal class RequestOtpResultVisitor : IRequestOtpResultVisitor<IActionResult>
    {
        private readonly ITempDataDictionary _tempData;

        public RequestOtpResultVisitor(ITempDataDictionary tempData)
        {
            _tempData = tempData;
        }

        public IActionResult Visit(RequestOtpResult.Success result)
        {
            var userSessionData = _tempData.Get<UserSessionData>();
            var redirectToActionResult = new RedirectToActionResult(
                nameof(DigitalController.VerifyOtp),
                nameof(DigitalController).RemoveController(),
                null);

            if (userSessionData == null)
            {
                return redirectToActionResult;
            }

            if (userSessionData.VerifyMobile != null)
            {
                userSessionData.VerifyMobile.MobileNumber = result.MobileNumber;
            }

            userSessionData.remainingOtpAttempts = UIConstants.Digital.InitialAllowedOtpAttempts;
            _tempData.Set(userSessionData);

            return redirectToActionResult;
        }

        public IActionResult Visit(RequestOtpResult.Failed result)
        {
            return new RedirectToActionResult(
                nameof(HomeController.Error),
                nameof(HomeController).RemoveController(),
                null);
        }

        public IActionResult Visit(RequestOtpResult.TooManyRequests result)
        {
            return new RedirectToActionResult(
                nameof(DigitalController.MaximumOtpsGenerated),
                nameof(DigitalController).RemoveController(),
                null);
        }
    }
}
