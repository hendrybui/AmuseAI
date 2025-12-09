using OnnxStack.Core.Video;
using System;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace Amuse.UI.Models.Upscale
{
    public class UpscaleVideoResultModel : IVideoResult
    {
        public string FilePrefix { get; set; }
        public DateTime Timestamp { get; } = DateTime.UtcNow;

        public double Elapsed { get; set; }

        [JsonIgnore]
        public string FileName { get; set; }

        [JsonIgnore]
        public OnnxVideo Video { get; set; }

        [JsonIgnore]
        public BitmapSource Thumbnail { get; set; }

        public UpscaleInfoModel UpscaleInfo { get; set; }
    }
}