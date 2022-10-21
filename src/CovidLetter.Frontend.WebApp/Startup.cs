using System;
using System.Net;
using System.Net.Http;
using CovidLetter.Frontend.AccessTokenCache.Options;
using CovidLetter.Frontend.AccessTokenCache.Services;
using CovidLetter.Frontend.Extensions;
using CovidLetter.Frontend.Logging;
using CovidLetter.Frontend.Pds;
using CovidLetter.Frontend.Pds.Options;
using CovidLetter.Frontend.Queue;
using CovidLetter.Frontend.Search;
using CovidLetter.Frontend.WebApp.Configuration;
using CovidLetter.Frontend.WebApp.Constants;
using CovidLetter.Frontend.WebApp.Extensions;
using CovidLetter.Frontend.WebApp.Filters;
using CovidLetter.Frontend.WebApp.Services;
using CovidLetter.Frontend.WebApp.Insights;
using CovidLetter.Frontend.WebApp.Services.SessionTimeoutService;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Wrap;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace CovidLetter.Frontend.WebApp
{
    public class Startup
    {
        private const string DataProtectionApplicationLocation = "/home/site/wwwroot";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var featureToggle = new FeatureToggle(Options.Create(this.Configuration.Get<AppOptions>()));

            services.Configure<RequestLocalizationOptions>(o =>
            {
                o.ApplyCurrentCultureToResponseHeaders = true;

                var supportedCultures = featureToggle.IsEnabled(FeatureToggle.WelshUI)
                    ? new[] { "en-GB", "cy-GB" }
                    : new[] { "en-GB" };
                o.SetDefaultCulture(supportedCultures[0])
                    .AddSupportedCultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures);
            });

            services.AddLocalization(o => o.ResourcesPath = "Resources");

            var mvc = services.AddControllersWithViews(options =>
                {
                    options.Filters.Add(typeof(RequestTelemetryPropertiesActionFilter));
                    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                })
                .AddMvcLocalization();

#if DEBUG
            // Enable runtime compilation - i.e. hot reloading
            mvc.AddRazorRuntimeCompilation();
#endif

            services
                .AddSingleton<ApplicationVersion>()
                .AddSingleton<IConfigureOptions<MvcOptions>, ConfigureModelBindingLocalization>(
                    x => new ConfigureModelBindingLocalization(x.GetRequiredService<IStringLocalizer<ModelBinding>>()))
                .AddTransient<IStartupFilter>(sp => sp.GetRequiredService<ApplicationVersion>())
                .AddTransient<IDateTimeHelperService, DateTimeHelperService>();

            services.Configure<AuthenticationConfiguration>(Configuration.GetSection(AuthenticationConfiguration.AuthenticationSectionName));

            services.AddAntiforgery(options =>
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always);

            services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
            {
                EnableAdaptiveSampling = false
            }).AddTransient<ITelemetryInitializer, TelemetryInitializer>();
            services.AddApplicationInsightsTelemetryProcessor<IgnoreRequestPathsTelemetryProcessor>();

            var azureBlobUriWithSasToken = Configuration.GetValue<string>("DataProtectionStorageAccountUriWithSas");
            if (string.IsNullOrWhiteSpace(azureBlobUriWithSasToken))
            {
                throw new ApplicationException("Sas token not found");
            }

            services.AddDataProtection()
                .PersistKeysToAzureBlobStorage(new Uri(azureBlobUriWithSasToken))
                // Application name is set to the root of the webapp directory, as that is where the keys should be stored 
                .SetApplicationName(DataProtectionApplicationLocation);

            services.Configure<CookieTempDataProviderOptions>(options =>
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always);

            services.AddMemoryCache();
            services.Configure<OtpServiceOptions>(Configuration.GetSection("OtpService"));
            services.Configure<ApiManagerClientOptions>(Configuration.GetSection("APIMClient"));
            services.Configure<OAuthOptions>(Configuration.GetSection("OAuth"));

            services.AddHttpClient<IPdsApiWrapperClient, PdsApiWrapperClient>()
                .ConfigurePrimaryHttpMessageHandler(_ =>
                    new HttpClientHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip
                    })
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetPdsApiRetryPolicy(services));

            services.AddHttpClient<IOtpService, OtpService>()
                .ConfigurePrimaryHttpMessageHandler(_ =>
                    new HttpClientHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip
                    })
                .SetHandlerLifetime(TimeSpan.FromMinutes(5));

            // if Sandbox PDS API is being consumed then the JWT token need not be generated
            if (Configuration.GetValue<bool?>("Features:UseSandbox").GetValueOrDefault())
            {
                services.AddTransient<ITokenCacheService, EmptyTokenCacheService>();
            }
            else
            {
                services.AddHttpClient<ITokenCacheService, TokenCacheService>()
                    .ConfigurePrimaryHttpMessageHandler(_ =>
                        new HttpClientHandler
                        {
                            AutomaticDecompression = DecompressionMethods.GZip
                        }).SetHandlerLifetime(TimeSpan.FromMinutes(5))
                    .AddPolicyHandler(GetRetryPolicy());
            }

            services.AddTransient<ISearchPatientService, SearchPatientService>();
            services.AddTransient<IObfuscationService, ObfuscationService>();
            services.AddTransient<OutcomeModelService>();
            services.AddTransient<DigitalModelService>();

            services.AddConfiguration(Configuration);
            services.AddTransient<LetterRequestService>();
            services.AddTransient<IQueueService>(x =>
            {
                if (x.GetRequiredService<FeatureToggle>().IsEnabled(FeatureToggle.UseServiceBus))
                {
                    return new ServiceBusService(
                        x.GetRequiredService<ILogger<ServiceBusService>>(),
                        x.GetRequiredService<IOptions<QueueConfig>>());
                }

                return new QueueService(
                    x.GetRequiredService<ILogger<QueueService>>(),
                    x.GetRequiredService<IOptions<QueueConfig>>());
            });

            services.AddHealthChecks();
            services.AddSingleton<IOtpService, OtpService>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddScoped<IUrlHelper>(x => {
                var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
                var factory = x.GetRequiredService<IUrlHelperFactory>();
                return factory.GetUrlHelper(actionContext);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            app.UseCacheResponseHeaders();
            app.UseSecurityResponseHeaders();

            if (!env.IsDevelopment())
            {
                app.UseHsts();
                app.UseExceptionHandler("/error");
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            /* Supports the display of images */
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    env.WebRootPath
                ),
                RequestPath = "/Images"
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            // basic health check endpoint for availability monitoring
            app.MapHealthChecks("/health");
        }

        static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            Random jitterer = new Random();
            var retryWithJitterPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(6,    // exponential back-off plus some jitter
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                    + TimeSpan.FromMilliseconds(jitterer.Next(0, 100))
                );

            return retryWithJitterPolicy;
        }

        static AsyncPolicyWrap<HttpResponseMessage> GetPdsApiRetryPolicy(IServiceCollection services)
        {
            Random jitterer = new Random();

            var tooManyRequestsPolicy =
                Policy.HandleResult<HttpResponseMessage>(msg =>
                        msg.StatusCode == HttpStatusCode.TooManyRequests)
                    .WaitAndRetryAsync(2,
                        retryAttempt => TimeSpan.FromSeconds(retryAttempt * 3)
                                        + TimeSpan.FromMilliseconds(jitterer.Next(0, 100)),
                        (result, timeSpan, retryAttempt, context) =>
                        {
                            context.GetLogger()?.LogWarning(AppEventId.PdsTooManyRequests, "PDS Rate limit exceeded. Delaying for {delay}ms, then making retry {retry}.", timeSpan.TotalMilliseconds, retryAttempt);
                        });

            // Use the default policy as the fallback, with our custom TooManyRequests policy applied first.
            return GetRetryPolicy()
                .WrapAsync(tooManyRequestsPolicy);
        }
    }
}
