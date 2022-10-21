namespace CovidLetter.Frontend.WebApp.Models;

public class UserJourneyModel
{
    public InitUserJourney? PrePdsJourney { get; set; }
    public PostPdsUserJourneyForDigitalUser? PostPdsDigitalUserJourney { get; set; }
    public bool UserHasTravelledThroughPds { get; set; }
    public bool userHasEmailAndPhoneNumber { get; set; }

}

public enum InitUserJourney
{
    Letter,
    Digital
}

public enum PostPdsUserJourneyForDigitalUser
{
    VerifyEmailWithGpRecord,
    VerifyMobileNumberWithGpRecord,
    NotYetDecided
}
