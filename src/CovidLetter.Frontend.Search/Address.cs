using System.Collections.Generic;

namespace CovidLetter.Frontend.Search
{
    public class Address
    {
        public List<string> Lines { get; set; } = new List<string>();

        public string PostalCode { get; set; } = string.Empty;
    }
}