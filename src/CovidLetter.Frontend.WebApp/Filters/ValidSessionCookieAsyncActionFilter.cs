using System.Threading.Tasks;
using CovidLetter.Frontend.Extensions;
using CovidLetter.Frontend.WebApp.Configuration;
using CovidLetter.Frontend.WebApp.Constants;
using CovidLetter.Frontend.WebApp.Services.SessionTimeoutService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace CovidLetter.Frontend.WebApp.Filters
{
    public class ValidSessionCookieAsyncActionFilter : IAsyncActionFilter
    {
        private readonly IOptions<AuthenticationConfiguration> _authConfiguration;
        private readonly IDateTimeHelperService _dateTimeHelperService;

        public ValidSessionCookieAsyncActionFilter(
            IOptions<AuthenticationConfiguration> authConfiguration, 
            IDateTimeHelperService dateTimeHelperService)
        {
            _authConfiguration = authConfiguration;
            _dateTimeHelperService = dateTimeHelperService;
        }
        
        public async Task OnActionExecutionAsync(ActionExecutingContext? context, ActionExecutionDelegate next)
        {
            var request = context?.HttpContext.Request;
            var routeValues = request?.RouteValues ?? new RouteValueDictionary();
            if (!IRedirectOnSessionTimeoutWhitelistService.IsCurrentRouteWhitelistedToBypassSessionTimeout(routeValues) 
                && (request?.Cookies.ContainsKey(CookieExtensions.NhsSessionTimeoutCookieName) != true ||
                    CookieExtensions.HasSessionCookieExpired(
                        request.Cookies[CookieExtensions.NhsSessionTimeoutCookieName],
                        _dateTimeHelperService.GetUtcDateTimeNowInTicks())))
            {
                // Redirect to expired which clears Temp data. 
                if (context != null)
                {
                    context.Result = new RedirectResult(UIConstants.Home.SessionExpiredPath);
                    return;
                }
            }
            else
            {
                // Cookie is still valid. Slide its value out to the full expiry period again.
                CookieExtensions.SetNhsSessionTimeoutCookie(context?.HttpContext, _authConfiguration.Value.ExpireMinutes);
            }
            
            await next();
        }
    }
}