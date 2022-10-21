using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CovidLetter.Frontend.WebApp.Models;

public class RequestLetterForTravelModel
    : IValidatableObject
{
    public bool IsServiceBusy { get; set; }
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        return Enumerable.Empty<ValidationResult>();
    }
}