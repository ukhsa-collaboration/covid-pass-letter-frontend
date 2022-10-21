using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CovidLetter.Frontend.Extensions;
using CovidLetter.Frontend.Logging;
using CovidLetter.Frontend.Pds;
using CovidLetter.Frontend.Pds.Models;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("CovidLetter.Frontend.Search.Tests")]

namespace CovidLetter.Frontend.Search
{
    public class SearchPatientService : ISearchPatientService
    {
        private const int MinimumLinesForUsableAddress = 1;
        private readonly IPdsApiWrapperClient _pdsApiWrapperClient;
        private readonly ILogger<SearchPatientService> _logger;
        private static readonly Regex AcceptablePhoneNumberCharacters = new(@"[\s\(\)\-\+]");
        private static readonly Regex UnacceptablePhoneNumberDigits = new(@"[^\d]");
        private static readonly Regex ValidEmailAddress = new(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~\-]+@([^.@][^@\s]+)$");
        private static readonly Regex ValidEmailHostnamePart = new(@"^(xn|[a-z0-9]+)(-?-[a-z0-9]+)*$", RegexOptions.IgnoreCase);
        private static readonly Regex ValidEmailTldPart = new("^([a-z]{2,63}|xn--([a-z0-9]+-)*[a-z0-9]+)$", RegexOptions.IgnoreCase);

        private static readonly List<string> CounterPrefixes = new(){ "1", "7", "20", "27", "30", "31", "32", "33", "34", "36", "39", "40", "41", "43", "44", "45", "46", "47",
            "48", "49", "51", "52", "53", "54", "55", "56", "57", "58", "60", "61", "62", "63", "64", "65", "66", "81",
            "82", "84", "86", "90", "91", "92", "93", "94", "95", "98", "211", "212", "213", "216", "218", "220", "221",
            "222", "223", "224", "225", "226", "227", "228", "229", "230", "231", "232", "233", "234", "235", "236",
            "237", "238", "239", "240", "241", "242", "243", "244", "245", "246", "248", "249", "250", "251", "252",
            "253", "254", "255", "256", "257", "258", "260", "261", "262", "263", "264", "265", "266", "267", "268",
            "269", "297", "298", "299", "350", "351", "352", "353", "354", "355", "356", "357", "358", "359", "370",
            "371", "372", "373", "374", "375", "376", "377", "378", "380", "381", "382", "385", "386", "387", "389",
            "420", "421", "423", "500", "501", "502", "503", "504", "505", "506", "507", "508", "509", "590", "591",
            "592", "593", "594", "595", "596", "597", "598", "599", "670", "672", "673", "674", "675", "676", "677",
            "678", "679", "680", "682", "685", "687", "689", "691", "692", "852", "853", "855", "856", "880", "886",
            "960", "961", "962", "963", "964", "965", "966", "967", "968", "970", "971", "972", "973", "974", "975",
            "976", "977", "992", "993", "994", "995", "996", "998", "1242", "1246", "1264", "1268", "1284", "1345",
            "1441", "1473", "1649", "1664", "1684", "1721", "1758", "1767", "1784", "1868", "1869", "1876"};

        private static string UKPrefix = "44";

        public SearchPatientService(IPdsApiWrapperClient pdsApiWrapperClient, ILogger<SearchPatientService> logger)
        {
            _pdsApiWrapperClient = pdsApiWrapperClient;
            _logger = logger;
        }

