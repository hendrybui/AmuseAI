using System.Collections.Generic;
using Amuse.UI.Models;

namespace Amuse.UI.Services
{
    public interface IDeviceService
    {
        Device BaseDevice { get; }
        IReadOnlyList<Device> Devices { get; }
        bool IsRyzenAI { get; }
        bool IsNPUSuperResolutionSupported { get; }
        bool IsNPUStableDiffusionSupported { get; }
        HardwareProfile GetHardwareProfile();
    }
}