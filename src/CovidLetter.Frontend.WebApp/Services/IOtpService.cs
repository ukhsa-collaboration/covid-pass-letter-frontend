using CovidLetter.Frontend.Pds;
using CovidLetter.Frontend.WebApp.Models;
using System.Threading.Tasks;

namespace CovidLetter.Frontend.WebApp.Services
{
    public interface IOtpService
    {
        Task<VerifyOtpResult> VerifyOtpMatch(VerifyOtpMatchParams verifyOtpMatchParams,
            string correlationId);

        Task<RequestOtpResult> RequestOtp(string mobileNumber,
    string correlationId);
    }
}