        public async Task<SearchPatientResult> Search(SearchPatientRequest searchPatientRequest, string correlationId)
        {
            try
            {
                var searchParams = PatientSearchParameters.Create(
                    searchPatientRequest.LastName,
                    searchPatientRequest.FirstName,
                    searchPatientRequest.DateOfBirth.GetValueOrDefault().ToString("yyyy-MM-dd"),
                    searchPatientRequest.Postcode);

                var patientSearchResult =
                    await _pdsApiWrapperClient.SearchPatient(searchParams, correlationId);

                if (patientSearchResult.Status == ApiResponseStatus.TooManyRequests)
                {
                    return new SearchPatientResult.TooManyRequests();
                }
                else if (patientSearchResult.Status != ApiResponseStatus.Success)
                {
                    return new SearchPatientResult.NoMatches();
                }

                if (!(patientSearchResult.ResponseData?.PatientDetails?.Any()).GetValueOrDefault())
                {
                    return new SearchPatientResult.NoMatches();
                }

                var patientDetailsResponses = patientSearchResult.ResponseData?.PatientDetails?.ToList();

                if (patientDetailsResponses?.Count > 1)
                {
                    return new SearchPatientResult.MoreThanOneMatch();
                }

                var patientDetails = patientDetailsResponses?.Single();

                var patientResource = patientDetails?.Resource;

                if (string.IsNullOrEmpty(patientResource?.Id))
                {
                    return new SearchPatientResult.MatchedButNoNhsNumber();
                }

                if (IsDeceased(patientResource))
                {
                    return new SearchPatientResult.MatchedButDeceased();
                }

                if (IsRestricted(patientResource))
                {
                    return new SearchPatientResult.NoMatches();
                }

                var addresses = GetHomeAddresses(patientResource);
                var mobilePhoneNumbers = GetMobilePhoneNumbers(patientResource);
                var emailAddresses = GetEmailAddresses(patientResource);

                var userHasNoContactDetails = UserHasNoContactDetails(mobilePhoneNumbers, emailAddresses, addresses);

                if (userHasNoContactDetails)
                {
                    return new SearchPatientResult.NoContactDetailsFound();
                }

                var nameDetailResult = ParseNameDetailFromPatientResource(patientResource.Name.ToList());

                if (!nameDetailResult.isNameParseSuccessful)
                {
                    return new SearchPatientResult.NoValidNameDetailsFound();
                }

                var userDetail = GenerateUserDetails(patientResource.Id, searchParams.BirthDate, nameDetailResult.title, nameDetailResult.firstName, nameDetailResult.lastName);

                if (searchPatientRequest.userIsDigitalJourney)
                    return ProcessUserForDigitalJourney(
                        searchPatientRequest.userHasWelshPostcode, 
                        userDetail,
                        mobilePhoneNumbers,
                        emailAddresses,
                        addresses
                    );

                return ProcessUserForLetterJourney(
                    searchPatientRequest.userHasWelshPostcode,
                    userDetail,
                    mobilePhoneNumbers,
                    emailAddresses,
                    addresses
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(AppEventId.FailedPdsMatch, ex, "PDS demographic lookup failed");
                throw;
            }
        }

        public async Task<SearchPatientResult> Get(
            string nhsNumber,
            DateTime dateOfBirth,
            string postcode,
            string correlationId,
            bool postcodeIsWelsh,
            bool journeyIsDigital)
        {
            try
            {
                var getPatientResult = await _pdsApiWrapperClient.GetPatientByNhsNumber(nhsNumber, correlationId);

                if (getPatientResult != null)
                {
                    if (getPatientResult.Status == ApiResponseStatus.Success)
                    {
                        if (IsDeceased(getPatientResult.ResponseData))
                        {
                            // Flagged as deceased, return No Match
                            return new SearchPatientResult.MatchedButDeceased();
                        }

                        if (IsRestricted(getPatientResult.ResponseData))
                        {
                            // Flagged as a restricted user
                            return new SearchPatientResult.NoMatches();
                        }

                        var patientResource = getPatientResult.ResponseData;
                        var addresses = GetHomeAddresses(patientResource);
                        var mobilePhoneNumbers = GetMobilePhoneNumbers(patientResource);
                        var emailAddresses = GetEmailAddresses(patientResource);

                        var userHasNoContactDetails = UserHasNoContactDetails(mobilePhoneNumbers, emailAddresses, addresses);

                        if (userHasNoContactDetails)
                        {
                            return new SearchPatientResult.NoContactDetailsFound();
                        }

                        var nameDetailResult = ParseNameDetailFromPatientResource(patientResource.Name.ToList());

                        if (!nameDetailResult.isNameParseSuccessful)
                        {
                            return new SearchPatientResult.NoValidNameDetailsFound();
                        }

                        var userDetailsMatchPds = IsMatch(getPatientResult, dateOfBirth, postcode);

                        if (userDetailsMatchPds)
                        {
                            var userDetail = GenerateUserDetails(nhsNumber, dateOfBirth.ToString("yyyy-MM-dd"), nameDetailResult.title, nameDetailResult.firstName, nameDetailResult.lastName);

                            if (journeyIsDigital)
                                return ProcessUserForDigitalJourney(
                                    postcodeIsWelsh,
                                    userDetail,
                                    mobilePhoneNumbers,
                                    emailAddresses,
                                    addresses
                                );

                            return ProcessUserForLetterJourney(
                                postcodeIsWelsh,
                                userDetail,
                                mobilePhoneNumbers,
                                emailAddresses,
                                addresses
                            );
                        }
                    }
                }
                else if (getPatientResult.Status == ApiResponseStatus.TooManyRequests)
                {
                    return new SearchPatientResult.TooManyRequests();
                }

                return new SearchPatientResult.NoMatches();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    AppEventId.FailedPdsMatch, ex, "PDS NHS Number lookup failed");
                throw;
            }
        }

