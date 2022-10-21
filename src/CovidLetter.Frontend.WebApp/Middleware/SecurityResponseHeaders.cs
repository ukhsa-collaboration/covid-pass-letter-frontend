using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CovidLetter.Frontend.WebApp.Middleware
{
    public sealed class SecurityResponseHeaders
    {
        private readonly RequestDelegate _next;

        public SecurityResponseHeaders(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context)
        {

            var nonceValue = $"{Guid.NewGuid()}";
            context.Items["nonce"] = nonceValue;

            context.Response.Headers.Add("X-Xss-Protection", "1; mode=block");
            context.Response.Headers.Add("X-Frame-Options", "DENY");
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Add("Referrer-Policy", "no-referrer");
            context.Response.Headers.Add("Content-Security-Policy", $"default-src https:; script-src 'nonce-{nonceValue}' 'strict-dynamic'; object-src 'none'; style-src 'self' 'unsafe-inline'; frame-ancestors 'none'; form-action https:; base-uri 'self';img-src 'self' data:;");
            context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");

            await _next(context);
        }
    }
}
