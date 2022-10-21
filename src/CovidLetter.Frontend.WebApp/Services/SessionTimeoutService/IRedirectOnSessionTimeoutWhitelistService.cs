using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Linq;
using CovidLetter.Frontend.WebApp.Controllers;
using CovidLetter.Frontend.WebApp.Extensions;

namespace CovidLetter.Frontend.WebApp.Services.SessionTimeoutService
{
    /// <summary>
    /// Static whitelist of Controllers and associated Actions which will bypass the Session Timeout feature.
    /// </summary>
    public interface IRedirectOnSessionTimeoutWhitelistService
    {
        private static readonly KeyValuePair<string, List<string>?> WhitelistedOutcomeControllerAndActions = new(
            nameof(OutcomeController).RemoveController(),
            new List<string>
            {
                nameof(OutcomeController.NoMatch),
                nameof(OutcomeController.Submitted)
            }
        );

        private static readonly KeyValuePair<string, List<string>?> WhitelistedHomeControllerAndActions = new(
            nameof(HomeController).RemoveController(),
            new List<string>
            {
                nameof(HomeController.Index),
                nameof(HomeController.KeepSessionAlive),
                nameof(HomeController.Expired),
                nameof(HomeController.RequestLetterForTravel),
                nameof(HomeController.WhoAreYouRequestingFor)
            }
        );

        /// <summary>
        /// Whitelist of Controllers and associated Actions which bypass the Session Timeout feature when the
        /// DomesticPassAvailable feature flag is disabled.
        /// </summary>
        public static readonly Dictionary<string, List<string>?> UnauthenticatedRoutes = new(
            new List<KeyValuePair<string, List<string>?>>
            {
                WhitelistedHomeControllerAndActions,
                WhitelistedOutcomeControllerAndActions
            });

        /// <summary>
        /// Check if the current RouteData is whitelisted to bypass session timeout.
        /// Use on views to determine whether or not to activate the Javascript for session countdown.
        /// </summary>
        /// <param name="routeValueDictionary">The Route Value Dictionary.</param>
        /// <returns>True if the current route is whitelisted, else False.</returns>
        public static bool IsCurrentRouteWhitelistedToBypassSessionTimeout(RouteValueDictionary? routeValueDictionary)
        {
            var controllerName = routeValueDictionary?["controller"] as string;
            var actionName = routeValueDictionary?["action"] as string;

            return ValidateControllerAndAction(controllerName, actionName);
        }

        /// <summary>
        /// Check if the current RouteData is whitelisted to bypass session timeout.
        /// Use on views to determine whether or not to activate the Javascript for session countdown.
        /// </summary>
        /// <param name="routeData">The RouteData.</param>
        /// <returns>True if the current route is whitelisted, else False.</returns>
        public static bool IsCurrentRouteWhitelistedToBypassSessionTimeout(RouteData? routeData)
        {
            var controllerName = routeData?.Values["controller"] as string;
            var actionName = routeData?.Values["action"] as string;

            return ValidateControllerAndAction(controllerName, actionName);
        }

        /// <summary>
        /// Validate if the Controller and Action names match a whitelisted route.
        /// </summary>
        /// <param name="controllerName">The controllerName.</param>
        /// <param name="actionName">The actionName.</param>
        /// <returns>True if the current route is whitelisted, else False.</returns>
        private static bool ValidateControllerAndAction(string? controllerName, string? actionName)
        {
            if (controllerName != null &&
                actionName != null &&
                UnauthenticatedRoutes.Any(x => x.Key == controllerName && (x.Value == null || x.Value.Contains(actionName))))
            {
                return true;
            }

            return false;
        }
    }
}
