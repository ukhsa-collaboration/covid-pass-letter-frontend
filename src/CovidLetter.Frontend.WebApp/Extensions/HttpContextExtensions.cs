using CovidLetter.Frontend.Extensions;
using CovidLetter.Frontend.WebApp.Models;
using Microsoft.AspNetCore.Http;

namespace CovidLetter.Frontend.WebApp.Extensions;

public static class HttpContextExtensions
{
    public static bool RequestingForMyself(this HttpContext context)
    {
        var requestingFor = RequestingFor(context)?.RequestingFor ?? RequestFor.Myself;
        return requestingFor == RequestFor.Myself;
    }
    
    public static bool ParentalOrGuardianConsent(this HttpContext context)
    {
        var model = RequestingFor(context);
        return model is { RequestingFor: RequestFor.SomeoneElse, IsParentOrGuardian: true };
    }

    private static WhoAreYouRequestingForModel? RequestingFor(this HttpContext context)
    {
        var tempData = context.GetTempData();
        return tempData?.Get<UserSessionData>()?.RequestingFor;
    }
}
