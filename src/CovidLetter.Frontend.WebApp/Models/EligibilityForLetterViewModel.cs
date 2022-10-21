using System.ComponentModel.DataAnnotations;
namespace CovidLetter.Frontend.WebApp.Models;

public class EligibilityForLetterViewModel
{
    [Required(ErrorMessage = "validationMissingLetterType")]
    public LetterTypes? RequestedLetterType { get; set; }
}

public enum LetterTypes
{
    Audio,
    Braille,
    BigPrint,
    AnotherLanguage,
    None
}

public class LetterType
{
    public string Key { get; set; } = "";
    public LetterTypes Type { get; set; }
    public string LocalizerTextKey { get; set; } = "";
    public string LocalizerHintTextKey { get; set; } = "";
}