        internal IList<Address> GetHomeAddresses(IResourceDto patientResource)
        {
            // According to the PDS FHIR API docs the Address array should always be returned, even for restricted
            // patients, but the error logs suggest otherwise. In this case return an empty list, which will gracefully
            // prevent the user from completing their letter request with the `We cannot find your details` error.
            if (patientResource.Address == null)
            {
                return new List<Address>();
            }

            return patientResource.Address
                .Where(x => x.Use == "home" && IsUsableAddress(x))
                .Select(x => new Address { Lines = x.ValidLine, PostalCode = x.PostalCode })
                .ToList();
        }

        private static IList<string> GetMobilePhoneNumbers(IResourceDto patientResource)
        {
            return patientResource.Telecom
                .Where(x => x.System == "phone" && x.Use == "mobile" && (IsValidUkMobilePhoneNumber(x.Value) || IsValidInternationalPhoneNumber(x.Value)))
                .Select(x => x.Value)
                .ToList();
        }

        private static IList<string> GetEmailAddresses(IResourceDto patientResource)
        {
            return patientResource.Telecom
                .Where(x => x.System == "email" && IsValidEmailAddress(x.Value))
                .Select(x => x.Value)
                .ToList();
        }

        private static bool IsMatch(GetPatientApiResult getPatientResult, DateTime dateOfBirth, string postcode)
        {
            return getPatientResult.ResponseData.Birthdate.IsEqualToDateTime(dateOfBirth) && getPatientResult.ResponseData.PostalCodeExists(postcode);
        }

        private bool IsDeceased(IResourceDto patientResource)
        {
            if (!string.IsNullOrWhiteSpace(patientResource.DeceasedDateTime))
            {
                _logger.LogInformation("Patient is marked as deceased");
                return true;
            }

            return false;
        }

        private bool IsRestricted(IResourceDto patientResource)
        {
            if (patientResource.Meta?.Security?[0]?.Code != null &&
                patientResource.Meta.Security[0].Code.Equals("R"))
            {
                _logger.LogInformation("Patient restricted");
                return true;
            }

            return false;
        }

        private bool IsUsableAddress(AddressItemDto address)
        {
            if (address.ValidLine.Count < MinimumLinesForUsableAddress)
            {
                _logger.LogInformation("Address does not have enough lines to support a notification");
                return false;
            }

            if (!IsEntityEndDateValid(address.Period.End, "Address"))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(address.PostalCode))
            {
                _logger.LogInformation("Address excluded due to no postal code");
                return false;
            }

            return true;
        }

        private (bool isNameParseSuccessful, string firstName, string lastName, string title) ParseNameDetailFromPatientResource(List<NameDto> nameDtos)
        {
            var validNameDtos = nameDtos.Where(x => IsEntityEndDateValid(x.Period.End, "Name")).ToList();

            if (validNameDtos.Count < 1)
            {
                return (false, string.Empty, string.Empty, string.Empty);
            }

            NameDto validNameDto = null;

            if (validNameDtos.Count > 1)
            {
                validNameDto = validNameDtos.FirstOrDefault(x => x.Use == "usual") ??
                                   validNameDtos.FirstOrDefault();
            }
            else
            {
                validNameDto = validNameDtos.Single();
            }

            var isGivenNameValid = ParseGivenName(validNameDto?.Given);

            return (isGivenNameValid.isValid, isGivenNameValid.givenName, validNameDto?.Family,
                validNameDto?.Prefix?.FirstOrDefault() ?? string.Empty);
        }

