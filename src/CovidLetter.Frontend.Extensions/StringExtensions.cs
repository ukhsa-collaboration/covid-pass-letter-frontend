
namespace CovidLetter.Frontend.Extensions
{
    /// <summary>
    /// String extension methods.
    /// </summary>
    public static class StringExtensions
    {


        /// <summary>
        /// Fluent syntax for the standard string IsNullOrEmpty check.
        /// </summary>
        /// <param name="input">The string.</param>
        /// <returns>Whether it IsNullOrEmpty.</returns>
        public static bool IsNullOrEmpty(this string input)
        {
            return string.IsNullOrEmpty(input);
        }

        /// <summary>
        /// Fluent syntax for the standard string TrimStart function.
        /// </summary>
        /// <param name="target">The target string.</param>
        /// <param name="trimChars">The chars to trim from the start.</param>
        /// <returns>The trimmed string.</returns>
        public static string? TrimStart(this string target, string trimChars)
        {
            return target?.TrimStart(trimChars?.ToCharArray());
        }

        public static string RemoveWhiteSpace(this string s) => s.Trim().Replace(" ", "");
    }
}
