using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using CovidLetter.Frontend.AccessTokenCache.Services;
using CovidLetter.Frontend.Pds.Models;
using CovidLetter.Frontend.Pds.Options;
using Marvin.StreamExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CovidLetter.Frontend.Pds
{
    public class PdsApiWrapperClient : IPdsApiWrapperClient
    {
        private const string XCorrelationIdLabel = "X-Correlation-ID";
        private const string XRequestIdLabel = "X-Request-ID";
        private const int TimeoutInSeconds = 30;

        private readonly ITokenCacheService _tokenCacheService;
        private readonly HttpClient _httpClient;
        private readonly string _getOperationPath;
        private readonly string _searchOperationPath;

        public PdsApiWrapperClient(
            HttpClient client,
            IOptions<ApiManagerClientOptions> options,
            ITokenCacheService tokenCacheService)
        {
            _tokenCacheService = tokenCacheService;
            _httpClient = client;

            var apiManagerClientOptions = options.Value;

            var baseUrl = apiManagerClientOptions.BaseUrl;
            var subscriptionKey = apiManagerClientOptions.SubscriptionKey;
            var keyHeaderName = apiManagerClientOptions.KeyHeaderName ?? "Subscription-Key";
            _getOperationPath = apiManagerClientOptions.GetOperation;
            _searchOperationPath = apiManagerClientOptions.SearchOperation;

            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = new TimeSpan(0, 0, TimeoutInSeconds);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add(keyHeaderName, subscriptionKey);
        }

        public async Task<GetPatientApiResult> GetPatientByNhsNumber(string nhsNumber, string correlationId)
        {
            var accessToken = await GetAccessToken();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_getOperationPath}/{nhsNumber}");

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            request.Headers.Add(XCorrelationIdLabel, correlationId);
            request.Headers.Add(XRequestIdLabel, Guid.NewGuid().ToString());

            using (var response = await
                _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None))
            {
                var stream = await response.Content.ReadAsStreamAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = stream.ReadAndDeserializeFromJson<PatientResponseDto>();
                    return GetPatientApiResult.CreateOkResult(result);
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return GetPatientApiResult.CreateNotFoundResult();
                }
                
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return GetPatientApiResult.CreateTooManyRequestsResult();
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return GetPatientApiResult.CreateBadRequestResult();
                }

                return GetPatientApiResult.CreateOtherResult();
            }
        }

        public async Task<PatientSearchApiResult> SearchPatient(
            PatientSearchParameters pdsPatientSearchParameters, string correlationId)
        {
            var accessToken = await GetAccessToken();

            string queryString = pdsPatientSearchParameters.QueryString;
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_searchOperationPath}{queryString}");

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            request.Headers.Add(XCorrelationIdLabel, correlationId);
            request.Headers.Add(XRequestIdLabel, Guid.NewGuid().ToString());

            using var response =
                await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

            var stream = await response.Content.ReadAsStreamAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = stream.ReadAndDeserializeFromJson<PdsPatientSearchResponseDto>();

                if (result.PatientDetails != null)
                {
                    return PatientSearchApiResult.CreateOkResult(result);
                }

                return PatientSearchApiResult.CreateNotFoundResult();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return PatientSearchApiResult.CreateNotFoundResult();
            }
            
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                return PatientSearchApiResult.CreateTooManyRequestsResult();
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return PatientSearchApiResult.CreateNotFoundResult();
            }

            throw new InvalidOperationException($"Received an un-catered for response from PDS. HTTP: {response.StatusCode}");
        }

        private async Task<string> GetAccessToken()
        {
            try
            {
                var accessToken = await _tokenCacheService.FetchTokenAsync();

                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    return accessToken;
                }

                throw new PdsGenerateAccessTokenException("Could not create an access token");
            }
            catch (Exception e)
            {
                throw new PdsGenerateAccessTokenException("Could not create an access token", e);
            }
        }
    }
}