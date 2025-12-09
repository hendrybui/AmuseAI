using Amuse.UI.Enums;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Amuse.UI.Models
{
    public record EZModeProfile
    {
        public EZModeSettings Generate { get; set; }
        public EZModeSettings Modify { get; set; }
        public EZModeSettings Create { get; set; }
    }


    public record EZModeSettings
    {
        public string DemoPrompt { get; set; }
        public string ImagePrompt { get; set; }
        public string ImageNegativePrompt { get; set; }
        public string VideoPrompt { get; set; }
        public string VideoNegativePrompt { get; set; }
    }


    public record HardwareProfile
    {
        public string Name { get; set; }
        public int MinMemory { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string> Devices { get; set; }
        public HardwareProfileGenerate Generate { get; set; }
        public HardwareProfileGroup Modify { get; set; }
        public HardwareProfileGroup Create { get; set; }
    }


    public enum HardwareProfileQualityType : int
    {
        Fast = 0,
        Balanced = 1,
        Quality = 2
    }


    public record HardwareProfileGroup
    {
        public HardwareProfileOption ImageProfile { get; set; }
        public HardwareProfileOption VideoProfile { get; set; }
    }


    public record HardwareProfileGenerate
    {
        public HardwareProfileQualityType DefaultQuality { get; set; }
        public HardwareProfileGroup Fast { get; set; }
        public HardwareProfileGroup Balanced { get; set; }
        public HardwareProfileGroup Quality { get; set; }
    }


    public record HardwareProfileOption
    {
        public List<int> Steps { get; set; }
        public HardwareProfileAspectType Aspect { get; set; }
        public HardwareProfileQualityType Quality { get; set; }
        public Guid ModelId { get; set; }
        public List<ControlNetProfile> ControlNet { get; set; }
        public HardwareProfileResolution Default { get; set; }
        public HardwareProfileResolution Portrait { get; set; }
        public HardwareProfileResolution Landscape { get; set; }
    }


    public record HardwareProfileResolution(int Width, int Height)
    {
        [JsonIgnore]
        public bool IsEnabled => Width > 0 && Height > 0;
    }


    public record ControlNetProfile(string Name, string Description, Guid ModelId, Guid ExtractorId)
    {
        public override string ToString()
        {
            return Name;
        }
    }

}
