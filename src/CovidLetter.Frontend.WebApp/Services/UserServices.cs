using System;

namespace CovidLetter.Frontend.WebApp.Services;

public static class UserServices
{
    public static bool UserIsUnder16(int? dobDay, int? dobMonth, int? dobYear)
    {
        var today = DateTime.Today;
        var age = today.Year - dobYear;

        /* hasn't had their birthday yet this year */
        if (today.Month < dobMonth || (today.Month == dobMonth && today.Day < dobDay))
        {
            age--;
        }

        return age < 16;
    }
}