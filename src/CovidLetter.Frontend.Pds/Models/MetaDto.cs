using System.Collections.Generic;
using Newtonsoft.Json;

namespace CovidLetter.Frontend.Pds.Models
{
    public class MetaDto
    {
        private const string SecurityProperty = "security";

        [JsonProperty(SecurityProperty)]
        public List<SecurityDto> Security { get; set; } = new List<SecurityDto>();
    }
}
