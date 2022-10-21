using System;

namespace CovidLetter.Frontend.AccessTokenCache.Extensions
{
    public static class StringExtensions
    {
        public static byte[] ToByteArray(this string value) =>
            Convert.FromBase64String(value);
    }
}
