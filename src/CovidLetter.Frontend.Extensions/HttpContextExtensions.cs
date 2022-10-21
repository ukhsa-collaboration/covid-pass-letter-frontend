using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace CovidLetter.Frontend.Extensions
{
    public static class HttpContextExtensions
    {
        public static ITempDataDictionary? GetTempData(this HttpContext? context)
        {
            var factory = context?.RequestServices?.GetRequiredService<ITempDataDictionaryFactory>();
            return factory?.GetTempData(context);
        }
    }
}
