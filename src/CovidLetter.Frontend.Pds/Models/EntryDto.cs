using Newtonsoft.Json;

namespace CovidLetter.Frontend.Pds.Models
{
    public class EntryDto
    {

        [JsonProperty("resource")]
        public ResourceDto Resource { get; set; } = new ResourceDto();
    }
}