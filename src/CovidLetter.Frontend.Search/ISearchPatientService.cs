using System;
using System.Threading.Tasks;

namespace CovidLetter.Frontend.Search
{
    public interface ISearchPatientService
    {
        Task<SearchPatientResult> Search(
            SearchPatientRequest searchPatientRequest, string correlationId);

        Task<SearchPatientResult> Get(
            string nhsNumber, DateTime dateOfBirth, string postcode, string correlationId, bool postcodeIsWelsh, bool journeyIsDigital);
    }
}
