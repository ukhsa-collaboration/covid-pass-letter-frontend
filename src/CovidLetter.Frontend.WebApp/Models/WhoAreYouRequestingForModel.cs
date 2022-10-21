using System.ComponentModel.DataAnnotations;

namespace CovidLetter.Frontend.WebApp.Models;

public class WhoAreYouRequestingForModel
{
    [Required(ErrorMessage = "validationMissingRequestingFor")]
    public RequestFor? RequestingFor { get; set; }

    public bool IsParentOrGuardian { get; set; }
}

public enum RequestFor
{
    Myself,
    SomeoneElse
}