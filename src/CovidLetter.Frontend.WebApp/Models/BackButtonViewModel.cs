namespace CovidLetter.Frontend.WebApp.Models;

public class BackButtonViewModel
{
    public string Action { get; set; } = "";

    public string Controller { get; set; } = "";

    public string HrefValue { get; set; } = "";

    public bool IsUsingHref { get; set; }
}