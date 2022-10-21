using Newtonsoft.Json;

namespace CovidLetter.Frontend.Pds.Models
{
    public class SecurityDto
    {
        private const string CodeProperty = "code";

        [JsonProperty(CodeProperty)]
        public string Code { get; set; } = string.Empty;

    }
}
