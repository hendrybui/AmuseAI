using Amuse.UI.Enums;
using Amuse.UI.Models;
using Microsoft.ML.OnnxRuntime;
using OnnxStack.Core.Model;
using OnnxStack.StableDiffusion.Enums;
using System;
using System.IO;
using System.Linq;

namespace Amuse.UI.Services
{
    public class ProviderService : IProviderService
    {
        private readonly AmuseSettings _settings;

        public ProviderService(AmuseSettings settings)
        {
            _settings = settings;
        }


        public OnnxExecutionProvider GetProvider(ExecutionProvider? provider, int? deviceId)
        {
            var selectedDevice = deviceId ?? _settings.DefaultExecutionDevice.DeviceId;
            var selectedProvider = provider ?? _settings.DefaultExecutionDevice.Provider;
            var cacheDirectory = Path.Combine(App.CacheDirectory, selectedProvider.ToString());
            return selectedProvider switch
            {
                ExecutionProvider.DirectML => DirectML(selectedDevice),
                ExecutionProvider.AMDGPU => AMDGPU(selectedDevice, cacheDirectory),
                _ => CPU()
            };
        }


        public OnnxExecutionProvider GetRyzenAI(int? deviceId, PipelineType pipelineType)
        {
            return RyzenAI(deviceId ?? _settings.DefaultExecutionDevice.DeviceId, pipelineType);
        }


        private static OnnxExecutionProvider CPU()
        {
            return new OnnxExecutionProvider(nameof(ExecutionProvider.CPU), configuration =>
            {
                var sessionOptions = new SessionOptions
                {
                    ExecutionMode = ExecutionMode.ORT_SEQUENTIAL,
                    GraphOptimizationLevel = GraphOptimizationLevel.ORT_DISABLE_ALL
                };

                sessionOptions.AppendExecutionProvider_CPU();
                return sessionOptions;
            });
        }


        private static OnnxExecutionProvider DirectML(int deviceId)
        {
            return new OnnxExecutionProvider(nameof(ExecutionProvider.DirectML), configuration =>
            {
                var sessionOptions = new SessionOptions
                {
                    ExecutionMode = ExecutionMode.ORT_SEQUENTIAL,
                    GraphOptimizationLevel = GraphOptimizationLevel.ORT_DISABLE_ALL
                };

                sessionOptions.AppendExecutionProvider_DML(deviceId);
                sessionOptions.AppendExecutionProvider_CPU();
                return sessionOptions;
            });
        }


        private static OnnxExecutionProvider RyzenAI(int deviceId, PipelineType pipelineType)
        {
            var vaeKey = pipelineType == PipelineType.StableDiffusion3 ? "SD30_DECODER" : "SD15_DECODER";
            var transformerKey = pipelineType == PipelineType.StableDiffusion3 ? "SD30_MMDIT" : "SD15_UNET";

            return new OnnxExecutionProvider("RyzenAI", configuration =>
            {
                var sessionOptions = new SessionOptions
                {
                    ExecutionMode = ExecutionMode.ORT_SEQUENTIAL,
                    GraphOptimizationLevel = GraphOptimizationLevel.ORT_DISABLE_ALL
                };

                var modelPath = Path.GetDirectoryName(configuration.OnnxModelPath);
                var modelFolderPath = modelPath
                    .Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.TrimEntries)
                    .TakeLast(2)
                    .ToArray();

                var modelName = modelFolderPath switch
                {
                    var a when a.Contains("unet") => transformerKey,
                    var a when a.Contains("controlnet") => transformerKey,
                    var a when a.Contains("transformer") => transformerKey,
                    var a when a.Contains("vae_encoder") => vaeKey,
                    var a when a.Contains("vae_decoder") => vaeKey,
                    _ => string.Empty
                };

                if (!string.IsNullOrEmpty(modelName))
                {
                    var modelCache = Path.Combine(modelPath, ".cache");
                    var dynamicDispatch = Path.Combine(Environment.CurrentDirectory, "Plugins", "RyzenAI");
                    sessionOptions.AddSessionConfigEntry("dd_root", dynamicDispatch);
                    sessionOptions.AddSessionConfigEntry("dd_cache", modelCache);
                    sessionOptions.AddSessionConfigEntry("onnx_custom_ops_const_key", modelCache);
                    sessionOptions.AddSessionConfigEntry("model_name", modelName);
                    sessionOptions.RegisterCustomOpLibrary("onnx_custom_ops.dll");
                }

                sessionOptions.AppendExecutionProvider_DML(deviceId);
                sessionOptions.AppendExecutionProvider_CPU();
                return sessionOptions;
            });
        }


        private static OnnxExecutionProvider AMDGPU(int deviceId, string cacheDirectory)
        {
            if (!Directory.Exists(cacheDirectory))
                Directory.CreateDirectory(cacheDirectory);

            return new OnnxExecutionProvider(nameof(ExecutionProvider.AMDGPU), configuration =>
            {
                var sessionOptions = new SessionOptions
                {
                    ExecutionMode = ExecutionMode.ORT_SEQUENTIAL,
                    GraphOptimizationLevel = GraphOptimizationLevel.ORT_DISABLE_ALL
                };

                sessionOptions.AppendExecutionProvider_MIGraphX(new OrtMIGraphXProviderOptions()
                {
                    DeviceId = deviceId,
                    ModelCacheDir = cacheDirectory
                });

                sessionOptions.AppendExecutionProvider_CPU();
                return sessionOptions;
            });
        }
    }
}
