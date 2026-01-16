namespace MyPhotoBiz.Services
{
    /// <summary>
    /// Service for applying watermarks to images
    /// </summary>
    public interface IWatermarkService
    {
        /// <summary>
        /// Apply watermark to an image and return the watermarked image bytes
        /// </summary>
        /// <param name="imageBytes">Original image bytes</param>
        /// <param name="settings">Watermark settings</param>
        /// <returns>Watermarked image bytes</returns>
        Task<byte[]> ApplyWatermarkAsync(byte[] imageBytes, WatermarkSettings settings);

        /// <summary>
        /// Apply watermark to an image file and return the watermarked image bytes
        /// </summary>
        /// <param name="imagePath">Path to the original image</param>
        /// <param name="settings">Watermark settings</param>
        /// <returns>Watermarked image bytes</returns>
        Task<byte[]> ApplyWatermarkFromFileAsync(string imagePath, WatermarkSettings settings);
    }

    /// <summary>
    /// Settings for watermark application
    /// </summary>
    public class WatermarkSettings
    {
        /// <summary>
        /// Text to display as watermark (used if no image watermark)
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// Path to watermark image (logo)
        /// </summary>
        public string? ImagePath { get; set; }

        /// <summary>
        /// Opacity of the watermark (0.0 to 1.0)
        /// </summary>
        public float Opacity { get; set; } = 0.5f;

        /// <summary>
        /// Position of the watermark
        /// </summary>
        public WatermarkPosition Position { get; set; } = WatermarkPosition.Center;

        /// <summary>
        /// Font size for text watermark (relative to image size)
        /// </summary>
        public float FontSizePercent { get; set; } = 5f;

        /// <summary>
        /// Enable tiled/repeating watermark pattern
        /// </summary>
        public bool Tiled { get; set; } = false;

        /// <summary>
        /// Rotation angle in degrees for tiled watermarks
        /// </summary>
        public float TileRotation { get; set; } = -30f;

        /// <summary>
        /// Output quality (1-100)
        /// </summary>
        public int OutputQuality { get; set; } = 90;
    }

    /// <summary>
    /// Watermark position options
    /// </summary>
    public enum WatermarkPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        Center,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }
}
