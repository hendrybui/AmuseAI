using OnnxStack.Core.Video;
using OnnxStack.StableDiffusion.Config;
using OnnxStack.StableDiffusion.Enums;
using System;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace Amuse.UI.Models
{
    public class VideoResultModel : IVideoResult
    {
        public string ModelName { get; init; }
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public PipelineType PipelineType { get; init; }
        public DiffuserType DiffuserType { get; init; }
        public GenerateOptions PromptOptions { get; init; }
        public SchedulerOptions SchedulerOptions { get; init; }
        public double Elapsed { get; init; }

        [JsonIgnore]
        public string FileName { get; set; }

        [JsonIgnore]
        public OnnxVideo Video { get; set; }

        [JsonIgnore]
        public BitmapSource Thumbnail { get; set; }


        public string FilePrefix => SchedulerOptions?.Seed.ToString();
    }
}