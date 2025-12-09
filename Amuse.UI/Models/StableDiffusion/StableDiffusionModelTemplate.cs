using OnnxStack.StableDiffusion.Enums;
using System.Text.Json.Serialization;

namespace Amuse.UI.Models.StableDiffusion
{
    public record StableDiffusionModelTemplate
    {
        public PipelineType PipelineType { get; set; }
        public ModelType ModelType { get; set; }
        public int SampleSize { get; set; }
        public int TokenizerLength { get; set; } = 768;
        public int Tokenizer2Limit { get; set; } = 77;
        public OptimizationType Optimization { get; set; } = OptimizationType.Level1;
        public DiffuserType[] DiffuserTypes { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int ContextSize { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SchedulerType[] Schedulers { get; set; }

        public StableDiffusionSchedulerDefaults SchedulerDefaults { get; set; } = new StableDiffusionSchedulerDefaults();
    }
}
