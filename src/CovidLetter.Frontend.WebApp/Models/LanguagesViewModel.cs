using System.ComponentModel.DataAnnotations;
using CovidLetter.Frontend.WebApp.Constants;

namespace CovidLetter.Frontend.WebApp.Models
{
    public class LanguagesViewModel
    {
        [Required(ErrorMessage = "noSelection")]
        public string Language { get; set; }

        public string GetLanguageOrDefault() =>
            string.IsNullOrEmpty(Language) ? UIConstants.NotRequestedValue : Language;
    }
}