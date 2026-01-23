using System.Threading;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Defines the background task queue contract.
    /// </summary>
    public interface IBackgroundTaskQueue
    {
        void Enqueue(string filePath);
        ValueTask<string> DequeueAsync(CancellationToken cancellationToken);
    }
}
