using System;

namespace CovidLetter.Frontend.Extensions
{
    public static class DateTimeExtensions
    {
        public static bool IsEqualToDateTime(this string stringDateTime, DateTime dateTime) =>
            DateTime.TryParse(stringDateTime, out var stringAsDateTime) &&
            stringAsDateTime == dateTime;
    }
}