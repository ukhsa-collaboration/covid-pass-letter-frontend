using System;

namespace CovidLetter.Frontend.Search
{
    public class SearchPatientRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Postcode { get; set; } = string.Empty;
        public bool userHasWelshPostcode { get; set; } = false;
        public bool userIsDigitalJourney { get; set; }
    }
}
