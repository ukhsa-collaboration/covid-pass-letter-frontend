using System;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CovidLetter.Frontend.Extensions;
using CovidLetter.Frontend.Search;
using CovidLetter.Frontend.WebApp.Configuration;
using CovidLetter.Frontend.WebApp.Models;
using Microsoft.Extensions.Options;

[assembly: InternalsVisibleTo("CovidLetter.Frontend.Search.Tests")]

namespace CovidLetter.Frontend.WebApp.Services
{
    public class OtpService : IOtpService
    {
        private readonly HttpClient _httpClient;
        private readonly string _requestOperation;
        private readonly string _verifyOperation;

        private const int TimeoutInSeconds = 30;

        public OtpService(HttpClient httpClient, IOptions<OtpServiceOptions> options)
        {
            _httpClient = httpClient;

            var otpServiceOptions = options.Value;

            var subscriptionKey = otpServiceOptions.SubscriptionKey;
            var keyHeaderName = otpServiceOptions.KeyHeaderName.IsNullOrEmpty() ? "Subscription-Key" : otpServiceOptions.KeyHeaderName;
            _requestOperation = otpServiceOptions.RequestOperation;
            _verifyOperation = otpServiceOptions.VerifyOperation;

            httpClient.BaseAddress = new Uri(otpServiceOptions.BaseUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(TimeoutInSeconds);    
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add(keyHeaderName, subscriptionKey);
        }

        public async Task<VerifyOtpResult> VerifyOtpMatch(
            VerifyOtpMatchParams verifyOtpMatchParams, 
            string correlationId)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_verifyOperation}");

            request.Headers.Add("phoneNumber", SearchPatientService.IsValidUkMobilePhoneNumber(verifyOtpMatchParams.PhoneNumber) ?
                SearchPatientService.ConvertUkNumberToInternationalFormat(verifyOtpMatchParams.PhoneNumber) : 
                verifyOtpMatchParams.PhoneNumber);

            request.Headers.Add("otpCode", verifyOtpMatchParams.OtpCode);

            using var response =
                await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

            var responseMessage = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                return new VerifyOtpResult(true, 0, false);
            }
            
            if (response.StatusCode == HttpStatusCode.BadRequest && Int32.TryParse(responseMessage, out var remainingAttempts))
            {
                return new VerifyOtpResult(false, Math.Max(0, remainingAttempts), false);
            }

            if (response.StatusCode == HttpStatusCode.Gone)
            {
                return new VerifyOtpResult(false, 0, true);
            }

            return new VerifyOtpResult(false, 0, false);
        }

        public async Task<RequestOtpResult> RequestOtp(
            string phoneNumber,
            string correlationId)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_requestOperation}");

            request.Headers.Add("phoneNumber", SearchPatientService.IsValidUkMobilePhoneNumber(phoneNumber) ?
                SearchPatientService.ConvertUkNumberToInternationalFormat(phoneNumber) :
                phoneNumber);

            using var response =
                await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

            if (response.IsSuccessStatusCode)
            {
                return new RequestOtpResult.Success(phoneNumber);
            }

            return response.StatusCode == HttpStatusCode.TooManyRequests ? new RequestOtpResult.TooManyRequests(phoneNumber) : new RequestOtpResult.Failed(phoneNumber);
        }
    }
}
