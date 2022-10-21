using System.Threading.Tasks;

namespace CovidLetter.Frontend.Pds
{
    public interface IPdsApiWrapperClient
    {
        Task<GetPatientApiResult> GetPatientByNhsNumber(
            string nhsNumber, string correlationId);

        Task<PatientSearchApiResult> SearchPatient(
            PatientSearchParameters pdsPatientSearchParameters, string correlationId);
    }
}
