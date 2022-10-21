using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CovidLetter.Frontend.AccessTokenCache.Models;
using CovidLetter.Frontend.AccessTokenCache.Options;
using Marvin.StreamExtensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CovidLetter.Frontend.AccessTokenCache.Services
{
    public class TokenCacheService : ITokenCacheService
    {
        private const int CacheTimeoutInSeconds = 30;
        private const int JwtLifespanInMinutes = 5;
        private const string MemoryCacheTokenAccessor = "TOKEN";

        private readonly OAuthOptions _configuration;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;

        public TokenCacheService(
            HttpClient client,
            IOptions<OAuthOptions> options,
            IMemoryCache memoryCache)
        {
            _httpClient = client;
            _configuration = options.Value;
            _memoryCache = memoryCache;

            client.BaseAddress = new Uri(_configuration.TokenEndpoint);
            client.Timeout = new TimeSpan(0, 0, CacheTimeoutInSeconds);
            client.DefaultRequestHeaders.Clear();
        }

        public async Task<string> FetchToken()
        {
            if (!_memoryCache.TryGetValue(MemoryCacheTokenAccessor, out string token))
            {
                var tokenModel = await GenerateAccessToken();

                // keep the value within cache for given amount of time, then delete it
                var options = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(
                        TimeSpan.FromSeconds(tokenModel.ExpiresAt));

                _memoryCache.Set(MemoryCacheTokenAccessor, tokenModel.Token, options);

                token = tokenModel.Token;
            }

            return token;
        }

        private async Task<JwtResponse> GenerateAccessToken()
        {
            var claim = _configuration.IssuerKey;

            var claims = new List<Claim>
            {
                new Claim("iss", claim),
                new Claim("sub", claim),
                new Claim("jti", Guid.NewGuid().ToString())
            };

            JwtResponse token = GenerateNewTokenAsync(claims);

            var request = new HttpRequestMessage(HttpMethod.Post, _configuration.TokenEndpoint);

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            IEnumerable<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_assertion_type",
                    "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"),
                new KeyValuePair<string, string>("client_assertion", token.Token)
            };

            HttpContent content = new FormUrlEncodedContent(parameters);

            request.Content = content;

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                CancellationToken.None);
            var stream = await response.Content.ReadAsStreamAsync();

            if (response.StatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException("Could not acquire an access token");

            var accessTokenResponse = stream.ReadAndDeserializeFromJson<AccessTokenDto>();
            return new JwtResponse
            {
                Token = accessTokenResponse.AccessToken,
                ExpiresAt = accessTokenResponse.ExpiresIn
            };
        }

        private JwtResponse GenerateNewTokenAsync(IEnumerable<Claim> claims)
        {
            using RSA rsa = RSA.Create();
            rsa.ImportFromPem(_configuration.PrivateKey);

            var signingCredentials = new SigningCredentials(
                new RsaSecurityKey(rsa),
                SecurityAlgorithms.RsaSha512)
            {
                CryptoProviderFactory = new CryptoProviderFactory {CacheSignatureProviders = false}
            };

            var jwt = new JwtSecurityToken(
                _configuration.IssuerKey,
                _configuration.TokenEndpoint,
                claims,
                DateTime.UtcNow,
                DateTime.UtcNow.AddMinutes(JwtLifespanInMinutes),
                signingCredentials
            );
            jwt.Header.Add("kid", _configuration.Kid);

            var unixTimeSeconds = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();

            return new JwtResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwt),
                ExpiresAt = unixTimeSeconds,
            };
        }
    }
}
