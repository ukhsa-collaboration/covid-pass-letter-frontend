using System.Threading;
using System.Threading.Tasks;

namespace CovidLetter.Frontend.Queue
{
    public interface IQueueService
    {
        Task Send(string json, bool pdf, string correlationId, CancellationToken cancellationToken);
    }
}