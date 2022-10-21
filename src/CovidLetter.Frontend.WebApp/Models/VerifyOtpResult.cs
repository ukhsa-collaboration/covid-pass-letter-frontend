using CovidLetter.Frontend.Search;
using CovidLetter.Frontend.WebApp.Services;
using System.Text;
using System.Web;

namespace CovidLetter.Frontend.WebApp.Models
{
    public class VerifyOtpResult
    {

        public bool otpSuccessfulMatch { get; }

        public bool expiredOtp { get; }

        public int remainingAttempts { get; }

        public VerifyOtpResult(bool otpSuccessfulMatch, int remainingAttempts, bool expiredOtp)
        {

            this.otpSuccessfulMatch = otpSuccessfulMatch;
            this.remainingAttempts = remainingAttempts;
            this.expiredOtp = expiredOtp;
        }   

    }
}
