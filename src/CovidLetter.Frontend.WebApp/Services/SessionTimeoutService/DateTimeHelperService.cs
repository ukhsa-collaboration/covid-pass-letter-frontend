using System;

namespace CovidLetter.Frontend.WebApp.Services.SessionTimeoutService
{
    public class DateTimeHelperService : IDateTimeHelperService
    {
        public long GetUtcDateTimeNowInTicks()
        {
            return DateTime.UtcNow.Ticks;
        }
    }
}