using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CovidLetter.Frontend.Pds.Models
{
    public class AddressItemDto
    {
        [JsonProperty("line")]
        public List<string> Line { get; set; } = new List<string>();

        [JsonProperty("period")]
        public PeriodDto Period { get; set; } = new PeriodDto();

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; } = string.Empty;

        [JsonProperty("use")]
        public string Use { get; set; } = string.Empty;

        [JsonIgnore]
        public List<string> ValidLine =>
          Line
            .Where(l => !string.IsNullOrEmpty(l))
            .ToList();
    }
}
