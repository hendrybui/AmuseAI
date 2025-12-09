using OnnxStack.Core.Video;
using System;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace Amuse.UI.Models.FeatureExtractor
{
    public class FeatureExtractorVideoResultModel : IVideoResult
    {
        public string FilePrefix { get; set; } = "feature";
        public DateTime Timestamp { get; } = DateTime.UtcNow;

        public double Elapsed { get; set; }

        [JsonIgnore]
        public string FileName { get; set; }

        [JsonIgnore]
        public OnnxVideo Video { get; set; }

        [JsonIgnore]
        public BitmapSource Thumbnail { get; set; }

        public FeatureExtractorInfoModel FeatureExtractorInfo { get; set; }
    }
}