using System;
using System.Collections.Generic;
using CovidLetter.Frontend.Search;
using CovidLetter.Frontend.WebApp.Services;

namespace CovidLetter.Frontend.WebApp.Models
{
    public sealed class SearchResultData
    {
        public UserDetail UserDetail { get; } = new UserDetail();
        public Dictionary<Guid, string> Emails { get; } = new Dictionary<Guid, string>();
        public Dictionary<Guid, string> Mobiles { get; } = new Dictionary<Guid, string>();
        public Dictionary<Guid, Address> Addresses { get; } = new Dictionary<Guid, Address>();
        
        // empty constructor for TempData deserialization
        public SearchResultData()
        {
        }

        public SearchResultData(
            UserDetail userDetail,
            IEnumerable<string> emails,
            IEnumerable<string> mobiles,
            IEnumerable<Address> addresses)
        {
            UserDetail = userDetail;

            foreach (var email in emails)
            {
                Emails.Add(Guid.NewGuid(), email);
            }

            foreach (var mobile in mobiles)
            {
                Mobiles.Add(Guid.NewGuid(), mobile);
            }

            foreach (var address in addresses)
            {
                Addresses.Add(Guid.NewGuid(), address);
            }
        }

        public bool IsValidKey(Guid key)
        {
            return Emails.ContainsKey(key) || Mobiles.ContainsKey(key) || Addresses.ContainsKey(key);
        }
    }
}
