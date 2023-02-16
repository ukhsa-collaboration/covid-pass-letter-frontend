using System.Threading.Tasks;

namespace CovidLetter.Frontend.AccessTokenCache.Services
{
    public interface ITokenCacheService
    {
        Task<string> FetchTokenAsync();
    }
}
