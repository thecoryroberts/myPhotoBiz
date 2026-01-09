using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace MyPhotoBiz.Services
{
    public class ImageProcessingHostedService : BackgroundService
    {
        private readonly IBackgroundTaskQueue _queue;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ImageProcessingHostedService> _logger;

        public ImageProcessingHostedService(IBackgroundTaskQueue queue, IWebHostEnvironment env, ILogger<ImageProcessingHostedService> logger)
        {
            _queue = queue;
            _env = env;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ImageProcessingHostedService started.");
            while (!stoppingToken.IsCancellationRequested)
            {
                string filePath;
                try
                {
                    filePath = await _queue.DequeueAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                try
                {
                    await ProcessImageAsync(filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed processing image {FilePath}", filePath);
                }
            }

            _logger.LogInformation("ImageProcessingHostedService stopping.");
        }

        private async Task ProcessImageAsync(string relativeOrAbsolutePath)
        {
            if (string.IsNullOrEmpty(relativeOrAbsolutePath)) return;

            var path = relativeOrAbsolutePath;
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(_env.WebRootPath ?? "wwwroot", relativeOrAbsolutePath.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar));
            }

            if (!File.Exists(path))
            {
                _logger.LogWarning("Image file not found: {Path}", path);
                return;
            }

            var dir = Path.GetDirectoryName(path) ?? _env.WebRootPath ?? "wwwroot";
            var baseName = Path.GetFileNameWithoutExtension(path);
            var thumbName = baseName + "_thumb.jpg";
            var thumbPath = Path.Combine(dir, thumbName);

            try
            {
                using var image = await Image.LoadAsync(path);
                image.Mutate(x => x.Resize(new ResizeOptions { Size = new SixLabors.ImageSharp.Size(400, 300), Mode = ResizeMode.Crop }));
                var encoder = new JpegEncoder { Quality = 80 };
                await image.SaveAsJpegAsync(thumbPath, encoder);
                _logger.LogInformation("Generated thumbnail {ThumbPath}", thumbPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating thumbnail for {Path}", path);
            }
        }
    }
}
