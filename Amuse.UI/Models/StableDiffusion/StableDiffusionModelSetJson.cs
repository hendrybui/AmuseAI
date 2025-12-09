using Amuse.UI.Enums;
using OnnxStack.Core.Model;
using OnnxStack.StableDiffusion.Config;
using OnnxStack.StableDiffusion.Enums;
using OnnxStack.StableDiffusion.Models;
using OnnxStack.StableDiffusion.Tokenizers;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Amuse.UI.Models.StableDiffusion
{
    public class StableDiffusionModelSetJson
    {
        public string Name { get; set; }
        public int SampleSize { get; set; } = 512;
        public PipelineType PipelineType { get; set; }
        public List<DiffuserType> Diffusers { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<SchedulerType> Schedulers { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SchedulerOptions SchedulerOptions { get; set; }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? DeviceId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ExecutionProvider? ExecutionProvider { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TokenizerModelJson TokenizerConfig { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TokenizerModelJson Tokenizer2Config { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TokenizerModelJson Tokenizer3Config { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TextEncoderModelJson TextEncoderConfig { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TextEncoderModelJson TextEncoder2Config { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TextEncoderModelJson TextEncoder3Config { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public UNetConditionModelJson UnetConfig { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public UNetConditionModelJson Unet2Config { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public AutoEncoderModelJson VaeDecoderConfig { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public AutoEncoderModelJson VaeEncoderConfig { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public UNetConditionModelJson ControlNetUnetConfig { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public FlowEstimationModelJson FlowEstimationConfig { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ResampleModelJson ResampleModelConfig { get; set; }


        public StableDiffusionModelSet ToModelConfig(OnnxExecutionProvider executionProvider)
        {
            return new StableDiffusionModelSet
            {
                Name = Name,
                SampleSize = SampleSize,
                PipelineType = PipelineType,
                Diffusers = Diffusers,
                Schedulers = Schedulers,
                SchedulerOptions = SchedulerOptions,
                ExecutionProvider = executionProvider,
                TokenizerConfig = TokenizerConfig?.ToModelConfig(executionProvider),
                Tokenizer2Config = Tokenizer2Config?.ToModelConfig(executionProvider),
                Tokenizer3Config = Tokenizer3Config?.ToModelConfig(executionProvider),
                TextEncoderConfig = TextEncoderConfig?.ToModelConfig(executionProvider),
                TextEncoder2Config = TextEncoder2Config?.ToModelConfig(executionProvider),
                TextEncoder3Config = TextEncoder3Config?.ToModelConfig(executionProvider),
                VaeDecoderConfig = VaeDecoderConfig?.ToModelConfig(executionProvider),
                VaeEncoderConfig = VaeEncoderConfig?.ToModelConfig(executionProvider),
                UnetConfig = UnetConfig?.ToModelConfig(executionProvider),
                Unet2Config = Unet2Config?.ToModelConfig(executionProvider),
                ControlNetUnetConfig = ControlNetUnetConfig?.ToModelConfig(executionProvider),
                FlowEstimationConfig = FlowEstimationConfig?.ToModelConfig(executionProvider),
                ResampleModelConfig = ResampleModelConfig?.ToModelConfig(executionProvider)
            };
        }

    }

    public record TokenizerModelJson : OnnxModelJson
    {
        public int TokenizerLimit { get; set; } = 77;
        public int TokenizerLength { get; set; } = 768;
        public int PadTokenId { get; set; } = 49407;
        public int BlankTokenId { get; set; } = 49407;

        public TokenizerConfig ToModelConfig(OnnxExecutionProvider executionProvider)
        {
            return new TokenizerConfig
            {
                BlankTokenId = BlankTokenId,
                ExecutionProvider = executionProvider,
                IsOptimizationSupported = true,
                OnnxModelPath = OnnxModelPath,
                PadTokenId = PadTokenId,
                TokenizerLength = TokenizerLength,
                TokenizerLimit = TokenizerLimit
            };
        }
    }


    public record TextEncoderModelJson : OnnxModelJson
    {
        public TextEncoderModelConfig ToModelConfig(OnnxExecutionProvider executionProvider)
        {
            return new TextEncoderModelConfig
            {
                IsOptimizationSupported = true,
                OnnxModelPath = OnnxModelPath,
                ExecutionProvider = executionProvider
            };
        }
    }


    public record UNetConditionModelJson : OnnxModelJson
    {
        public ModelType ModelType { get; set; }
        public int ContextSize { get; set; }
        public int FrameRate { get; set; } = 8;

        public UNetConditionModelConfig ToModelConfig(OnnxExecutionProvider executionProvider)
        {
            return new UNetConditionModelConfig
            {
                ContextSize = ContextSize,
                OnnxModelPath = OnnxModelPath,
                FrameRate = FrameRate,
                ModelType = ModelType,
                IsOptimizationSupported = true,
                ExecutionProvider = executionProvider
            };
        }
    }

    public record AutoEncoderModelJson : OnnxModelJson
    {
        public float ScaleFactor { get; set; }

        public AutoEncoderModelConfig ToModelConfig(OnnxExecutionProvider executionProvider)
        {
            return new AutoEncoderModelConfig
            {
                ScaleFactor = ScaleFactor,
                IsOptimizationSupported = true,
                OnnxModelPath = OnnxModelPath,
                ExecutionProvider = executionProvider
            };
        }
    }

    public record FlowEstimationModelJson : OnnxModelJson
    {
        public FlowEstimationModelConfig ToModelConfig(OnnxExecutionProvider executionProvider)
        {
            return new FlowEstimationModelConfig
            {
                IsOptimizationSupported = true,
                OnnxModelPath = OnnxModelPath,
                ExecutionProvider = executionProvider
            };
        }
    }

    public record ResampleModelJson : OnnxModelJson
    {
        public ResampleModelConfig ToModelConfig(OnnxExecutionProvider executionProvider)
        {
            return new ResampleModelConfig
            {
                IsOptimizationSupported = true,
                OnnxModelPath = OnnxModelPath,
                ExecutionProvider = executionProvider
            };
        }
    }


    public record OnnxModelJson
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? DeviceId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ExecutionProvider? ExecutionProvider { get; set; }
      
        public string OnnxModelPath { get; set; }
    }
}
