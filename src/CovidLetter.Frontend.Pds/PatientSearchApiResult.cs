using CovidLetter.Frontend.Pds.Models;

namespace CovidLetter.Frontend.Pds
{
    public class PatientSearchApiResult
    {
        public ApiResponseStatus Status { get; }
        public PdsPatientSearchResponseDto ResponseData { get; }

        private PatientSearchApiResult(ApiResponseStatus status, PdsPatientSearchResponseDto responseData)
        {
            Status = status;
            ResponseData = responseData;
        }

        public static PatientSearchApiResult CreateOkResult(PdsPatientSearchResponseDto responseData)
        {
            return new PatientSearchApiResult(ApiResponseStatus.Success, responseData);
        }

        public static PatientSearchApiResult CreateTooManyRequestsResult()
        {
            return new PatientSearchApiResult(ApiResponseStatus.TooManyRequests, null);
        }

        public static PatientSearchApiResult CreateNotFoundResult()
        {
            return new PatientSearchApiResult(ApiResponseStatus.NotFound, null);
        }
    }
}
