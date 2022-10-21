using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Messaging.ServiceBus;
using CovidLetter.Frontend.Logging;
using CovidLetter.Frontend.Queue.Constants;
using CovidLetter.Frontend.Queue.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace CovidLetter.Frontend.Queue;

public sealed class ServiceBusService : IQueueService
{
    private readonly ILogger<ServiceBusService> _logger;
    private readonly QueueConfig _options;
    private readonly AsyncRetryPolicy _policy;

    public ServiceBusService(
        ILogger<ServiceBusService> logger,
        IOptions<QueueConfig> options)
    {
        _logger = logger;
        _options = options.Value;

        _policy = Policy
            .Handle<TimeoutException>()
            .WaitAndRetryAsync(
                5,
                sleepDuration => TimeSpan.FromSeconds(Math.Pow(2, sleepDuration)),
                LogRetry);
    }

    public async Task Send(string json, bool pdf, string correlationId, CancellationToken cancellationToken)
    {
        await Execute(Add, cancellationToken);
        async Task Add(CancellationToken retryCancellationToken)
        {
            await SendMessageAndLog(json, pdf, correlationId, retryCancellationToken);
        }
    }

    private void LogRetry(Exception exception, TimeSpan sleepDuration)
        => _logger.LogInformation(
            AppEventId.QueueRetry,
            exception,
            "Retrying after sleeping {SleepDuration} of {RetryAction}",
            sleepDuration,
            "Add Request");

    private async Task Execute(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
        => await _policy.ExecuteAsync(action, cancellationToken);

    private async Task SendMessageAndLog(string content, bool pdf, string correlationId, CancellationToken cancellationToken)
    {
        var textBytes = Encoding.UTF8.GetBytes(content);
        var versionString = "1";
        var message = new ServiceBusMessage(content)
        {
            ContentType = ContentType.ApplicationJson.ToString(),
            MessageId = Guid.NewGuid().ToString(),
            CorrelationId = correlationId,
            ApplicationProperties =
            {
                [QueueMetadataKeys.Version] = versionString,
                [QueueMetadataKeys.Sha256Checksum] = Checksum.Sha256(textBytes)
            },
        };

        await using var client = new ServiceBusClient(pdf ? _options.CSBConnectionString : _options.ConnectionString);
        await using var sender = client.CreateSender(pdf ? _options.PdfQueueName : _options.QueueName);
        await sender.SendMessageAsync(message, cancellationToken);

        _logger.LogInformation(
            "Message {MessageId} for {CorrelationId} successfully saved to the queue", message.MessageId,
                correlationId);
    }
}