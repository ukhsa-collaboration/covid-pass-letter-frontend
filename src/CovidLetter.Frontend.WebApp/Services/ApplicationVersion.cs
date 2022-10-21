using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace CovidLetter.Frontend.WebApp.Services
{
    public sealed class ApplicationVersion : IStartupFilter
    {
        private readonly ILogger _logger;

        public ApplicationVersion(ILogger<ApplicationVersion> logger) => _logger = logger;

        internal string AssemblyVersion { get; private set; } = "Unknown";
        internal string AssemblyFileVersion { get; private set; } = "Unknown";
        public string AssemblyInformationalVersion { get; private set; } = "Unknown";

        Action<IApplicationBuilder> IStartupFilter.Configure(Action<IApplicationBuilder> next)
        {
            var entryAssembly = Assembly.GetEntryAssembly();

            AssemblyVersion = entryAssembly?.GetName().Version?.ToString() ?? "Unknown";
            _logger.LogInformation("{VersionName}: {Version}", nameof(AssemblyVersion), AssemblyVersion);

            AssemblyFileVersion = entryAssembly?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "Unknown";
            _logger.LogInformation("{VersionName}: {Version}", nameof(AssemblyFileVersion), AssemblyFileVersion);

            AssemblyInformationalVersion = entryAssembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "Unknown";
            _logger.LogInformation("{VersionName}: {Version}", nameof(AssemblyInformationalVersion), AssemblyInformationalVersion);

            return next;
        }
    }
}