using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CovidLetter.Frontend.Pds.Models
{
    public class ResourceDto : IResourceDto
    {
        [JsonProperty("address")]
        public AddressItemDto[] Address { get; set; } = Array.Empty<AddressItemDto>();

        [JsonProperty("deceasedDateTime")]
        public string DeceasedDateTime { get; set; } = string.Empty;

        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("name")]
        public List<NameDto> Name { get; set; } = new List<NameDto>();
        
        [JsonProperty("meta")]
        public MetaDto Meta { get; set; }

        [JsonProperty("telecom")]
        public List<TelecomItemDto> Telecom { get; set; } = new List<TelecomItemDto>();
    }
}