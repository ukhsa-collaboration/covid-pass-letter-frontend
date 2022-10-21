using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace CovidLetter.Frontend.WebApp.Configuration;

public class FeatureToggle
{
    public static readonly string LogOutboundRequestHeaders = nameof(LogOutboundRequestHeaders);

    public static readonly string UseServiceBus = nameof(UseServiceBus);

    public static readonly string WelshUI = nameof(WelshUI);

    private readonly Lazy<IReadOnlySet<string>> _enabledFeatures;

    public FeatureToggle(IOptions<AppOptions> functionOptions)
    {
        _enabledFeatures = new Lazy<IReadOnlySet<string>>(() => NormalizeNames(functionOptions.Value.EnabledFeatures));
    }

    public bool IsEnabled(string featureName) => _enabledFeatures.Value.Contains(featureName);

    private static HashSet<string> NormalizeNames(string enabledFunctions)
    {
        var hashSet =
            (enabledFunctions ?? string.Empty)
            .Split(",").Select(f => f.Trim())
            .ToHashSet(StringComparer.InvariantCultureIgnoreCase);

        return hashSet;
    }
}