using System;
using System.Windows.Media.Imaging;

namespace Amuse.UI.Models.FeatureExtractor
{
    public record FeatureExtractorResult(BitmapSource Image, FeatureExtractorInfoModel Info, double Elapsed) : IImageResult
    {
        public DateTime Timestamp => DateTime.Now;
        public string FilePrefix => "feature";

        public int Width => Image?.PixelWidth ?? 0;
        public int Height => Image?.PixelHeight ?? 0;
    }
}
