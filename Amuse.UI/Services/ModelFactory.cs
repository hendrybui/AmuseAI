using Amuse.UI.Models;
using Amuse.UI.Models.FeatureExtractor;
using Amuse.UI.Models.StableDiffusion;
using Amuse.UI.Models.Upscale;
using OnnxStack.Core;
using OnnxStack.StableDiffusion.Config;
using OnnxStack.StableDiffusion.Enums;
using System.IO;
using System.Linq;

namespace Amuse.UI.Services
{
    public class ModelFactory : IModelFactory
    {
        private readonly AmuseSettings _settings;
        private readonly string _defaultTokenizerPath;

        public ModelFactory(AmuseSettings settings)
        {
            _settings = settings;
            var defaultTokenizerPath = Path.Combine(App.BaseDirectory, "cliptokenizer.onnx");
            if (File.Exists(defaultTokenizerPath))
                _defaultTokenizerPath = defaultTokenizerPath;
        }

        public StableDiffusionModelSetJson CreateStableDiffusionModelSet(string name, string folder, string modelTemplateType)
        {
            var template = _settings.Templates
                .Where(x => x.Category == ModelTemplateCategory.StableDiffusion && x.Template == modelTemplateType && x.Group == ModelTemplateGroup.Fixed)
                .FirstOrDefault();
            if (template == null)
                return null;

            return CreateStableDiffusionModelSet(name, folder, template.StableDiffusionTemplate);
        }

        public StableDiffusionModelSetJson CreateStableDiffusionModelSet(string name, string folder, StableDiffusionModelTemplate modelTemplate)
        {
            var modelSet = default(StableDiffusionModelSet);
            if (modelTemplate.PipelineType == PipelineType.Flux)
            {
                modelSet = OnnxStack.StableDiffusion.Helpers.ModelFactory.CreateFluxModelSet(folder, modelTemplate.ModelType);
            }
            else if (modelTemplate.PipelineType == PipelineType.Locomotion)
            {
                modelSet = OnnxStack.StableDiffusion.Helpers.ModelFactory.CreateLocomotionModelSet(folder, modelTemplate.ContextSize);
            }
            else
            {
                modelSet = OnnxStack.StableDiffusion.Helpers.ModelFactory.CreateModelSet(folder, modelTemplate.PipelineType, modelTemplate.ModelType);
            }

            modelSet.Name = name;
            if (!modelTemplate.Schedulers.IsNullOrEmpty())
                modelSet.Schedulers = modelTemplate.Schedulers.ToList();

            return new StableDiffusionModelSetJson
            {
                Name = modelSet.Name,
                Diffusers = modelSet.Diffusers,
                PipelineType = modelSet.PipelineType,
                SampleSize = modelSet.SampleSize,
                SchedulerOptions = modelSet.SchedulerOptions,
                Schedulers = modelSet.Schedulers,
                TokenizerConfig = modelSet.TokenizerConfig == null ? null : new TokenizerModelJson
                {
                    OnnxModelPath = modelSet.TokenizerConfig.OnnxModelPath,
                    BlankTokenId = modelSet.TokenizerConfig.BlankTokenId,
                    PadTokenId = modelSet.TokenizerConfig.PadTokenId,
                    TokenizerLength = modelSet.TokenizerConfig.TokenizerLength,
                    TokenizerLimit = modelSet.TokenizerConfig.TokenizerLimit
                },
                Tokenizer2Config = modelSet.Tokenizer2Config == null ? null : new TokenizerModelJson
                {
                    OnnxModelPath = modelSet.Tokenizer2Config.OnnxModelPath,
                    BlankTokenId = modelSet.Tokenizer2Config.BlankTokenId,
                    PadTokenId = modelSet.Tokenizer2Config.PadTokenId,
                    TokenizerLength = modelSet.Tokenizer2Config.TokenizerLength,
                    TokenizerLimit = modelSet.Tokenizer2Config.TokenizerLimit
                },
                Tokenizer3Config = modelSet.Tokenizer3Config == null ? null : new TokenizerModelJson
                {
                    OnnxModelPath = modelSet.Tokenizer3Config.OnnxModelPath,
                    BlankTokenId = modelSet.Tokenizer3Config.BlankTokenId,
                    PadTokenId = modelSet.Tokenizer3Config.PadTokenId,
                    TokenizerLength = modelSet.Tokenizer3Config.TokenizerLength,
                    TokenizerLimit = modelSet.Tokenizer3Config.TokenizerLimit
                },
                TextEncoderConfig = modelSet.TextEncoderConfig == null ? null : new TextEncoderModelJson
                {
                    OnnxModelPath = modelSet.TextEncoderConfig.OnnxModelPath
                },
                TextEncoder2Config = modelSet.TextEncoder2Config == null ? null : new TextEncoderModelJson
                {
                    OnnxModelPath = modelSet.TextEncoder2Config.OnnxModelPath
                },
                TextEncoder3Config = modelSet.TextEncoder3Config == null ? null : new TextEncoderModelJson
                {
                    OnnxModelPath = modelSet.TextEncoder3Config.OnnxModelPath
                },
                VaeEncoderConfig = modelSet.VaeEncoderConfig == null ? null : new AutoEncoderModelJson
                {
                    OnnxModelPath = modelSet.VaeEncoderConfig.OnnxModelPath,
                    ScaleFactor = modelSet.VaeEncoderConfig.ScaleFactor
                },
                VaeDecoderConfig = modelSet.VaeDecoderConfig == null ? null : new AutoEncoderModelJson
                {
                    OnnxModelPath = modelSet.VaeDecoderConfig.OnnxModelPath,
                    ScaleFactor = modelSet.VaeDecoderConfig.ScaleFactor
                },
                UnetConfig = modelSet.UnetConfig == null ? null : new UNetConditionModelJson
                {
                    ModelType = modelSet.UnetConfig.ModelType,
                    ContextSize = modelSet.UnetConfig.ContextSize,
                    FrameRate = modelSet.UnetConfig.FrameRate,
                    OnnxModelPath = modelSet.UnetConfig.OnnxModelPath
                },
                Unet2Config = modelSet.Unet2Config == null ? null : new UNetConditionModelJson
                {
                    ModelType = modelSet.Unet2Config.ModelType,
                    ContextSize = modelSet.Unet2Config.ContextSize,
                    FrameRate = modelSet.Unet2Config.FrameRate,
                    OnnxModelPath = modelSet.Unet2Config.OnnxModelPath
                },
                ControlNetUnetConfig = modelSet.ControlNetUnetConfig == null ? null : new UNetConditionModelJson
                {
                    ModelType = modelSet.ControlNetUnetConfig.ModelType,
                    ContextSize = modelSet.ControlNetUnetConfig.ContextSize,
                    FrameRate = modelSet.ControlNetUnetConfig.FrameRate,
                    OnnxModelPath = modelSet.ControlNetUnetConfig.OnnxModelPath
                },
                FlowEstimationConfig = modelSet.FlowEstimationConfig == null ? null : new FlowEstimationModelJson
                {
                    OnnxModelPath = modelSet.FlowEstimationConfig.OnnxModelPath
                },
                ResampleModelConfig = modelSet.ResampleModelConfig == null ? null : new ResampleModelJson
                {
                    OnnxModelPath = modelSet.ResampleModelConfig.OnnxModelPath
                }
            };
        }


