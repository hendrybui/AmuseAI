using Amuse.UI.Enums;
using OnnxStack.Core.Image;
using OnnxStack.Core.Model;
using OnnxStack.FeatureExtractor.Common;

namespace Amuse.UI.Models.FeatureExtractor
{
    public record FeatureExtractorModelJson
    {
        public string Name { get; set; }
        public int SampleSize { get; set; }
        public int OutputChannels { get; set; }
        public ImageNormalizeType NormalizeType { get; set; }
        public ImageNormalizeType NormalizeOutputType { get; set; }
        public bool SetOutputToInputAlpha { get; set; }
        public ImageResizeMode InputResizeMode { get; set; }
        public bool InvertOutput { get; set; }


        public int DeviceId { get; set; }
        public ExecutionProvider ExecutionProvider { get; set; }
        public string OnnxModelPath { get; set; }


        public FeatureExtractorModelConfig ToModelConfig(OnnxExecutionProvider executionProvider)
        {
            return new FeatureExtractorModelConfig
            {
                Name = Name,
                InputResizeMode = InputResizeMode,
                SampleSize = SampleSize,
                OnnxModelPath = OnnxModelPath,
                NormalizeType = NormalizeType,
                InvertOutput = InvertOutput,
                IsOptimizationSupported = true,
                NormalizeOutputType = NormalizeOutputType,
                OutputChannels = OutputChannels,
                SetOutputToInputAlpha = SetOutputToInputAlpha,
                ExecutionProvider = executionProvider
            };
        }
    }
}
