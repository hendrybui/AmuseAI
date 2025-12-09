using Amuse.UI.Enums;
using Amuse.UI.Models.StableDiffusion;
using OnnxStack.StableDiffusion.Enums;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amuse.UI.Models
{
    public record MemoryInfoModel : INotifyPropertyChanged
    {
        private readonly PipelineType _pipeline;
        private readonly bool _isControlNet;
        private readonly float _deviceMemory;
        private readonly float _deviceSharedMemory;
        private readonly float _pipelineMemory;
        private readonly ModelDetails _modelDetails;
        private readonly float _baseMemoryMin;
        private readonly float _baseMemoryMax;
        private readonly float _controlNetMemory;
        private readonly float _extractorMemory;
        private readonly float _upscaleMemory;

        private bool _isLowMemoryPipelineEnabled;
        private bool _isLowMemoryComputeEnabled;
        private bool _isLowMemoryEncoderEnabled;
        private bool _isLowMemoryDecoderEnabled;
        private bool _isLowMemoryTextEncoderEnabled;
        private bool _isVaeTileEnabled;
        private float _memoryRequired;
        private bool _isImageinput;
        private MemoryMode _currentMode;


        public MemoryInfoModel(StableDiffusionPipelineModel pipeline, float deviceMemory, float deviceSharedMemory)
        {
            var internalMemory = 1f;
            var isXLPipeline = IsLargePipeline(pipeline.PipelineType);

            _baseMemoryMin = pipeline.BaseModel.Template.MemoryMin;
            _baseMemoryMax = pipeline.BaseModel.Template.MemoryMax;

            if (pipeline.FeatureExtractorModel?.Template is not null)
            {
                _extractorMemory = isXLPipeline
                    ? pipeline.FeatureExtractorModel.Template.MemoryMax
                    : pipeline.FeatureExtractorModel.Template.MemoryMin;
            }

            if (pipeline.UpscaleModel?.Template is not null)
            {
                _upscaleMemory = isXLPipeline
                    ? pipeline.UpscaleModel.Template.MemoryMax
                    : pipeline.UpscaleModel.Template.MemoryMin;
            }

            if (pipeline.ControlNetModel?.Template is not null)
            {
                _controlNetMemory = isXLPipeline
                    ? pipeline.ControlNetModel.Template.MemoryMax
                    : pipeline.ControlNetModel.Template.MemoryMin;
            }

            var isControlNet = IsControlNetDiffuser(pipeline);
            var pipelineMemory = _upscaleMemory + _extractorMemory + internalMemory;

            _pipeline = pipeline.PipelineType;
            _deviceMemory = deviceMemory;
            _deviceSharedMemory = deviceSharedMemory;
            _isControlNet = isControlNet;
            _pipelineMemory = pipelineMemory;
            _modelDetails = GetModelDetails(_pipeline);
        }


        public bool IsLowMemoryPipelineEnabled
        {
            get { return _isLowMemoryPipelineEnabled; }
            set { _isLowMemoryPipelineEnabled = value; NotifyPropertyChanged(); UpdateCalulation(); }
        }

        public bool IsLowMemoryComputeEnabled
        {
            get { return _isLowMemoryComputeEnabled; }
            set { _isLowMemoryComputeEnabled = value; NotifyPropertyChanged(); UpdateCalulation(); }
        }

        public bool IsLowMemoryEncoderEnabled
        {
            get { return _isLowMemoryEncoderEnabled; }
            set { _isLowMemoryEncoderEnabled = value; NotifyPropertyChanged(); UpdateCalulation(); }
        }

        public bool IsLowMemoryDecoderEnabled
        {
            get { return _isLowMemoryDecoderEnabled; }
            set { _isLowMemoryDecoderEnabled = value; NotifyPropertyChanged(); UpdateCalulation(); }
        }

        public bool IsLowMemoryTextEncoderEnabled
        {
            get { return _isLowMemoryTextEncoderEnabled; }
            set { _isLowMemoryTextEncoderEnabled = value; NotifyPropertyChanged(); UpdateCalulation(); }
        }

        public bool IsVaeTileEnabled
        {
            get { return _isVaeTileEnabled; }
            set { _isVaeTileEnabled = value; NotifyPropertyChanged(); UpdateCalulation(); }
        }

        public float DeviceMemory
        {
            get { return _deviceMemory; }
        }

        public float MemoryRequired
        {
            get { return _memoryRequired; }
            set { _memoryRequired = value; NotifyChanges(); }
        }

        public bool IsMemoryAvailable => DeviceMemory >= MemoryRequired;

        public bool IsMaximumSupported => IsMemoryAvailable
            && !_isLowMemoryPipelineEnabled
            && !_isLowMemoryComputeEnabled
            && !_isLowMemoryEncoderEnabled
            && !_isLowMemoryDecoderEnabled
            && !_isLowMemoryTextEncoderEnabled
            && !_isLowMemoryTextEncoderEnabled;

        public bool IsUnsupported => MemoryRequired > (_deviceMemory + _deviceSharedMemory);


        public void Update(MemoryMode memoryMode, DiffuserType diffuserType, float strength)
        {
            _currentMode = memoryMode;
            _isImageinput = IsImageInput(diffuserType, strength);
            SetMemoryMode(_currentMode);
        }

        private void SetMemoryMode(MemoryMode memoryMode)
        {
            _currentMode = memoryMode;
            if (_currentMode == MemoryMode.Maximum)
            {
                _isLowMemoryPipelineEnabled = false;
                _isLowMemoryComputeEnabled = false;
                _isLowMemoryEncoderEnabled = false;
                _isLowMemoryDecoderEnabled = false;
                _isLowMemoryTextEncoderEnabled = false;
                _isLowMemoryTextEncoderEnabled = false;
                UpdateCalulation();
            }
            else if (_currentMode == MemoryMode.Minimum)
            {
                _isLowMemoryPipelineEnabled = true;
                _isLowMemoryComputeEnabled = true;
                _isLowMemoryEncoderEnabled = true;
                _isLowMemoryDecoderEnabled = true;
                _isLowMemoryTextEncoderEnabled = true;
                _isVaeTileEnabled = _pipeline == PipelineType.StableDiffusionXL;
                UpdateCalulation();
            }
            else if (_currentMode == MemoryMode.Auto || _currentMode == MemoryMode.Custom)
            {
                UpdateCalulation(true);
            }

            NotifyChanges();
        }


        private void UpdateCalulation(bool isAuto = false)
        {
            var memoryValue = isAuto ? AutoCalulateMemory() : CalulateMemory();
            MemoryRequired = Math.Max(_baseMemoryMin, memoryValue);
        }


        private float CalulateMemory()
        {
            var tiledVaeSize = _modelDetails.TiledVaeSize;
            var decoderSize = _modelDetails.DecoderSize;
            var encoderSize = _modelDetails.EncoderSize;
            var textEncoderSize = _modelDetails.TextEncoderSize;

            var required = _baseMemoryMax + _pipelineMemory;
            if (_isLowMemoryPipelineEnabled)
                required -= _pipelineMemory;

            // ControlNet
            if (_isControlNet)
                required += _controlNetMemory;


            // Encoder/Decoder
            if (_isVaeTileEnabled)
            {
                required -= tiledVaeSize;
                encoderSize -= tiledVaeSize;
                decoderSize -= tiledVaeSize;
            }

            if (_isImageinput)
                required += encoderSize;

            if (_isLowMemoryComputeEnabled)
            {
                if (_isLowMemoryTextEncoderEnabled)
                    required -= textEncoderSize;
                if (_isImageinput && _isLowMemoryEncoderEnabled && _isLowMemoryDecoderEnabled)
                    required -= encoderSize;
                if (_isLowMemoryDecoderEnabled)
                    required -= decoderSize;
            }
            else
            {
                if ((_isImageinput && _isLowMemoryEncoderEnabled) && _isLowMemoryDecoderEnabled)
                {
                    if (_isLowMemoryTextEncoderEnabled)
                        required -= Math.Min(encoderSize + textEncoderSize, decoderSize);
                    else
                        required -= Math.Min(encoderSize, decoderSize);
                }
                else if (_isLowMemoryDecoderEnabled)
                {
                    if (_isLowMemoryTextEncoderEnabled)
                        required -= textEncoderSize;
                }
            }

            return Math.Max(_baseMemoryMin, required);
        }


        private float AutoCalulateMemory()
        {
            _isVaeTileEnabled = false;
            _isLowMemoryPipelineEnabled = false;
            _isLowMemoryComputeEnabled = false;
            _isLowMemoryEncoderEnabled = false;
            _isLowMemoryDecoderEnabled = false;
            _isLowMemoryTextEncoderEnabled = false;

            var tiledVaeSize = _modelDetails.TiledVaeSize;
            var decoderSize = _modelDetails.DecoderSize;
            var encoderSize = _modelDetails.EncoderSize;
            var textEncoderSize = _modelDetails.TextEncoderSize;

            var required = _baseMemoryMax + _pipelineMemory;
            if (_isControlNet)
                required += _controlNetMemory;
            if (_isImageinput)
                required += encoderSize;
            if (_deviceMemory > required)
                return required;

            required -= _pipelineMemory;
            _isLowMemoryPipelineEnabled = true;
            if (_deviceMemory > required)
                return required;

            required -= Math.Min(decoderSize, encoderSize);
            _isLowMemoryEncoderEnabled = true;
            _isLowMemoryDecoderEnabled = true;
            if (_deviceMemory > required)
                return required;

            required -= textEncoderSize;
            _isLowMemoryTextEncoderEnabled = true;
            if (_deviceMemory > required)
                return required;

            // VAE Tiling
            if (_pipeline == PipelineType.StableDiffusionXL)
            {
                required -= tiledVaeSize;
                if (_isImageinput)
                    required -= tiledVaeSize;

                _isVaeTileEnabled = true;
                if (_deviceMemory > required)
                    return required;
            }

            _isLowMemoryComputeEnabled = true;
            return _baseMemoryMin;
        }


        private ModelDetails GetModelDetails(PipelineType pipelineType)
        {
            switch (pipelineType)
            {
                case PipelineType.StableDiffusion:
                case PipelineType.StableDiffusion2:
                case PipelineType.LatentConsistency:
                case PipelineType.Locomotion:
                    return new ModelDetails(0.5f, 2f, 2f, 0f);
                case PipelineType.StableDiffusionXL:
                    return new ModelDetails(1.5f, 3f, 4f, 2f);
                case PipelineType.StableCascade:
                    return new ModelDetails(1.5f, 3f, 4f, 0f);
                case PipelineType.StableDiffusion3:
                    return new ModelDetails(4f, 3f, 4f, 0f);
                case PipelineType.Flux:
                    return new ModelDetails(4f, 6f, 6f, 0);
                default:
                    break;
            }
            return new ModelDetails(0.5f, 2f, 2f, 0f);
        }


        private bool IsImageInput(DiffuserType diffuserType, float strength)
        {
            if (_isControlNet && strength >= 1)
                return false;

            return diffuserType == DiffuserType.ControlNetImage
                || diffuserType == DiffuserType.ControlNetVideo
                || diffuserType == DiffuserType.ImageToImage
                || diffuserType == DiffuserType.ImageInpaint
                || diffuserType == DiffuserType.ImageInpaintLegacy
                || diffuserType == DiffuserType.ImageToVideo
                || diffuserType == DiffuserType.VideoToVideo;
        }

        private bool IsControlNetDiffuser(StableDiffusionPipelineModel pipeline)
        {
            return pipeline.ControlNetModel is not null;
        }

        private bool IsLargePipeline(PipelineType pipelineType)
        {
            switch (pipelineType)
            {
                case PipelineType.StableDiffusion:
                case PipelineType.StableDiffusion2:
                case PipelineType.LatentConsistency:
                case PipelineType.Locomotion:
                    return false;
                case PipelineType.StableDiffusionXL:
                case PipelineType.StableDiffusion3:
                case PipelineType.StableCascade:
                case PipelineType.Flux:
                    return true;
                default:
                    break;
            }
            return false;
        }


        private void NotifyChanges()
        {
            NotifyPropertyChanged(nameof(IsLowMemoryPipelineEnabled));
            NotifyPropertyChanged(nameof(IsLowMemoryComputeEnabled));
            NotifyPropertyChanged(nameof(IsLowMemoryEncoderEnabled));
            NotifyPropertyChanged(nameof(IsLowMemoryDecoderEnabled));
            NotifyPropertyChanged(nameof(IsLowMemoryTextEncoderEnabled));
            NotifyPropertyChanged(nameof(IsVaeTileEnabled));
            NotifyPropertyChanged(nameof(MemoryRequired));
            NotifyPropertyChanged(nameof(IsMemoryAvailable));
            NotifyPropertyChanged(nameof(IsMaximumSupported));
            NotifyPropertyChanged(nameof(IsUnsupported));
        }

        private record ModelDetails(float TextEncoderSize, float EncoderSize, float DecoderSize, float TiledVaeSize);

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion
    }
}
