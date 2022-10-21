using System.ComponentModel.DataAnnotations;

namespace CovidLetter.Frontend.WebApp.Models
{
    public class PostcodeViewModel
    {
        private const string PostcodeRegex = @"^([a-zA-Z]{1,2}\d[a-zA-Z\d]? ?\d[a-zA-Z]{2}|GIR ?0A{2})$";
        private string _postcode = "";

        [Required(ErrorMessage = "missing")]
        [RegularExpression(PostcodeRegex, ErrorMessage = "wrongFormat")]
        public string Postcode
        {
            get => _postcode;
            set => _postcode = value != null ? value.Trim() : string.Empty;
        }
    }
}