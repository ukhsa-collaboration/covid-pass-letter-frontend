using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using CovidLetter.Frontend.Extensions;
using CovidLetter.Frontend.Pds.Options;
using CovidLetter.Frontend.WebApp.Configuration;
using CovidLetter.Frontend.WebApp.Extensions;
using CovidLetter.Frontend.WebApp.Models;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

namespace CovidLetter.Frontend.WebApp.Insights;

internal sealed class TelemetryInitializer : ITelemetryInitializer
{
    private const string PiiRegexPattern =
        @".+Patient\/(\d+)|\?.+family=(\w+).+given=([\w\+]+).+birthdate=eq([0-9-]+).+postcode=([\w|\+]+)";

    private static readonly ICollection<string> SensitiveHeaders = new List<string>
    {
        "x-functions-key",
        "Authorization"
    };

    private readonly FeatureToggle _featureToggle;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _pdsBaseUrl;

    public TelemetryInitializer(
        IHttpContextAccessor httpContextAccessor,
        IOptions<ApiManagerClientOptions> options,
        FeatureToggle featureToggle)
    {
        _httpContextAccessor = httpContextAccessor;
        _pdsBaseUrl = options.Value.BaseUrl;
        _featureToggle = featureToggle;
    }

    private bool ShouldLogOutboundHeaders => _featureToggle.IsEnabled(FeatureToggle.LogOutboundRequestHeaders);

    private static IEnumerable<string> RequestHeaders { get; } = new[]
    {
        "True-Client-IP",
        "Referer",
        "User-Agent",
        "X-Forwarded-For",
        "X-Forwarded-Host",
        "X-Forwarded-Proto"
    };

    private static IEnumerable<string> ResponseHeaders { get; } = new[]
    {
        "Content-Type",
        "X-Correlation-ID"
    };

    private ITempDataDictionary? TempData => _httpContextAccessor.HttpContext?.GetTempData();

    public void Initialize(ITelemetry telemetry)
    {
        switch (telemetry)
        {
            case RequestTelemetry requestTelemetry:
                AddCorrelationId(requestTelemetry);
                AddHeaders(requestTelemetry);
                break;
            case ExceptionTelemetry exceptionTelemetry
                when exceptionTelemetry.Exception?.GetType() != typeof(CryptographicException):
                AddCorrelationId(exceptionTelemetry);
                AddCoronavirusHelplineUserFlag(exceptionTelemetry);
                break;
            case TraceTelemetry traceTelemetry:
                AddCorrelationId(traceTelemetry);
                AddCoronavirusHelplineUserFlag(traceTelemetry);
                break;
            case DependencyTelemetry dependencyTelemetry:
                RedactPdsRequest(dependencyTelemetry);
                AddOutboundRequestHeaders(dependencyTelemetry);
                break;
        }
    }

    private void AddCoronavirusHelplineUserFlag(ISupportProperties? supportProperties)
    {
        var coronavirusHelplineSessionData = TempData?.Get<CoronavirusHelplineSessionData>();
        if (coronavirusHelplineSessionData == null)
        {
            return;
        }

        if (supportProperties != null)
        {
            supportProperties.Properties[nameof(CoronavirusHelplineSessionData.IsCoronavirusHelplineUser)] =
                coronavirusHelplineSessionData.IsCoronavirusHelplineUser.ToString();
        }
    }

    private void AddCorrelationId(ISupportProperties? supportProperties)
    {
        var correlationData = TempData?.Get<CorrelationData>();

        if (correlationData == null && supportProperties is not ExceptionTelemetry)
        {
            correlationData = new CorrelationData(Guid.NewGuid());
            TempData?.Set(correlationData);
        }

        if (supportProperties != null && correlationData != null)
        {
            supportProperties.Properties[nameof(CorrelationData.CorrelationId)] = correlationData.CorrelationId;
        }
    }

    private void AddHeaders(ISupportProperties supportProperties)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            return;
        }

        // Add Headers to Telemetry
        AddHeadersAsProperties(RequestHeaders, context.Request.Headers, "Request");
        AddHeadersAsProperties(ResponseHeaders, context.Response.Headers, "Response");

        void AddHeadersAsProperties(IEnumerable<string> headerNames, IHeaderDictionary headers, string prefix)
        {
            foreach (var headerName in headerNames)
            {
                var headerValues = headers[headerName];
                if (headerValues.Any())
                {
                    supportProperties?.Properties.Add($"{prefix} {headerName}",
                        string.Join(Environment.NewLine, headerValues));
                }
            }
        }
    }

    private void AddOutboundRequestHeaders(DependencyTelemetry dependencyTelemetry)
    {
        if (!ShouldLogOutboundHeaders
            || !dependencyTelemetry.TryGetOperationDetail("HttpRequest", out var request)
            || request is not HttpRequestMessage httpRequest)
        {
            return;
        }

        foreach (var (headerName, headerValues) in httpRequest.Headers)
        {
            if (dependencyTelemetry.Properties.ContainsKey(headerName))
            {
                continue;
            }

            var sensitiveHeader = SensitiveHeaders.Contains(headerName, StringComparer.InvariantCultureIgnoreCase);
            dependencyTelemetry.Properties.Add("headers." + headerName, string.Join(Environment.NewLine, headerValues.Select(val =>
                sensitiveHeader
                    ? val[..3] + "***"
                    : val)));
        }
    }

    private void RedactPdsRequest(DependencyTelemetry dependencyTelemetry)
    {
        if (dependencyTelemetry.Data == null || !dependencyTelemetry.Data.StartsWith(_pdsBaseUrl))
        {
            return;
        }

        // redact first name, last name, date of birth, postcode and NHS number from URL dependency tracking
        var redactedUrl = dependencyTelemetry.Data;
        try
        {
            Regex.Replace(dependencyTelemetry.Data, PiiRegexPattern, match =>
            {
                var groups = match.Groups
                    .Cast<Group>()
                    .Skip(1)
                    .Where(g => g.Captures.Any())
                    .Where(g => !string.IsNullOrWhiteSpace(g.Value));
                redactedUrl = groups.Aggregate(
                    dependencyTelemetry.Data,
                    (current, group) => current.Replace(group.Value, group.Value[..3] + "***"));
                return null!;
            });
        }
        catch
        {
            // do not break dependency tracking if unexpected content found in DependencyTelemetry.Data
            redactedUrl = dependencyTelemetry.Data;
        }

        dependencyTelemetry.Data = redactedUrl;
    }
}
