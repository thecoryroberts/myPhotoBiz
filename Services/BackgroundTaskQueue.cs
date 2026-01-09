using System.Threading;
using System.Threading.Channels;

namespace MyPhotoBiz.Services
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<string> _queue;

        public BackgroundTaskQueue()
        {
            // Unbounded channel for simplicity
            _queue = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
        }

        public void Enqueue(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            _queue.Writer.TryWrite(filePath);
        }

        public async ValueTask<string> DequeueAsync(CancellationToken cancellationToken)
        {
            var item = await _queue.Reader.ReadAsync(cancellationToken);
            return item;
        }
    }
}
