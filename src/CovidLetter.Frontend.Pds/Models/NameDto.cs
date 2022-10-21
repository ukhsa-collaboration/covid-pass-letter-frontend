using System.Collections.Generic;
using Newtonsoft.Json;

namespace CovidLetter.Frontend.Pds.Models
{
    public class NameDto
    {
        private const string FamilyProperty = "family";
        private const string GivenProperty = "given";
        private const string PrefixProperty = "prefix";
        private const string PeriodProperty = "period";
        private const string UseProperty = "use";

        [JsonProperty(FamilyProperty)]
        public string Family { get; set; } = string.Empty;

        [JsonProperty(GivenProperty)]
        public List<string> Given { get; set; } = new List<string>();

        [JsonProperty(PrefixProperty)]
        public List<string> Prefix { get; set; } = new List<string>();
                
        [JsonProperty(PeriodProperty)]
        public PeriodDto Period { get; set; } = new PeriodDto();

        [JsonProperty(UseProperty)]
        public string Use { get; set; }
    }
}
