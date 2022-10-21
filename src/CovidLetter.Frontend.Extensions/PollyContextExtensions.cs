using Microsoft.Extensions.Logging;
using Polly;

namespace CovidLetter.Frontend.Extensions
{
    /// <summary>
    /// Polly Context extensions
    /// </summary>
    public static class PollyContextExtensions
    {
        private static readonly string LoggerKey = "ILogger";

        /// <summary>
        /// Get the ILogger to allow us to log within a Polly retry function
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ILogger? GetLogger(this Context context)
        {
            if (context.TryGetValue(LoggerKey, out object logger))
            {
                return logger as ILogger;
            }

            return null;
        }
    }
}