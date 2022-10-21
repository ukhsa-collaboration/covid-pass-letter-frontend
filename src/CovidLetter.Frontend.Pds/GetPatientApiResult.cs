using CovidLetter.Frontend.Pds.Models;

namespace CovidLetter.Frontend.Pds
{
    public class GetPatientApiResult
    {
        public ApiResponseStatus Status { get; }
        public PatientResponseDto ResponseData { get; }

        private GetPatientApiResult(ApiResponseStatus status,
            PatientResponseDto responseData)
        {
            Status = status;
            ResponseData = responseData;
        }

        public static GetPatientApiResult CreateOkResult(PatientResponseDto responseData)
        {
            return new GetPatientApiResult(ApiResponseStatus.Success, responseData);
        }

        public static GetPatientApiResult CreateNotFoundResult()
        {
            return new GetPatientApiResult(ApiResponseStatus.NotFound, null);
        }

        public static GetPatientApiResult CreateBadRequestResult()
        {
            return new GetPatientApiResult(ApiResponseStatus.BadRequest, null);
        }
        
        public static GetPatientApiResult CreateTooManyRequestsResult()
        {
            return new GetPatientApiResult(ApiResponseStatus.TooManyRequests, null);
        }

        public static GetPatientApiResult CreateOtherResult()
        {
            return new GetPatientApiResult(ApiResponseStatus.Other, null);
        }
    }
}