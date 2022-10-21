using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using CovidLetter.Frontend.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace CovidLetter.Frontend.Queue
{
    public class QueueService : IQueueService
    {
        private readonly ILogger<QueueService> _logger;
        private readonly QueueClient _queueClient;
        private readonly AsyncRetryPolicy _policy;

        public QueueService(
            ILogger<QueueService> logger,
            IOptions<QueueConfig> options)
        {
            _logger = logger;

            var queueConfig = options.Value;
            _queueClient = new QueueClient(
                queueConfig.ConnectionString,
                queueConfig.QueueName,
                new QueueClientOptions
                {
                    MessageEncoding = QueueMessageEncoding.Base64
                });

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
                await SendMessageAndLog(json, retryCancellationToken);
            }
        }

        private void LogRetry(Exception exception, TimeSpan sleepDuration)
            => _logger.LogInformation(
                AppEventId.QueueRetry,
                exception,
                "Retrying after sleeping {sleepDuration} of {RetryAction}",
                sleepDuration,
                "Add Request");

        private async Task Execute(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
            => await _policy.ExecuteAsync(action, cancellationToken);

        private async Task SendMessageAndLog(string content, CancellationToken cancellationToken)
        {
            await _queueClient.SendMessageAsync(content, cancellationToken);

            _logger.LogInformation(
                "Message successfully saved to the queue");
        }
    }
}