using System.Collections.Generic;
using System.Linq;
using CovidLetter.Frontend.Extensions;
using Newtonsoft.Json;

namespace CovidLetter.Frontend.Pds.Models
{
    public class PatientResponseDto : IResourceDto
    {
        private const string AddressProperty = "address";
        private const string BirthDateProperty = "birthDate";
        private const string DeceasedDateTimeProperty = "deceasedDateTime";
        private const string NameProperty = "name";
        private const string MetaProperty = "meta";
        private const string TelecomProperty = "telecom";

        [JsonProperty(AddressProperty)]
        public AddressItemDto[] Address { get; set; }

        [JsonProperty(BirthDateProperty)]
        public string Birthdate { get; set; }

        [JsonProperty(DeceasedDateTimeProperty)]
        public string DeceasedDateTime { get; set; }

        [JsonProperty(NameProperty)]
        public NameDto[] Name { get; set; }

        [JsonProperty(MetaProperty)]
        public MetaDto Meta { get; set; }

        [JsonProperty(TelecomProperty)]
        public List<TelecomItemDto> Telecom { get; set; } = new List<TelecomItemDto>();

        public bool PostalCodeExists(string postalCode) =>
            Address.Any(a =>
                a.PostalCode.RemoveWhiteSpace().ToUpper() == postalCode.RemoveWhiteSpace().ToUpper());
    }
}
