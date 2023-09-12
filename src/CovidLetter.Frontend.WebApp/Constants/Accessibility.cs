using System.Collections.Generic;
using System.Collections.Immutable;

namespace CovidLetter.Frontend.WebApp.Constants
{
    public class Accessibility
    {
        public static readonly ImmutableList<string> Formats = new List<string>()
        {
            "Audio",
            "Braille",
            "Large print"
        }.ToImmutableList();
    }
}