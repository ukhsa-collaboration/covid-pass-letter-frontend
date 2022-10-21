using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CovidLetter.Frontend.Extensions
{
    public static class ModelStateDictionaryExtensions
    {
        public static bool HasError(this ModelStateDictionary modelStateDictionary, string fieldName)
            => !string.IsNullOrWhiteSpace(fieldName)
               && modelStateDictionary.TryGetValue(fieldName, out var entry)
               && (entry.Errors?.Any() ?? false);
    }
}