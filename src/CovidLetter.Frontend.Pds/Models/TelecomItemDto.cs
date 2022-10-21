using Newtonsoft.Json;

namespace CovidLetter.Frontend.Pds.Models
{
    public class TelecomItemDto
    {
        [JsonProperty("system")]
        public string System { get; set; } = string.Empty;

        [JsonProperty("use")]
        public string Use { get; set; } = string.Empty;

        [JsonProperty("value")]
        public string Value { get; set; } = string.Empty;
    }
}
