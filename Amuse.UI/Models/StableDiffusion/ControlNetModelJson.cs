using Amuse.UI.Enums;
using OnnxStack.Core.Model;
using OnnxStack.StableDiffusion.Models;

namespace Amuse.UI.Models.StableDiffusion
{
    public record ControlNetModelJson
    {
        public string Name { get; set; }
        public bool InvertInput { get; set; }
        public int LayerCount { get; set; }
        public bool DisablePooledProjection { get; set; }

        public int DeviceId { get; set; }
        public ExecutionProvider ExecutionProvider { get; set; }
        public string OnnxModelPath { get; set; }

        public ControlNetModelConfig ToModelConfig(OnnxExecutionProvider executionProvider)
        {
            return new ControlNetModelConfig
            {
                Name = Name,
                InvertInput = InvertInput,
                LayerCount = LayerCount,
                DisablePooledProjection = DisablePooledProjection,
                IsOptimizationSupported = true,
                OnnxModelPath = OnnxModelPath,
                ExecutionProvider = executionProvider
            };
        }
    }
}
