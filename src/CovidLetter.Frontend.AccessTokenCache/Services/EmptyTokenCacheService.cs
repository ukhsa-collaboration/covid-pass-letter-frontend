using System.Threading.Tasks;

namespace CovidLetter.Frontend.AccessTokenCache.Services;

public class EmptyTokenCacheService : ITokenCacheService
{
    public Task<string> FetchTokenAsync()
    {
        return Task.FromResult("empty");
    }
}