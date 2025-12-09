using Amuse.UI.Models.FeatureExtractor;
using Amuse.UI.Models.Upscale;
using OnnxStack.Core;
using OnnxStack.StableDiffusion.Config;
using OnnxStack.StableDiffusion.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amuse.UI.Models.StableDiffusion
{
    public class StableDiffusionPipelineModel : INotifyPropertyChanged
    {
        private readonly StableDiffusionModelSetViewModel _baseModel;
        private readonly ControlNetModelSetViewModel _controlNetModel;
        private readonly UpscaleModelSetViewModel _upscaleModel;
        private readonly FeatureExtractorModelSetViewModel _featureExtractorModel;
        private readonly ContentFilterModelSetViewModel _contentFilterModel;

        public StableDiffusionPipelineModel(
            AmuseSettings settings,
            StableDiffusionModelSetViewModel baseModel,
            ControlNetModelSetViewModel controlNetModel,
            UpscaleModelSetViewModel upscaleModel,
            FeatureExtractorModelSetViewModel featureExtractorModel,
            ContentFilterModelSetViewModel contentFilterModel)
        {
            _baseModel = baseModel;
            _controlNetModel = controlNetModel;
            _upscaleModel = upscaleModel;
            _featureExtractorModel = featureExtractorModel;
            _contentFilterModel = contentFilterModel;
            if (_contentFilterModel is not null)
            {
                _contentFilterModel.ModelSet = _contentFilterModel.ModelSet with
                {
                    DeviceId = _baseModel.ModelSet.DeviceId ?? settings.DefaultExecutionDevice.DeviceId,
                    ExecutionProvider = _baseModel.ModelSet.ExecutionProvider ?? settings.DefaultExecutionDevice.Provider
                };
            }

            _baseModel.PropertyChanged += (s, e) => NotifyPropertyChanged(nameof(IsLoaded));
            if (_controlNetModel is not null)
                _controlNetModel.PropertyChanged += (s, e) => NotifyPropertyChanged(nameof(IsLoaded));
            if (_upscaleModel is not null)
                _upscaleModel.PropertyChanged += (s, e) => NotifyPropertyChanged(nameof(IsLoaded));
            if (_featureExtractorModel is not null)
                _featureExtractorModel.PropertyChanged += (s, e) => NotifyPropertyChanged(nameof(IsLoaded));
            if (_contentFilterModel is not null)
                _contentFilterModel.PropertyChanged += (s, e) => NotifyPropertyChanged(nameof(IsLoaded));
        }


        public string Name => _baseModel.Name;
        public PipelineType PipelineType => _baseModel.ModelSet.PipelineType;
        public SchedulerOptions DefaultSchedulerOptions => GetSchedulerOptions();
        public IReadOnlyList<SchedulerType> SupportedSchedulers => GetSchedulerTypes();
        public StableDiffusionModelSetViewModel BaseModel => _baseModel;
        public ControlNetModelSetViewModel ControlNetModel => _controlNetModel;
        public UpscaleModelSetViewModel UpscaleModel => _upscaleModel;
        public FeatureExtractorModelSetViewModel FeatureExtractorModel => _featureExtractorModel;
        public ContentFilterModelSetViewModel ContentFilterModel => _contentFilterModel;
        public ModelType ModelType => _baseModel.ModelSet.UnetConfig.ModelType;
        public int ContextSize => _baseModel.ModelSet.UnetConfig.ContextSize;
        public OptimizationType OptimizationType => _baseModel.Template.StableDiffusionTemplate.Optimization;
        public int SampleSize => _baseModel.ModelSet.SampleSize;

        public bool IsLoaded
        {
            get
            {
                return _baseModel.IsLoaded
                    && (_controlNetModel is null || _controlNetModel.IsLoaded)
                    && (_upscaleModel is null || _upscaleModel.IsLoaded)
                    && (_featureExtractorModel is null || _featureExtractorModel.IsLoaded)
                    && (_contentFilterModel is null || _contentFilterModel.IsLoaded);
            }
        }

        public bool IsControlNetEnabled => _controlNetModel is not null;

        public bool IsContentFilterEnabled => _contentFilterModel is not null;

        private SchedulerOptions GetSchedulerOptions()
        {
            if (BaseModel.ModelSet.SchedulerOptions is not null)
                return BaseModel.ModelSet.SchedulerOptions;

            return PipelineType switch
            {
                PipelineType.StableDiffusion => new SchedulerOptions
                {
                    InferenceSteps = 30,
                    GuidanceScale = 7.5f,
                    SchedulerType = SchedulerType.EulerAncestral
                },
                PipelineType.StableDiffusion2 => new SchedulerOptions
                {
                    Width = 768,
                    Height = 768,
                    InferenceSteps = 30,
                    GuidanceScale = 7.5f,
                    SchedulerType = SchedulerType.DDPM,
                    PredictionType = PredictionType.VariablePrediction
                },
                PipelineType.StableDiffusionXL => new SchedulerOptions
                {
                    Width = 1024,
                    Height = 1024,
                    InferenceSteps = 20,
                    GuidanceScale = 5f,
                    SchedulerType = SchedulerType.EulerAncestral
                },
                PipelineType.LatentConsistency => new SchedulerOptions
                {
                    InferenceSteps = 6,
                    GuidanceScale = 1f,
                    SchedulerType = SchedulerType.LCM
                },
                PipelineType.StableCascade => new SchedulerOptions
                {
                    Width = 1024,
                    Height = 1024,
                    InferenceSteps = 20,
                    GuidanceScale = 4f,
                    InferenceSteps2 = 10,
                    GuidanceScale2 = 0,
                    SchedulerType = SchedulerType.DDPMWuerstchen
                },
                PipelineType.StableDiffusion3 => new SchedulerOptions
                {
                    Shift = 3f,
                    Width = 1024,
                    Height = 1024,
                    InferenceSteps = 28,
                    GuidanceScale = 7f,
                    SchedulerType = SchedulerType.FlowMatchEulerDiscrete
                },
                PipelineType.Flux => new SchedulerOptions
                {
                    Shift = 1f,
                    Width = 1024,
                    Height = 1024,
                    InferenceSteps = 4,
                    GuidanceScale = 0,
                    SchedulerType = SchedulerType.FlowMatchEulerDiscrete
                },
                PipelineType.Locomotion => new SchedulerOptions
                {
                    Width = 512,
                    Height = 512,
                    InferenceSteps = 8,
                    GuidanceScale = 0f,
                    SchedulerType = SchedulerType.Locomotion
                },
                _ => new SchedulerOptions()
            };
        }


        private IReadOnlyList<SchedulerType> GetSchedulerTypes()
        {
            if (!BaseModel.ModelSet.Schedulers.IsNullOrEmpty())
                return BaseModel.ModelSet.Schedulers;

            return PipelineType switch
            {
                PipelineType.StableDiffusion => new List<SchedulerType>
                {
                    SchedulerType.LMS,
                    SchedulerType.Euler,
                    SchedulerType.EulerAncestral,
                    SchedulerType.DDPM,
                    SchedulerType.DDIM,
                    SchedulerType.KDPM2,
                    SchedulerType.KDPM2Ancestral,
                    SchedulerType.LCM
                },
                PipelineType.StableDiffusion2 => new List<SchedulerType>
                {
                    SchedulerType.DDPM,
                    SchedulerType.DDIM,
                },
                PipelineType.StableDiffusionXL => new List<SchedulerType>
                {
                    SchedulerType.LMS,
                    SchedulerType.Euler,
                    SchedulerType.EulerAncestral,
                    SchedulerType.DDPM,
                    SchedulerType.DDIM,
                    SchedulerType.KDPM2,
                    SchedulerType.KDPM2Ancestral,
                    SchedulerType.LCM
                },
                PipelineType.LatentConsistency => new List<SchedulerType>
                {
                    SchedulerType.LCM
                },
                PipelineType.StableCascade => new List<SchedulerType>
                {
                    SchedulerType.DDPMWuerstchen
                },
                PipelineType.StableDiffusion3 => new List<SchedulerType>
                {
                    SchedulerType.FlowMatchEulerDiscrete,
                    SchedulerType.FlowMatchEulerDynamic
                },
                PipelineType.Flux => new List<SchedulerType>
                {
                    SchedulerType.FlowMatchEulerDiscrete,
                    SchedulerType.FlowMatchEulerDynamic
                },
                PipelineType.Locomotion => new List<SchedulerType>
                {
                    SchedulerType.Locomotion
                },
                _ => throw new NotImplementedException()
            };
        }


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }


        #endregion
    }
}
