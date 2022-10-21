#nullable enable
using System;
using System.Globalization;

namespace CovidLetter.Frontend.WebApp.Extensions
{
    /// <summary>
    /// String extension methods.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Removes the substring "Controller" from a string.
        /// Very specific use case - when want to use static typing in Controllers for routing, ie. "nameof(HomeController).RemoveController()".
        /// </summary>
        /// <param name="str">The string to remove from.</param>
        /// <returns>The updated string.</returns>
        public static string RemoveController(this string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            return str.Replace("Controller", string.Empty, StringComparison.CurrentCulture);
        }
    }
}
