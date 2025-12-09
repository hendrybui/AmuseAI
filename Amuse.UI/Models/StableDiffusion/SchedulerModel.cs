using OnnxStack.StableDiffusion.Enums;

namespace Amuse.UI.Models.StableDiffusion
{
    public record SchedulerModel(SchedulerType Scheduler, bool IsKarras)
    {
        public override string ToString()
        {
            if (IsKarras)
                return $"{Scheduler.GetDescription()} (Karras)";

            return Scheduler.GetDescription();
        }
    }
}
