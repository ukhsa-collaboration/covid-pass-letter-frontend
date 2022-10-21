namespace CovidLetter.Frontend.WebApp.Models
{
    public sealed class PostPdsJourneyData
    {
        public UserJourneyModel UserJourney { get; } = new UserJourneyModel();
        
        // empty constructor for TempData deserialization
        public PostPdsJourneyData()
        {
        }

        public PostPdsJourneyData(UserJourneyModel userJourney)
        {
            UserJourney = userJourney;
        }
    }
}
