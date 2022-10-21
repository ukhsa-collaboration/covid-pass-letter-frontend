using System.Collections.Generic;

namespace CovidLetter.Frontend.Pds.Models;

public interface IResourceDto
{
    AddressItemDto[] Address { get; }
    string DeceasedDateTime { get; }
    MetaDto Meta { get; }
    List<TelecomItemDto> Telecom { get; }
}