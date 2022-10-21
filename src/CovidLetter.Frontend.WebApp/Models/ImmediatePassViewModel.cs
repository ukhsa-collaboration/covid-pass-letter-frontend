using System.ComponentModel.DataAnnotations;

namespace CovidLetter.Frontend.WebApp.Models;

public class ImmediatePassViewModel
{
    public bool? UserRequestedDigitalPass { get; set; }

    public RequestFor? RequestFor { get; set; }

    public bool? UserHasMobileOnPdsRecord { get; set; }

    public bool? UserHasEmailOnPdsRecord { get; set; }

    public ImmediateVerifyMethod verifyMethod { get; set; }
    
}

public enum ImmediateVerifyMethod
{
    noneSelected,
    phone,
    email,
    noUplift
}