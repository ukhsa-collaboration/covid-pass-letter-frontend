using CovidLetter.Frontend.WebApp.Models;

namespace CovidLetter.Frontend.WebApp.Services;

public static class ViewServices
{
    public static BackButtonViewModel GenerateBackButtonModelUsingControllerAndAction(string action, string controller)
    {
        return new BackButtonViewModel() {
            Action = action,
            Controller = controller,
            IsUsingHref = false,
            HrefValue = ""
        };
    }

    public static BackButtonViewModel GenerateBackButtonModelUsingHref(string hrefLink)
    {
        return new BackButtonViewModel()
        {
            Action = "",
            Controller = "",
            IsUsingHref = true,
            HrefValue = hrefLink
        };
    }
}