        private bool IsEntityEndDateValid(string periodEnd, string entityName)
        {
            if (!string.IsNullOrWhiteSpace(periodEnd))
            {
                if (!DateTime.TryParseExact(periodEnd, "yyyy-MM-dd", null, DateTimeStyles.None,
                    out var endDate))
                {
                    _logger.LogInformation(
                        "{EntityName} excluded due to period end having an invalid format", entityName);
                    return false;
                }

                if (endDate < DateTime.UtcNow)
                {
                    _logger.LogInformation(
                        "{EntityName} excluded due to period end in the past", entityName);

                    return false;
                }
            }

            return true;
        }

        private (bool isValid, string givenName) ParseGivenName(List<string> givenNames)
        {
            if (givenNames.Count == 0)
            {
                _logger.LogInformation("Name excluded due to no given name");
                return (false, string.Empty);
            }

            return (true, givenNames.FirstOrDefault());
        }

        /// <summary>
        /// Validates if the given value is a valid UK mobile number, following the validation logic used by Gov.Notify:
        /// https://github.com/alphagov/notifications-utils/blob/master/notifications_utils/recipients.py#L583
        /// </summary>
        /// <param name="value">The mobile number</param>
        /// <returns>True if the number is valid; otherwise false</returns>
        public static bool IsValidUkMobilePhoneNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var number = NormalisePhoneNumber(value);
            number = RemoveUKPrefix(number);

