using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CovidLetter.Frontend.WebApp.Models
{
    public class EmailOrPhoneNumberChoiceViewModel
    {
        public VerifyMethod? verifyMethod { get; set; }
    }
    public enum VerifyMethod
    {
        Email,
        PhoneNumber
    }
}
