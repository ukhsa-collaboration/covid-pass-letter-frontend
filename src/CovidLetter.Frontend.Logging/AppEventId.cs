using Microsoft.Extensions.Logging;

namespace CovidLetter.Frontend.Logging
{
    public static class AppEventId
    {
        public static EventId SuccessfulPdsMatch { get; } = new(2000, nameof(SuccessfulPdsMatch));

        public static EventId FailedPdsMatch { get; } = new(4000, nameof(FailedPdsMatch));

        public static EventId ContactDetailsNotFound { get; } = new(4041, nameof(ContactDetailsNotFound));

        public static EventId QueueRetry { get; } = new(4290, nameof(QueueRetry));

        public static EventId ErrorObfuscatingEmail { get; } = new(5000, nameof(ErrorObfuscatingEmail));

        public static EventId ErrorObfuscatingMobile { get; } = new(5001, nameof(ErrorObfuscatingMobile));

        public static EventId PdsTooManyRequests { get; } = new(4291, nameof(PdsTooManyRequests));
        
        public static EventId InvalidUserSessionData { get; } = new(5010, nameof(InvalidUserSessionData));
    }
}