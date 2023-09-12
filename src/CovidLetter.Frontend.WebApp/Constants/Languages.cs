using System.Collections.Generic;
using System.Collections.Immutable;

namespace CovidLetter.Frontend.WebApp.Constants
{
    public class Languages
    {
        public static readonly ImmutableList<string> Codes = new List<string>()
        {
            "ar",
            "bn",
            "bg",
            "szh",
            "tzh",
            "fr",
            "gu",
            "hi",
            "it",
            "lt",
            "ne",
            "pl",
            "pt",
            "pa",
            "ro",
            "es",
            "ur"
        }.ToImmutableList();
    }
}