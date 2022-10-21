using System;
using System.Globalization;
using CovidLetter.Frontend.WebApp.Models;

namespace CovidLetter.Frontend.WebApp.Extensions
{
    public static class DateOfBirthViewModelExtensions
    {
        public static DateTime ToDateTime(this DateOfBirthViewModel model) => new(
            model.Year.GetValueOrDefault(),
            model.Month.GetValueOrDefault(),
            model.Day.GetValueOrDefault());

        public static string ToCustomDateString(this DateOfBirthViewModel model) =>
            model.ToDateTime().ToString("dd MMMM yyyy", CultureInfo.InvariantCulture);
    }
}