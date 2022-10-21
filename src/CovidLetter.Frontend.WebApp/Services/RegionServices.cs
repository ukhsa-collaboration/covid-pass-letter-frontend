using System;
using System.Linq;
using CovidLetter.Frontend.WebApp.Controllers;
using CovidLetter.Frontend.WebApp.Models;

namespace CovidLetter.Frontend.WebApp.Services;

public static class RegionServices
{
    private static readonly string[] WelshPostcodes = { "LL", "SY", "LD", "SA", "CF", "NP" };

    private const string Wales = "WALES";
    private const string England = "ENG";
    private const string IsleOfMan = "IM";

    /// <summary>
    /// Returns whether the requested postcode is in Wales, based on the first two characters.
    /// </summary>
    /// <remarks>
    /// <p>
    /// The default should be the assumption that the user is <em>not</em> in Wales.
    /// </p>
    /// <p>
    /// In some circumstances <see cref="UserSessionData" /> in TempData becomes partially corrupted causing the
    /// <see cref="PostcodeViewModel" /> to be nullified. The user experience should not be adversely affected by this
    /// issue that may be related to an infrastructure problem. Sufficient calls to this method by the time the user
    /// reaches the <see cref="HomeController.CheckAnswers()" /> action should enable the correct visibility on actions
    /// and default values in models.
    /// </p>
    /// </remarks>
    /// <param name="postcode">The postcode.</param>
    /// <returns><c>true</c> if in Wales, otherwise <c>false</c></returns>
    public static bool PostcodeIsInWales(string? postcode)
    {
        if (postcode == default)
        {
            return false;
        }
        
        return GetRegionFromPostcode(postcode) == Wales;
    }
    
    public static string GetRegionFromPostcode(string? postcode)
    {
        ArgumentNullException.ThrowIfNull(postcode);

        if (postcode.StartsWith("im", StringComparison.InvariantCultureIgnoreCase))
        {
            return IsleOfMan;
        }

        return WelshPostcodes.Any(p => postcode.StartsWith(p, StringComparison.InvariantCultureIgnoreCase)) 
            ? Wales 
            : England;
    }
}