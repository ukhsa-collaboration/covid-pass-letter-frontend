using System;

namespace CovidLetter.Frontend.WebApp.Models
{
    public class CorrelationData
    {
        public CorrelationData() 
        {
            CorrelationId = "";
        }

        public CorrelationData(Guid correlationId)
        {
            CorrelationId = correlationId.ToString();
        }

        public string CorrelationId { get; set; }
    }
}
