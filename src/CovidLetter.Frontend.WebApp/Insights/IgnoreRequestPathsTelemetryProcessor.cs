// ReSharper disable ClassNeverInstantiated.Global
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace CovidLetter.Frontend.WebApp.Insights;

public class IgnoreRequestPathsTelemetryProcessor : ITelemetryProcessor
{
    private readonly ITelemetryProcessor _next;

    private readonly IReadOnlyCollection<string> _ignoreRequestTelemetryPaths = new List<string>
    {
        "/health",
        "/lib"
    };

    public IgnoreRequestPathsTelemetryProcessor(ITelemetryProcessor next)
    {
        _next = next;
    }

    public void Process(ITelemetry item)
    {
        switch (item)
        {
            case RequestTelemetry request:
            {
                var absolutePath = request.Url.AbsolutePath;
                if (request.ResponseCode == ((int) HttpStatusCode.OK).ToString()
                    && _ignoreRequestTelemetryPaths.Any(p => absolutePath.StartsWith(p)))
                {
                    return;
                }

                break;
            }
            case TraceTelemetry trace
                when trace.SeverityLevel.GetValueOrDefault(SeverityLevel.Information) < SeverityLevel.Warning 
                     && trace.Properties.ContainsKey("Host") 
                     && trace.Properties.TryGetValue("Path", out var path) 
                     && _ignoreRequestTelemetryPaths.Any(p => path.StartsWith(p)):
                return;
        }

        _next.Process(item);
    }
}