using System;
using CovidLetter.Frontend.WebApp.Models;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace CovidLetter.Frontend.WebApp.Extensions
{
    public static class TempDataExtensions
    {
        public static T? Get<T>(this ITempDataDictionary tempData) where T : class
            => tempData.TryPeek(typeof(T).Name, out var data) ? JsonConvert.DeserializeObject<T>(data) : null;

        public static void Set<T>(this ITempDataDictionary tempData, T incomingTempData) =>
            tempData[typeof(T).Name] = JsonConvert.SerializeObject(incomingTempData);

        public static string GetCorrelationId(this ITempDataDictionary tempData)
        {
            var correlationData = tempData.Get<CorrelationData>();

            if (correlationData == null)
            {
                correlationData = new CorrelationData(Guid.NewGuid());
                tempData.Set(correlationData);
            }

            return correlationData.CorrelationId;
        }

        private static bool TryPeek(this ITempDataDictionary tempData, string key, out string value)
        {
            object? peeked;
            try
            {
                peeked = tempData.Peek(key);
            }
            catch
            {
                // unable to retrieve value due to e.g. cryptographic failure
                peeked = null;
            }

            switch (peeked)
            {
                case string temp:
                    value = temp;
                    return true;
                default:
                    value = string.Empty;
                    return false;
            }
        }
    }
}