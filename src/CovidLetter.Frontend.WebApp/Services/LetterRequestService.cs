using System.Threading;
using System.Threading.Tasks;
using CovidLetter.Frontend.Queue;
using CovidLetter.Frontend.WebApp.Services.Queue;
using Newtonsoft.Json;

namespace CovidLetter.Frontend.WebApp.Services;

public class LetterRequestService
{
    private readonly IQueueService _queueService;

    public LetterRequestService(IQueueService queueService)
    {
        _queueService = queueService;
    }

    public async Task SendLetterRequest(LetterRequestMessage letterRequest)
    {
        UpperCase(letterRequest);

        await _queueService.Send(
            JsonConvert.SerializeObject(letterRequest),
            pdf: false,
            letterRequest.CorrelationId,
            new CancellationToken());
    }

    public async Task SendPDFRequest(UnattendedPdfRequest pdfRequest)
    {
        await _queueService.Send(
            JsonConvert.SerializeObject(pdfRequest), 
            pdf: true,
            pdfRequest.CorrelationId,
            new CancellationToken());
    }

    private void UpperCase(LetterRequestMessage letterRequest)
    {
        letterRequest.Title = letterRequest.Title?.ToUpper();
        letterRequest.FirstName = letterRequest.FirstName?.ToUpper();
        letterRequest.LastName = letterRequest.LastName?.ToUpper();
        letterRequest.AddressLine1 = letterRequest.AddressLine1?.ToUpper();
        letterRequest.AddressLine2 = letterRequest.AddressLine2?.ToUpper();
        letterRequest.AddressLine3 = letterRequest.AddressLine3?.ToUpper();
        letterRequest.AddressLine4 = letterRequest.AddressLine4?.ToUpper();
        letterRequest.Postcode = letterRequest.Postcode?.ToUpper();
    }
}