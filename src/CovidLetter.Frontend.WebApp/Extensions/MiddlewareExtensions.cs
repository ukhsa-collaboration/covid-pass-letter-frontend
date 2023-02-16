using CovidLetter.Frontend.WebApp.Middleware;
using Microsoft.AspNetCore.Builder;

namespace CovidLetter.Frontend.WebApp.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseCacheResponseHeaders(this IApplicationBuilder builder) =>
            builder.UseMiddleware<CacheResponseHeaders>();

        public static IApplicationBuilder UseSecurityResponseHeaders(this IApplicationBuilder builder) =>
            builder.UseMiddleware<SecurityResponseHeaders>();
    }
}
