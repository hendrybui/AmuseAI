using OnnxStack.StableDiffusion.Enums;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Amuse.UI.Models.StableDiffusion
{
    public record StableDiffusionSchedulerDefaults(
        SchedulerType SchedulerType = SchedulerType.EulerAncestral,
        int Steps = 30, int StepsMin = 4, int StepsMax = 100,
        float Guidance = 7.5f, float GuidanceMin = 0f, float GuidanceMax = 30f,
        float Guidance2 = 0, float Guidance2Min = 0f, float Guidance2Max = 0f,
        TimestepSpacingType TimestepSpacing = TimestepSpacingType.Linspace,
        PredictionType PredictionType = PredictionType.Epsilon,
        BetaScheduleType BetaSchedule = BetaScheduleType.ScaledLinear, float BetaStart = 0.00085f, float BetaEnd = 0.012f)
    {

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<int> Timesteps { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool UseKarrasSigmas { get; set; }
    }
}
