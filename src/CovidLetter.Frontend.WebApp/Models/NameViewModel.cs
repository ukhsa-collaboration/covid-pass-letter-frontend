using System.ComponentModel.DataAnnotations;

namespace CovidLetter.Frontend.WebApp.Models
{
    public class NameViewModel
    {
        private string _firstName = "";
        private string _lastName = "";
        private const string NameRegex = "^[-.'\\sa-zA-Z\u00C0-\u024f\u1E00-\u1EFF]+$";

        [Required(ErrorMessage = "validationMissingFirstName")]
        [RegularExpression(NameRegex, ErrorMessage = "validationInvalidFirstName")]
        public string FirstName {
            get => _firstName;
            set => _firstName = value != null ? value.Trim() : string.Empty;
        }

        [Required(ErrorMessage = "validationMissingLastName")]
        [RegularExpression(NameRegex, ErrorMessage = "validationInvalidLastName")]
        public string LastName {
            get => _lastName;
            set => _lastName = value != null ? value.Trim() : string.Empty;
        }
    }
}