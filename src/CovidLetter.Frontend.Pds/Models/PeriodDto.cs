using Newtonsoft.Json;

namespace CovidLetter.Frontend.Pds.Models
{
    public class PeriodDto
    {
        [JsonProperty("start")]
        public string Start { get; set; } = string.Empty;

        [JsonProperty("end")]
        public string End { get; set; } = string.Empty;
    }
}
