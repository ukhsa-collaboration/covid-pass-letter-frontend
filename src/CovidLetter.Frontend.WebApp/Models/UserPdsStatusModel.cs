namespace CovidLetter.Frontend.WebApp.Models
{
    public sealed class UserPdsStatusModel
    {
        public UserJourneyModel UserJourney { get; } = new UserJourneyModel();
        
        // empty constructor for TempData deserialization
        public UserPdsStatusModel()
        {
        }

        public UserPdsStatusModel(UserJourneyModel userJourney)
        {
            UserJourney = userJourney;
        }
    }
}
