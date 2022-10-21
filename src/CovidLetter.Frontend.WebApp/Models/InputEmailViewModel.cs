using System;
using System.ComponentModel.DataAnnotations;
using CovidLetter.Frontend.WebApp.Models.Validation;

namespace CovidLetter.Frontend.WebApp.Models
{
    public class InputEmailViewModel
    {
        private string _emailAddress = "";
        private string _emailAddressConfirm = "";

        [EmailValidator]
        public string EmailAddress
        {
            get => _emailAddress;
            set => _emailAddress = value != null ? value.Trim() : string.Empty;
        }

        public string EmailAddressConfirm
        {
            get => _emailAddressConfirm;
            set => _emailAddressConfirm = value != null ? value.Trim() : string.Empty;
        }
    }
}