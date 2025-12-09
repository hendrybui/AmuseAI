using System;
using System.Windows.Media.Imaging;

namespace Amuse.UI.Models.Upscale
{
    public record UpscaleResult(BitmapSource Image, UpscaleInfoModel Info, double Elapsed) : IImageResult
    {
        public DateTime Timestamp => DateTime.Now;

        public string FilePrefix => $"{Info.OutputWidth}x{Info.OutputHeight}";

        public int Width => Image?.PixelWidth ?? 0;
        public int Height => Image?.PixelHeight ?? 0;
    }
}
