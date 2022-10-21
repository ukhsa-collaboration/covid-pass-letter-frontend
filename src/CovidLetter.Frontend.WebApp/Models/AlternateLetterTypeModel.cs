namespace CovidLetter.Frontend.WebApp.Models
{
    public class AlternateLetterTypeModel
    {
        public bool? UserRequestedAccessibleFormat { get; set; }

        public string? AccessibleFormatType { get; set; }

        public bool? UserRequestedAnotherLanguage { get; set; }
    }
}