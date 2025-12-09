using Amuse.UI.Enums;
using OnnxStack.Core.Image;
using OnnxStack.Core.Model;
using OnnxStack.ImageUpscaler.Common;

namespace Amuse.UI.Models.Upscale
{
    public record UpscaleModelJson
    {
        public string Name { get; set; }
        public int Channels { get; set; }
        public int SampleSize { get; set; }
        public int ScaleFactor { get; set; }
        public ImageNormalizeType NormalizeType { get; set; }
        public TileMode TileMode { get; set; }
        public int TileSize { get; set; }
        public int TileOverlap { get; set; }

        public int DeviceId { get; set; }
        public ExecutionProvider ExecutionProvider { get; set; }
        public string OnnxModelPath { get; set; }


        public UpscaleModelConfig ToModelConfig(OnnxExecutionProvider executionProvider)
        {
            return new UpscaleModelConfig
            {
                Name = Name,
                Channels = Channels,
                IsOptimizationSupported = true,
                NormalizeType = NormalizeType,
                OnnxModelPath = OnnxModelPath,
                SampleSize = SampleSize,
                ScaleFactor = ScaleFactor,
                TileMode = TileMode,
                TileOverlap = TileOverlap,
                TileSize = TileSize,
                ExecutionProvider = executionProvider
            };
        }
    }
}
