using System.Threading;

namespace MyPhotoBiz.Services
{
    public interface IBackgroundTaskQueue
    {
        void Enqueue(string filePath);
        ValueTask<string> DequeueAsync(CancellationToken cancellationToken);
    }
}