            return !UnacceptablePhoneNumberDigits.IsMatch(number) && number.StartsWith("7") && number.Length == 10;
        }

        public static string ConvertUkNumberToInternationalFormat(string value)
        {
            var number = NormalisePhoneNumber(value);
            number = RemoveUKPrefix(number);

            return "+" + UKPrefix + number;
        }

        private static string RemoveUKPrefix(string input)
        {
            return input.TrimStart(UKPrefix)!.TrimStart("0")!.TrimStart(UKPrefix)!;
        }

        private static string NormalisePhoneNumber(string value)
        {
            return AcceptablePhoneNumberCharacters.Replace(value, string.Empty).TrimStart('0');
        }

        /// <summary>
        /// Validates if the given value is a valid email address, following the validation logic used by Gov.Notify:
        /// https://github.com/alphagov/notifications-utils/blob/master/notifications_utils/recipients.py#L634
        /// </summary>
        /// <param name="value">The email address</param>
        /// <returns>True if the email address is valid; otherwise false</returns>
        internal static bool IsValidEmailAddress(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var emailAddress = value.Trim();
            var match = ValidEmailAddress.Match(emailAddress);
            if (!match.Success || emailAddress.Length > 320 || emailAddress.Contains(".."))
            {
                return false;
            }

            var hostname = match.Groups[1].Value;
            var idn = new IdnMapping();
            try
            {
                hostname = idn.GetAscii(hostname);
            }
            catch
            {
                // Decoded string is not a valid IDN name.
                return false;
            }

            var hostnameParts = hostname.Split('.');
            if (hostname.Length > 253 || hostnameParts.Length < 2)
            {
                return false;
            }

            if (hostnameParts.Any(part =>
                    string.IsNullOrWhiteSpace(part)
                    || part.Length > 63
                    || !ValidEmailHostnamePart.IsMatch(part)))
            {
                return false;
            }

            return ValidEmailTldPart.IsMatch(hostnameParts[^1]);
        }

        /// <summary>
        /// Validates if the given value is a valid International mobile number, following the validation logic used by Gov.Notify:
        /// https://github.com/alphagov/notifications-utils/blob/master/notifications_utils/recipients.py#L583
        /// </summary>
        /// <param name="value">The mobile number</param>
        /// <returns>True if the number is valid; otherwise false</returns>
        public static bool IsValidInternationalPhoneNumber(string value)
        {
            var normalised = NormalisePhoneNumber(value);

            if (string.IsNullOrEmpty(normalised)) return false;

            if (UnacceptablePhoneNumberDigits.IsMatch(normalised)) return false;

            if (normalised.Length < 8) return false;
            if (normalised.Length > 15) return false;

            var prefix = GetInternationalPrefix(normalised);

            if (string.IsNullOrEmpty(prefix)) return false;

            if (prefix.Equals(UKPrefix) && (!normalised.TrimStart(UKPrefix).StartsWith("7") || normalised.TrimStart(UKPrefix).Length != 10)) return false;

            return true;
        }

        private static string GetInternationalPrefix(string normalised)
        {
            return CounterPrefixes.FirstOrDefault(normalised.StartsWith);
        }

        public static bool NumbersAreSame(string left, string right)
        {
            if (IsValidUkMobilePhoneNumber(left) && IsValidUkMobilePhoneNumber(right))
            {
                left = ConvertUkNumberToInternationalFormat(left);
                right = ConvertUkNumberToInternationalFormat(right);
                return InternationalNumbersAreSame(left, right);
            }

            if (IsValidInternationalPhoneNumber(left) && IsValidInternationalPhoneNumber(right))
            {
                return InternationalNumbersAreSame(left, right);
            }

            return false;
        }

        private static bool InternationalNumbersAreSame(string left, string right)
        {
            return NormalisePhoneNumber(left).Equals(NormalisePhoneNumber(right));
        }

        private static SearchPatientResult ProcessUserForDigitalJourney(
            bool userHasWelshPostcode,
            UserDetail userDetail,
            IList<string> mobilePhoneNumbers,
            IList<string> emailAddresses,
            IList<Address> addresses
        ) {
            var userHasAMobileNumber = mobilePhoneNumbers.Any();
            var userHasAnEmailAddress = emailAddresses.Any();

            if (userHasWelshPostcode)
                return new SearchPatientResult.MatchFoundButWelshPostcode();
            
            if (userHasAMobileNumber && userHasAnEmailAddress)
            {
                return new SearchPatientResult.SuccessWithMobileAndEmail(userDetail: userDetail,
                    mobiles: mobilePhoneNumbers,
                    emails: emailAddresses,
                    addresses: addresses
                );
            }

            if (userHasAMobileNumber)
                return new SearchPatientResult.SuccessWithMobile(userDetail: userDetail,
                    mobiles: mobilePhoneNumbers,
                    emails: emailAddresses,
                    addresses: addresses
                );

            if (userHasAnEmailAddress)
                return new SearchPatientResult.SuccessWithEmail(userDetail: userDetail,
                    mobiles: mobilePhoneNumbers,
                    emails: emailAddresses,
                    addresses: addresses
                );

            return new SearchPatientResult.MatchFoundButNoContactDetails();
        }

        private SearchPatientResult ProcessUserForLetterJourney(
            bool userHasWelshPostcode,
            UserDetail userDetail,
            IList<string> mobilePhoneNumbers,
            IList<string> emailAddresses,
            IList<Address> addresses
        ) {
            var userHasAMobileNumber = mobilePhoneNumbers.Any();
            var userHasAnEmailAddress = emailAddresses.Any();

            if (userHasWelshPostcode)
                return new SearchPatientResult.SuccessWithoutAccessibility(userDetail: userDetail,
                    mobiles: mobilePhoneNumbers,
                    emails: emailAddresses,
                    addresses: addresses
                );

            if (userHasAMobileNumber || userHasAnEmailAddress)
                return new SearchPatientResult.ImmediatePass(userDetail: userDetail,
                    mobiles: mobilePhoneNumbers,
                    emails: emailAddresses,
                    addresses: addresses
                );
            
            return new SearchPatientResult.SuccessWithAccessibility(userDetail: userDetail,
                mobiles: mobilePhoneNumbers,
                emails: emailAddresses,
                addresses: addresses
            );
        }

        private static UserDetail GenerateUserDetails(string nhsNumber, string dateOfBirth, string title, string forename, string surname)
        {
            return new UserDetail()
            {
                NHSNumber = nhsNumber,
                DoB = dateOfBirth,
                Title = title,
                FirstName = forename,
                LastName = surname
            };
        }

        private static bool UserHasNoContactDetails(IList<string> mobilePhoneNumbers, IList<string> emailAddresses, IList<Address> addresses) 
        {
            return !mobilePhoneNumbers.Any() && !emailAddresses.Any() && !addresses.Any();
        }
    }
}
