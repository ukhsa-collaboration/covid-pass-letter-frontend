using CovidLetter.Frontend.WebApp.Models;

namespace CovidLetter.Frontend.WebApp.Services
{
    public interface IRequestOtpResultVisitor<out T>
    {
        T Visit(RequestOtpResult.Success result);

        T Visit(RequestOtpResult.Failed result);

        T Visit(RequestOtpResult.TooManyRequests tooManyRequests);
    }
}
