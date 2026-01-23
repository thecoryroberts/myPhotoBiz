using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;

namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Provides watermark operations.
    /// </summary>
    public class WatermarkService : IWatermarkService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<WatermarkService> _logger;
        private static FontFamily? _fontFamily;
        private static readonly object _fontLock = new();

        public WatermarkService(IWebHostEnvironment env, ILogger<WatermarkService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<byte[]> ApplyWatermarkAsync(byte[] imageBytes, WatermarkSettings settings)
        {
            try
            {
                using var inputStream = new MemoryStream(imageBytes);
                using var image = await Image.LoadAsync<Rgba32>(inputStream);

                ApplyWatermarkToImage(image, settings);

                using var outputStream = new MemoryStream();
                var encoder = new JpegEncoder { Quality = settings.OutputQuality };
                await image.SaveAsJpegAsync(outputStream, encoder);
                return outputStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying watermark to image");
                throw;
            }
        }

        public async Task<byte[]> ApplyWatermarkFromFileAsync(string imagePath, WatermarkSettings settings)
        {
            try
            {
                using var image = await Image.LoadAsync<Rgba32>(imagePath);

                ApplyWatermarkToImage(image, settings);

                using var outputStream = new MemoryStream();
                var encoder = new JpegEncoder { Quality = settings.OutputQuality };
                await image.SaveAsJpegAsync(outputStream, encoder);
                return outputStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying watermark from file: {FilePath}", imagePath);
                throw;
            }
        }

        private void ApplyWatermarkToImage(Image<Rgba32> image, WatermarkSettings settings)
        {
            if (!string.IsNullOrEmpty(settings.ImagePath))
            {
                ApplyImageWatermark(image, settings);
            }
            else if (!string.IsNullOrEmpty(settings.Text))
            {
                if (settings.Tiled)
                {
                    ApplyTiledTextWatermark(image, settings);
                }
                else
                {
                    ApplyTextWatermark(image, settings);
                }
            }
        }

        private void ApplyImageWatermark(Image<Rgba32> image, WatermarkSettings settings)
        {
            var watermarkPath = GetAbsolutePath(settings.ImagePath!);
            if (!File.Exists(watermarkPath))
            {
                _logger.LogWarning("Watermark image not found: {Path}", watermarkPath);
                return;
            }

            using var watermark = Image.Load<Rgba32>(watermarkPath);

            // Scale watermark to be proportional to the main image (max 20% of width)
            var maxWatermarkWidth = (int)(image.Width * 0.2);
            var maxWatermarkHeight = (int)(image.Height * 0.2);

            if (watermark.Width > maxWatermarkWidth || watermark.Height > maxWatermarkHeight)
            {
                var scale = Math.Min(
                    (float)maxWatermarkWidth / watermark.Width,
                    (float)maxWatermarkHeight / watermark.Height
                );
                watermark.Mutate(x => x.Resize((int)(watermark.Width * scale), (int)(watermark.Height * scale)));
            }

            // Apply opacity
            watermark.Mutate(x => x.Opacity(settings.Opacity));

            // Calculate position
            var position = CalculatePosition(image.Size, watermark.Size, settings.Position);

            // Draw watermark
            image.Mutate(x => x.DrawImage(watermark, position, 1f));
        }

        private void ApplyTextWatermark(Image<Rgba32> image, WatermarkSettings settings)
        {
            var font = GetFont(image.Width, settings.FontSizePercent);
            if (font == null)
            {
                _logger.LogWarning("Could not load font for watermark");
                return;
            }

            var text = settings.Text ?? "PROOF";

            // Measure text
            var textOptions = new RichTextOptions(font)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var textBounds = TextMeasurer.MeasureBounds(text, textOptions);
            var textSize = new Size((int)textBounds.Width + 20, (int)textBounds.Height + 10);

            // Calculate position
            var position = CalculatePosition(image.Size, textSize, settings.Position);

            // Create semi-transparent white text with dark outline for visibility
            var textColor = Color.White.WithAlpha(settings.Opacity);
            var outlineColor = Color.Black.WithAlpha(settings.Opacity * 0.5f);

            var drawOptions = new RichTextOptions(font)
            {
                Origin = new PointF(position.X + textSize.Width / 2, position.Y + textSize.Height / 2),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Draw outline (offset in 4 directions)
            var outlineOffset = Math.Max(1, font.Size / 20);
            image.Mutate(x =>
            {
                x.DrawText(new RichTextOptions(font) { Origin = new PointF(drawOptions.Origin.X - outlineOffset, drawOptions.Origin.Y), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }, text, outlineColor);
                x.DrawText(new RichTextOptions(font) { Origin = new PointF(drawOptions.Origin.X + outlineOffset, drawOptions.Origin.Y), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }, text, outlineColor);
                x.DrawText(new RichTextOptions(font) { Origin = new PointF(drawOptions.Origin.X, drawOptions.Origin.Y - outlineOffset), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }, text, outlineColor);
                x.DrawText(new RichTextOptions(font) { Origin = new PointF(drawOptions.Origin.X, drawOptions.Origin.Y + outlineOffset), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }, text, outlineColor);
                x.DrawText(drawOptions, text, textColor);
            });
        }

        private void ApplyTiledTextWatermark(Image<Rgba32> image, WatermarkSettings settings)
        {
            var font = GetFont(image.Width, settings.FontSizePercent);
            if (font == null)
            {
                _logger.LogWarning("Could not load font for tiled watermark");
                return;
            }

            var text = settings.Text ?? "PROOF";

            // Measure text
            var textOptions = new RichTextOptions(font)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var textBounds = TextMeasurer.MeasureBounds(text, textOptions);
            var textWidth = (int)textBounds.Width + 40;
            var textHeight = (int)textBounds.Height + 20;

            // Calculate spacing between tiles
            var spacingX = textWidth + (int)(image.Width * 0.1);
            var spacingY = textHeight + (int)(image.Height * 0.15);

            // Create watermark color
            var textColor = Color.White.WithAlpha(settings.Opacity);
            var outlineColor = Color.Black.WithAlpha(settings.Opacity * 0.3f);

            // Apply rotation transformation for tiled effect
            var rotationRadians = settings.TileRotation * (float)Math.PI / 180f;

            image.Mutate(ctx =>
            {
                // Calculate expanded bounds for rotated tiles
                var diagonal = (int)Math.Sqrt(image.Width * image.Width + image.Height * image.Height);
                var startX = -diagonal / 2;
                var startY = -diagonal / 2;
                var endX = image.Width + diagonal / 2;
                var endY = image.Height + diagonal / 2;

                // Draw tiles
                for (var y = startY; y < endY; y += spacingY)
                {
                    for (var x = startX; x < endX; x += spacingX)
                    {
                        // Calculate rotated position
                        var centerX = image.Width / 2f;
                        var centerY = image.Height / 2f;
                        var dx = x - centerX;
                        var dy = y - centerY;
                        var rotatedX = centerX + dx * (float)Math.Cos(rotationRadians) - dy * (float)Math.Sin(rotationRadians);
                        var rotatedY = centerY + dx * (float)Math.Sin(rotationRadians) + dy * (float)Math.Cos(rotationRadians);

                        // Skip if outside image bounds (with margin)
                        if (rotatedX < -textWidth || rotatedX > image.Width + textWidth ||
                            rotatedY < -textHeight || rotatedY > image.Height + textHeight)
                            continue;

                        var drawOptions = new RichTextOptions(font)
                        {
                            Origin = new PointF(rotatedX, rotatedY),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };

                        // Draw outline
                        var outlineOffset = Math.Max(1, font.Size / 30);
                        ctx.DrawText(new RichTextOptions(font) { Origin = new PointF(rotatedX - outlineOffset, rotatedY), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }, text, outlineColor);
                        ctx.DrawText(new RichTextOptions(font) { Origin = new PointF(rotatedX + outlineOffset, rotatedY), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }, text, outlineColor);
                        ctx.DrawText(drawOptions, text, textColor);
                    }
                }
            });
        }

        private Point CalculatePosition(Size imageSize, Size watermarkSize, WatermarkPosition position)
        {
            var margin = (int)(Math.Min(imageSize.Width, imageSize.Height) * 0.02); // 2% margin

            return position switch
            {
                WatermarkPosition.TopLeft => new Point(margin, margin),
                WatermarkPosition.TopCenter => new Point((imageSize.Width - watermarkSize.Width) / 2, margin),
                WatermarkPosition.TopRight => new Point(imageSize.Width - watermarkSize.Width - margin, margin),
                WatermarkPosition.MiddleLeft => new Point(margin, (imageSize.Height - watermarkSize.Height) / 2),
                WatermarkPosition.Center => new Point((imageSize.Width - watermarkSize.Width) / 2, (imageSize.Height - watermarkSize.Height) / 2),
                WatermarkPosition.MiddleRight => new Point(imageSize.Width - watermarkSize.Width - margin, (imageSize.Height - watermarkSize.Height) / 2),
                WatermarkPosition.BottomLeft => new Point(margin, imageSize.Height - watermarkSize.Height - margin),
                WatermarkPosition.BottomCenter => new Point((imageSize.Width - watermarkSize.Width) / 2, imageSize.Height - watermarkSize.Height - margin),
                WatermarkPosition.BottomRight => new Point(imageSize.Width - watermarkSize.Width - margin, imageSize.Height - watermarkSize.Height - margin),
                _ => new Point((imageSize.Width - watermarkSize.Width) / 2, (imageSize.Height - watermarkSize.Height) / 2)
            };
        }

        private Font? GetFont(int imageWidth, float fontSizePercent)
        {
            var fontSize = Math.Max(12, imageWidth * fontSizePercent / 100f);

            lock (_fontLock)
            {
                if (_fontFamily == null)
                {
                    // Try to load system fonts
                    var fontCollection = new FontCollection();

                    // Try common font paths
                    var fontPaths = new[]
                    {
                        "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",
                        "/usr/share/fonts/truetype/liberation/LiberationSans-Bold.ttf",
                        "/usr/share/fonts/TTF/DejaVuSans-Bold.ttf",
                        "C:\\Windows\\Fonts\\arial.ttf",
                        "C:\\Windows\\Fonts\\arialbd.ttf",
                        Path.Combine(_env.WebRootPath ?? "wwwroot", "fonts", "Roboto-Bold.ttf")
                    };

                    foreach (var path in fontPaths)
                    {
                        if (File.Exists(path))
                        {
                            try
                            {
                                _fontFamily = fontCollection.Add(path);
                                _logger.LogInformation("Loaded watermark font from: {Path}", path);
                                break;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to load font from: {Path}", path);
                            }
                        }
                    }

                    // Fallback to system fonts
                    if (_fontFamily == null)
                    {
                        try
                        {
                            if (SystemFonts.TryGet("Arial", out var arial))
                            {
                                _fontFamily = arial;
                            }
                            else if (SystemFonts.TryGet("DejaVu Sans", out var dejavu))
                            {
                                _fontFamily = dejavu;
                            }
                            else if (SystemFonts.TryGet("Liberation Sans", out var liberation))
                            {
                                _fontFamily = liberation;
                            }
                            else
                            {
                                // Get any available font
                                var families = SystemFonts.Families.ToList();
                                if (families.Count > 0)
                                {
                                    _fontFamily = families[0];
                                    _logger.LogInformation("Using fallback font: {Font}", _fontFamily.Value.Name);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to get system fonts");
                        }
                    }
                }
            }

            if (_fontFamily == null)
            {
                _logger.LogError("No fonts available for watermarking");
                return null;
            }

            return _fontFamily.Value.CreateFont(fontSize, FontStyle.Bold);
        }

        private string GetAbsolutePath(string relativePath)
        {
            if (Path.IsPathRooted(relativePath))
                return relativePath;

            // Remove leading slash/tilde
            var cleanPath = relativePath.TrimStart('/', '~', '\\');
            return Path.Combine(_env.WebRootPath ?? "wwwroot", cleanPath);
        }
    }
}
