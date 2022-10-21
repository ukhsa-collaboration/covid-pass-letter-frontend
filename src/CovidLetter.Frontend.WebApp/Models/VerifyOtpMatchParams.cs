using System.Text;
using System.Web;

namespace CovidLetter.Frontend.WebApp.Models
{
    public class VerifyOtpMatchParams
    {
        public string PhoneNumber { get; }

        public string OtpCode { get; }

        public VerifyOtpMatchParams(string phoneNumber, string otpCode)
        {
            PhoneNumber = phoneNumber;
            OtpCode = otpCode;
        }
    }
}
