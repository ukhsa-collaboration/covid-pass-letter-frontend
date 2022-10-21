using System.ComponentModel.DataAnnotations;
using CovidLetter.Frontend.WebApp.Models.Validation;

namespace CovidLetter.Frontend.WebApp.Models
{
    public class NhsNumberViewModel
    {
        private string _nhsNumber = "";

        [Required(ErrorMessage = "validationMissingKnowsNhsNumber")]
        public bool? KnowsNhsNumber { get; set; }

        [NhsNumberValidator]
        public string NhsNumber
        {
            get => _nhsNumber;
            set => _nhsNumber = value != null ? value.Trim() : string.Empty;
        }
    }
}