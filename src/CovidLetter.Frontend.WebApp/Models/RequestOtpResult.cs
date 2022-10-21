using CovidLetter.Frontend.Search;
using CovidLetter.Frontend.WebApp.Services;
using System.Text;
using System.Web;

namespace CovidLetter.Frontend.WebApp.Models
{
    public abstract class RequestOtpResult
    {

        public abstract T Accept<T>(IRequestOtpResultVisitor<T> visitor);

        public class Success : RequestOtpResult
        {
            public string MobileNumber { get; }

            public Success(string mobileNumber)
            {
                MobileNumber = mobileNumber;
            }

            public override T Accept<T>(IRequestOtpResultVisitor<T> visitor)
            {
                return visitor.Visit(this);
            }
        }
        public class Failed : RequestOtpResult
        {
            public string MobileNumber { get; }

            public Failed(string mobileNumber)
            {
                MobileNumber = mobileNumber;
            }

            public override T Accept<T>(IRequestOtpResultVisitor<T> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class TooManyRequests : RequestOtpResult
        {
            public string MobileNumber { get; }

            public TooManyRequests(string mobileNumber)
            {
                MobileNumber = mobileNumber;
            }

            public override T Accept<T>(IRequestOtpResultVisitor<T> visitor)
            {
                return visitor.Visit(this);
            }
        }
    }
}
