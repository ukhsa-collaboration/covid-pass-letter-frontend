using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace CovidLetter.Frontend.WebApp.Middleware
{
    public sealed class CacheResponseHeaders
    {
        private readonly RequestDelegate _next;

        public CacheResponseHeaders(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                context.Response.GetTypedHeaders().CacheControl =
                    new CacheControlHeaderValue
                    {
                        NoCache = true,
                        NoStore = true,
                        NoTransform = true,
                        Private = true,
                        MustRevalidate = true,
                    };

                context.Response.Headers[HeaderNames.Pragma] = "no-cache";

                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}