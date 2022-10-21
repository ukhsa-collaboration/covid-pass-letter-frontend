using System.Collections.Generic;
using Newtonsoft.Json;

namespace CovidLetter.Frontend.Pds.Models
{
    public class PdsPatientSearchResponseDto
    {
        [JsonProperty("entry")]
        public List<EntryDto> PatientDetails { get; set; } = new List<EntryDto>();
    }
}
