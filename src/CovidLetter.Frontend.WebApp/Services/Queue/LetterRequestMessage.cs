using Newtonsoft.Json;

namespace CovidLetter.Frontend.WebApp.Services.Queue
{
    public class LetterRequestMessage
    {
        public string? NhsNumber { get; set; }

        public string? Title { get; set; }
        
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? DateOfBirth { get; set; }

        public string? CorrelationId { get; set; }

        public string? AddressLine1 { get; set; }

        public string? AddressLine2 { get; set; }

        public string? AddressLine3 { get; set; }

        public string? AddressLine4 { get; set; }

        public string? Postcode { get; set; }
        
        public string? EmailAddress { get; set; }
        
        public string? MobileNumber { get; set; }

        public string? AlternateLanguage { get; set; }

        public string? AccessibilityNeeds { get; set; }
        
        public string? Region { get; set; }

        [JsonIgnore]
        public ContactMethodEnum ContactMethodSettable { get; set; }

        public string[]? LetterType { get; set; }
    }
}