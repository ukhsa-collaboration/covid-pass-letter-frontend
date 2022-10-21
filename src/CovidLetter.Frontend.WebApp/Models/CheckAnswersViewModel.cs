using System;

namespace CovidLetter.Frontend.WebApp.Models
{
    public class CheckAnswersViewModel
    {
        public string BackLink { get; set; } = "";
        
        public string NhsNumber { get; set; } = "";

        public string Name { get; set; } = "";

        public string DateOfBirth { get; set; } = "";

        public string Postcode { get; set; } = "";

        public bool KnowsNhsNumber { get; set; }
    }
}