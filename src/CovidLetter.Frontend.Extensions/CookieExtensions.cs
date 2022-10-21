using System;
using Microsoft.AspNetCore.Http;

namespace CovidLetter.Frontend.Extensions
{
    public static class CookieExtensions
    {
        public static readonly string NhsSessionTimeoutCookieName = ".NHS.VaccineCertification.Cookie";
        
        public static void SetNhsSessionTimeoutCookie(HttpContext? context, int customSessionCookieExpiration)
        {
            if (context != null)
            {
                var cookieOptions = new CookieOptions
                {
                    Path = "/",
                    HttpOnly = true,
                    IsEssential = true,
                    MaxAge = TimeSpan.FromDays(1),
                    SameSite = SameSiteMode.Lax,
                    Secure = true,
                };

                context?.Response?.Cookies?.Append(
                    NhsSessionTimeoutCookieName, 
                    ConvertExpirationMinutesToTicksValue(customSessionCookieExpiration, DateTime.UtcNow.Ticks).ToString(), 
                    cookieOptions);
            }
        }
        
        public static bool HasSessionCookieExpired(string? cookieValueInTicks, long dateTimeUtcNowInTicks)
        {
            var cookieValueInTicksAsLong = Convert.ToInt64(cookieValueInTicks);
            
            // If the expiry time from the cookie is less than now then session has expired
            return cookieValueInTicksAsLong < dateTimeUtcNowInTicks;
        }
        
        public static string? GetCookieValueFromResponse(HttpResponse response, string cookieName)
        {
            foreach (var headers in response.Headers.Values)
            {
                foreach (var header in headers)
                {
                    if (header.StartsWith($"{cookieName}="))
                    {
                        var p1 = header.IndexOf('=');
                        var p2 = header.IndexOf(';');
                        return header.Substring(p1 + 1, p2 - p1 - 1);
                    }
                }
            }

            return null;
        }

        public static long ConvertExpirationMinutesToTicksValue(int customSessionCookieExpirationMinutes, long dateTimeUtcNowInTicks)
        {
            return dateTimeUtcNowInTicks + TimeSpan.FromMinutes(customSessionCookieExpirationMinutes).Ticks;
        }
    }
}