        public UpscaleModelJson CreateUpscaleModelSet(string name, string filename, string modelTemplateType)
        {
            var template = _settings.Templates
               .Where(x => x.Category == ModelTemplateCategory.Upscaler && x.Template == modelTemplateType && x.Group == ModelTemplateGroup.Fixed)
               .FirstOrDefault();
            if (template == null)
                return null;

            return CreateUpscaleModelSet(name, filename, template.UpscaleTemplate);
        }


        public UpscaleModelJson CreateUpscaleModelSet(string name, string filename, UpscaleModelTemplate modelTemplate)
        {
            return new UpscaleModelJson
            {
                Name = name,
                DeviceId = _settings.DefaultExecutionDevice.DeviceId,
                ExecutionProvider = _settings.DefaultExecutionDevice.Provider,
                Channels = modelTemplate.Channels,
                SampleSize = modelTemplate.SampleSize,
                ScaleFactor = modelTemplate.ScaleFactor,
                NormalizeType = modelTemplate.NormalizeType,
                TileMode = modelTemplate.TileMode,
                TileSize = modelTemplate.TileSize,
                TileOverlap = modelTemplate.TileOverlap,
                OnnxModelPath = filename
            };
        }


        public ControlNetModelJson CreateControlNetModelSet(string name, string filename, string modelTemplateType)
        {
            var template = _settings.Templates
               .Where(x => x.Category == ModelTemplateCategory.ControlNet && x.Template == modelTemplateType && x.Group == ModelTemplateGroup.Fixed)
               .FirstOrDefault();
            if (template == null)
                return null;

            return CreateControlNetModelSet(name, filename, template.ControlNetTemplate);
        }


        public ControlNetModelJson CreateControlNetModelSet(string name, string filename, ControlNetModelTemplate modelTemplate)
        {
            return new ControlNetModelJson
            {
                Name = name,
                DeviceId = _settings.DefaultExecutionDevice.DeviceId,
                ExecutionProvider = _settings.DefaultExecutionDevice.Provider,
                OnnxModelPath = filename,
                InvertInput = modelTemplate.InvertInput,
                LayerCount = modelTemplate.LayerCount,
                DisablePooledProjection = modelTemplate.DisablePooledProjection
            };
        }



        public FeatureExtractorModelJson CreateFeatureExtractorModelSet(string name, string filename, string modelTemplateType)
        {
            var template = _settings.Templates
               .Where(x => x.Category == ModelTemplateCategory.FeatureExtractor && x.Template == modelTemplateType && x.Group == ModelTemplateGroup.Fixed)
               .FirstOrDefault();
            if (template == null)
                return null;

            return CreateFeatureExtractorModelSet(name, filename, template.FeatureExtractorTemplate);
        }


        public FeatureExtractorModelJson CreateFeatureExtractorModelSet(string name, string filename, FeatureExtractorModelTemplate modelTemplate)
        {
            return new FeatureExtractorModelJson
            {
                Name = name,
                DeviceId = _settings.DefaultExecutionDevice.DeviceId,
                ExecutionProvider = _settings.DefaultExecutionDevice.Provider,
                OutputChannels = modelTemplate.OutputChannels,
                SampleSize = modelTemplate.SampleSize,
                InputResizeMode = modelTemplate.InputResizeMode,
                SetOutputToInputAlpha = modelTemplate.SetOutputToInputAlpha,
                InvertOutput = modelTemplate.InvertOutput,
                NormalizeType = modelTemplate.NormalizeType,
                NormalizeOutputType = modelTemplate.NormalizeOutputType,
                OnnxModelPath = filename
            };
        }

    }
}
