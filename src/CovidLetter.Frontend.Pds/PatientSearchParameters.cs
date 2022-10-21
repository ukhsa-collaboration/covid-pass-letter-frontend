using System.Text;
using System.Web;

namespace CovidLetter.Frontend.Pds
{
    public class PatientSearchParameters
    {
        private const string ExactMatchIdentifier = "_exact-match";
        private const string HistoryIdentifier = "_history";
        private const string FuzzyMatchIdentifier = "_fuzzy-match";
        private const string MaxResultsIdentifier = "_max-results";
        private const string FamilyIdentifier = "family";
        private const string GivenIdentifier = "given";
        private const string BirthDateIdentifier = "birthdate";
        private const string AddressPostcodeIdentifier = "address-postcode";

        public bool ExactMatch { get; }
        public bool History { get; }
        public bool FuzzyMatch { get; }
        public int MaxResults { get; }
        public string Family { get; }
        public string Given { get; }

        // Assumption is that we will pass in correctly formatted date string 'yyyy-mm-dd'
        public string BirthDate { get; }
        public string AddressPostcode { get; }

        public string QueryString => BuildQueryString();

        private PatientSearchParameters(
            bool exactMatch,
            bool history,
            bool fuzzyMatch,
            int maxResults,
            string family,
            string given,
            string birthdate,
            string addressPostcode
        )
        {
            ExactMatch = exactMatch;
            History = history;
            FuzzyMatch = fuzzyMatch;
            MaxResults = maxResults;
            Family = family;
            Given = given;
            BirthDate = birthdate;
            AddressPostcode = addressPostcode;
        }

        public static PatientSearchParameters Create(
            string family,
            string given,
            string birthdate,
            string addressPostcode)
        {
            return new PatientSearchParameters(
                true,
                false,
                false,
                1,
                family,
                given,
                birthdate,
                addressPostcode);
        }

        private string BuildQueryString()
        {
            var sb = new StringBuilder("?");

            sb.Append($"{ExactMatchIdentifier}={ExactMatch.ToString().ToLowerInvariant()}");

            sb.Append($"&{HistoryIdentifier}={History.ToString().ToLowerInvariant()}");

            sb.Append($"&{FuzzyMatchIdentifier}={FuzzyMatch.ToString().ToLowerInvariant()}");

            sb.Append($"&{MaxResultsIdentifier}={MaxResults}");

            if (!string.IsNullOrWhiteSpace(Family))
            {
                sb.Append($"&{FamilyIdentifier}={HttpUtility.UrlEncode(Family)}");
            }

            if (!string.IsNullOrWhiteSpace(Given))
            {
                sb.Append($"&{GivenIdentifier}={HttpUtility.UrlEncode(Given)}");
            }

            if (!string.IsNullOrWhiteSpace(BirthDate))
            {
                sb.Append($"&{BirthDateIdentifier}=eq{HttpUtility.UrlEncode(BirthDate)}");
            }

            if (!string.IsNullOrWhiteSpace(AddressPostcode))
            {
                sb.Append($"&{AddressPostcodeIdentifier}={HttpUtility.UrlEncode(AddressPostcode)}");
            }

            return sb.ToString();
        }
    }
}
