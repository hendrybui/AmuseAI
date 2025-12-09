using Amuse.UI.Models;
using Amuse.UI.Models.StableDiffusion;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime.Tensors;
using OnnxStack.Core;
using OnnxStack.Core.Model;
using OnnxStack.StableDiffusion.Enums;
using OnnxStack.StableDiffusion.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Amuse.UI.Services
{

    public class PreviewService : IPreviewService
    {
        private readonly ILogger<PreviewService> _logger;
        private readonly AmuseSettings _settings;
        private readonly IProviderService _providerService;
        private readonly Dictionary<PipelineType, AutoEncoderModelConfig> _previewModels;
        private AutoEncoderModel _previewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="PreviewService"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="logger">The logger.</param>
        public PreviewService(AmuseSettings settings, IProviderService providerService, ILogger<PreviewService> logger)
        {
            _logger = logger;
            _settings = settings;
            _providerService = providerService;
            _previewModels = new Dictionary<PipelineType, AutoEncoderModelConfig>();
            ConfigurePreviewModels();
        }


        /// <summary>
        /// Load the preview model
        /// </summary>
        /// <param name="stableDiffusionModelSet">The stable diffusion model set.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task LoadAsync(StableDiffusionModelSetJson stableDiffusionModelSet)
        {
            await UnloadAsync();

            if (!_settings.IsPreviewEnabled)
                return;

            if (_previewModels.TryGetValue(stableDiffusionModelSet.PipelineType, out var modelConfig))
            {
                var config = modelConfig with
                {
                    ExecutionProvider = _providerService.GetProvider(stableDiffusionModelSet.ExecutionProvider, stableDiffusionModelSet.DeviceId)
                };
                _previewModel = new AutoEncoderModel(config);
                if (_previewModel == null)
                    return;

                await _previewModel.LoadAsync();
            }
        }


        /// <summary>
        /// Unload the preview model
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task UnloadAsync()
        {
            if (_previewModel != null)
                await _previewModel.UnloadAsync();

            _previewModel = null;
        }


        /// <summary>
        /// Generates the preview image.
        /// </summary>
        /// <param name="latent">The latent.</param>
        /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>BitmapSource.</returns>
        public async Task<BitmapSource> GeneratePreview(DenseTensor<float> latent)
        {
            if (!_settings.IsPreviewEnabled)
                return null;

            var timestamp = _logger.LogBegin();
            if (latent == null || _previewModel == null)
                return null;

            try
            {
                var outputDim = new[] { 1, 3, latent.Dimensions[2] * 8, latent.Dimensions[3] * 8 };
                var metadata = await _previewModel.LoadAsync();
                using (var inferenceParameters = new OnnxInferenceParameters(metadata))
                {
                    inferenceParameters.AddInputTensor(latent);
                    inferenceParameters.AddOutputBuffer(outputDim);
                    var results = _previewModel.RunInference(inferenceParameters);
                    using (var imageResult = results.First())
                    {
                        return await imageResult
                            .ToDenseTensor()
                            .ToBitmapAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PreviewService] - An exception occured generation image preview");
                return null;
            }
            finally
            {
                _logger.LogEnd("[PreviewService] - Preview image", timestamp);
            }
        }


        /// <summary>
        /// Configures the preview models.
        /// </summary>
        private void ConfigurePreviewModels()
        {
            var stableDiffusionPath = Path.Combine(App.PluginDirectory, "Preview", "SD", "model.onnx");
            if (File.Exists(stableDiffusionPath))
            {
                var config = new AutoEncoderModelConfig
                {
                    ScaleFactor = 1,
                    OnnxModelPath = stableDiffusionPath
                };

                _previewModels.Add(PipelineType.StableDiffusion, config);
                _previewModels.Add(PipelineType.StableDiffusion2, config);
                _previewModels.Add(PipelineType.LatentConsistency, config);
                _logger.LogInformation("[PreviewService] - Loaded StableDiffusion preview model configuration");
                _logger.LogInformation("[PreviewService] - Loaded StableDiffusion2 preview model configuration");
                _logger.LogInformation("[PreviewService] - Loaded LatentConsistency preview model configuration");
            }

            var stableDiffusionXLPath = Path.Combine(App.PluginDirectory, "Preview", "SDXL", "model.onnx");
            if (File.Exists(stableDiffusionXLPath))
            {
                var config = new AutoEncoderModelConfig
                {
                    ScaleFactor = 1,
                    OnnxModelPath = stableDiffusionXLPath
                };

                _previewModels.Add(PipelineType.StableDiffusionXL, config);
                _logger.LogInformation("[PreviewService] - Loaded StableDiffusionXL preview model configuration");
            }

            var stableDiffusion3Path = Path.Combine(App.PluginDirectory, "Preview", "SD3", "model.onnx");
            if (File.Exists(stableDiffusion3Path))
            {
                var config = new AutoEncoderModelConfig
                {
                    ScaleFactor = 1,
                    OnnxModelPath = stableDiffusion3Path
                };

                _previewModels.Add(PipelineType.StableDiffusion3, config);
                _logger.LogInformation("[PreviewService] - Loaded StableDiffusion3 preview model configuration");
            }

            var fluxPath = Path.Combine(App.PluginDirectory, "Preview", "Flux", "model.onnx");
            if (File.Exists(fluxPath))
            {
                var config = new AutoEncoderModelConfig
                {
                    ScaleFactor = 1,
                    OnnxModelPath = fluxPath
                };

                _previewModels.Add(PipelineType.Flux, config);
                _logger.LogInformation("[PreviewService] - Loaded Flux preview model configuration");
            }
        }

    }
}
