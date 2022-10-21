using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CovidLetter.Frontend.WebApp.Extensions;
using CovidLetter.Frontend.WebApp.Models;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CovidLetter.Frontend.WebApp.Filters;

public class RequestTelemetryPropertiesActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var controller = context.Controller as Controller;
        var telemetry = context?.HttpContext?.Features?.Get<RequestTelemetry>();

        AddTempDataProperty<CoronavirusHelplineSessionData>(
            controller,
            telemetry,
            d => (nameof(d.IsCoronavirusHelplineUser), d?.IsCoronavirusHelplineUser.ToString()));

        AddTempDataProperty<CorrelationData>(
            controller,
            telemetry,
            d => (nameof(d.CorrelationId), d?.CorrelationId));

        if (TryGetRequestCulture(context?.HttpContext, out var cultureName))
        {
            AddProperty(telemetry, "RequestCulture", cultureName);
        }

        await next();
    }

    private static void AddProperty(RequestTelemetry? telemetry, string? key, string? value)
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value) || telemetry?.Properties == null)
        {
            return;
        }

        telemetry.Properties[key] = value;
    }

    private static void AddTempDataProperty<T>(
        Controller? controller,
        RequestTelemetry? requestTelemetry,
        Func<T, (string?, string?)> getKeyAndValue) where T : class
    {
        var tempData = controller?.TempData.Get<T>();

        if (tempData == null)
        {
            return;
        }

        var (key, value) = getKeyAndValue(tempData);
        AddProperty(requestTelemetry, key, value);
    }

    private static bool TryGetRequestCulture(HttpContext? context, [NotNullWhen(true)] out string? cultureName)
    {
        var requestCultureFeature = context?.Features?.Get<IRequestCultureFeature>();

        cultureName = requestCultureFeature?.RequestCulture?.Culture?.Name;

        return cultureName != null;
    }
}