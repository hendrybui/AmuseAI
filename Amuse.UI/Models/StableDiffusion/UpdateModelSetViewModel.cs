using Amuse.UI.Enums;
using OnnxStack.StableDiffusion.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amuse.UI.Models.StableDiffusion
{
    public class UpdateModelSetViewModel : INotifyPropertyChanged
    {
        private string _name;
        private int _deviceId;
        private ExecutionProvider _executionProvider;
        private bool _enableTextToImage;
        private bool _enableImageToImage;
        private bool _enableImageInpaint;
        private bool _enableImageInpaintLegacy;
        private bool _enableControlNet;
        private bool _enableControlNetImage;
        private bool _enableControlNetVideo;
        private bool _enableTextToVideo;
        private bool _enableImageToVideo;
        private bool _enableVideoToVideo;
        private PipelineType _pipelineType;
        private int _sampleSize;
        private int _padTokenId;
        private int _blankTokenId;
        private float _scaleFactor;
        private int _tokenizerLimit;
        private int _tokenizerLength;
        private int _tokenizer2Length;
        private int _tokenizer3Length;
        private ModelType _modelType;
        private ModelFileViewModel _unetModel;
        private ModelFileViewModel _unet2Model;
        private ModelFileViewModel _vaeEncoderModel;
        private ModelFileViewModel _vaeDecoderModel;
        private ModelFileViewModel _textEncoderModel;
        private ModelFileViewModel _textEncoder2Model;
        private ModelFileViewModel _textEncoder3Model;
        private ModelFileViewModel _tokenizerModel;
        private ModelFileViewModel _tokenizer2Model;
        private ModelFileViewModel _tokenizer3Model;
        private ModelFileViewModel _controlNetModelModel;
        private int _tokenizer2Limit;
        private int _contextSize;
        private ModelFileViewModel _flowEstimationModel;
        private ModelFileViewModel _resampleModel;

        public string Name
        {
            get { return _name; }
            set { _name = value; NotifyPropertyChanged(); }
        }

        public int PadTokenId
        {
            get { return _padTokenId; }
            set { _padTokenId = value; NotifyPropertyChanged(); }
        }
        public int BlankTokenId
        {
            get { return _blankTokenId; }
            set { _blankTokenId = value; NotifyPropertyChanged(); }
        }

        public int SampleSize
        {
            get { return _sampleSize; }
            set { _sampleSize = value; NotifyPropertyChanged(); }
        }

        public float ScaleFactor
        {
            get { return _scaleFactor; }
            set { _scaleFactor = value; NotifyPropertyChanged(); }
        }

        public int TokenizerLimit
        {
            get { return _tokenizerLimit; }
            set { _tokenizerLimit = value; NotifyPropertyChanged(); }
        }

        public int Tokenizer2Limit
        {
            get { return _tokenizer2Limit; }
            set { _tokenizer2Limit = value; NotifyPropertyChanged(); }
        }

        public int TokenizerLength
        {
            get { return _tokenizerLength; }
            set { _tokenizerLength = value; NotifyPropertyChanged(); }
        }

        public int Tokenizer2Length
        {
            get { return _tokenizer2Length; }
            set { _tokenizer2Length = value; NotifyPropertyChanged(); }
        }

        public int Tokenizer3Length
        {
            get { return _tokenizer3Length; }
            set { _tokenizer3Length = value; NotifyPropertyChanged(); }
        }

        public bool EnableTextToImage
        {
            get { return _enableTextToImage; }
            set { _enableTextToImage = value; NotifyPropertyChanged(); }
        }

        public bool EnableImageToImage
        {
            get { return _enableImageToImage; }
            set { _enableImageToImage = value; NotifyPropertyChanged(); }
        }

        public bool EnableImageInpaint
        {
            get { return _enableImageInpaint; }
            set
            { _enableImageInpaint = value; NotifyPropertyChanged(); }
        }

        public bool EnableImageInpaintLegacy
        {
            get { return _enableImageInpaintLegacy; }
            set { _enableImageInpaintLegacy = value; NotifyPropertyChanged(); }
        }

        public bool EnableControlNet
        {
            get { return _enableControlNet; }
            set { _enableControlNet = value; NotifyPropertyChanged(); }
        }

        public bool EnableControlNetImage
        {
            get { return _enableControlNetImage; }
            set { _enableControlNetImage = value; NotifyPropertyChanged(); }
        }

        public bool EnableControlNetVideo
        {
            get { return _enableControlNetVideo; }
            set { _enableControlNetVideo = value; NotifyPropertyChanged(); }
        }

        public bool EnableTextToVideo
        {
            get { return _enableTextToVideo; }
            set { _enableTextToVideo = value; NotifyPropertyChanged(); }
        }

        public bool EnableImageToVideo
        {
            get { return _enableImageToVideo; }
            set { _enableImageToVideo = value; NotifyPropertyChanged(); }
        }

        public bool EnableVideoToVideo
        {
            get { return _enableVideoToVideo; }
            set { _enableVideoToVideo = value; NotifyPropertyChanged(); }
        }

        public int DeviceId
        {
            get { return _deviceId; }
            set { _deviceId = value; NotifyPropertyChanged(); }
        }

        public ExecutionProvider ExecutionProvider
        {
            get { return _executionProvider; }
            set { _executionProvider = value; NotifyPropertyChanged(); }
        }

        public PipelineType PipelineType
        {
            get { return _pipelineType; }
            set { _pipelineType = value; NotifyPropertyChanged(); }
        }

        public ModelType ModelType
        {
            get { return _modelType; }
            set { _modelType = value; NotifyPropertyChanged(); }
        }


        public ModelFileViewModel UnetModel
        {
            get { return _unetModel; }
            set { _unetModel = value; NotifyPropertyChanged(); }
        }

        public ModelFileViewModel Unet2Model
        {
            get { return _unet2Model; }
            set { _unet2Model = value; NotifyPropertyChanged(); }
        }

        public ModelFileViewModel TokenizerModel
        {
            get { return _tokenizerModel; }
            set { _tokenizerModel = value; NotifyPropertyChanged(); }
        }

        public ModelFileViewModel Tokenizer2Model
        {
            get { return _tokenizer2Model; }
            set { _tokenizer2Model = value; NotifyPropertyChanged(); }
        }

        public ModelFileViewModel Tokenizer3Model
        {
            get { return _tokenizer3Model; }
            set { _tokenizer3Model = value; NotifyPropertyChanged(); }
        }


        public ModelFileViewModel TextEncoderModel
        {
            get { return _textEncoderModel; }
            set { _textEncoderModel = value; NotifyPropertyChanged(); }
        }

        public ModelFileViewModel TextEncoder2Model
        {
            get { return _textEncoder2Model; }
            set { _textEncoder2Model = value; NotifyPropertyChanged(); }
        }

        public ModelFileViewModel TextEncoder3Model
        {
            get { return _textEncoder3Model; }
            set { _textEncoder3Model = value; NotifyPropertyChanged(); }
        }

        public ModelFileViewModel VaeDecoderModel
        {
            get { return _vaeDecoderModel; }
            set { _vaeDecoderModel = value; NotifyPropertyChanged(); }
        }

        public ModelFileViewModel VaeEncoderModel
        {
            get { return _vaeEncoderModel; }
            set { _vaeEncoderModel = value; NotifyPropertyChanged(); }
        }

        public ModelFileViewModel ControlNetModel
        {
            get { return _controlNetModelModel; }
            set { _controlNetModelModel = value; NotifyPropertyChanged(); }
        }

        public ModelFileViewModel FlowEstimationModel
        {
            get { return _flowEstimationModel; }
            set { _flowEstimationModel = value; NotifyPropertyChanged(); }
        }

        public ModelFileViewModel ResampleModel
        {
            get { return _resampleModel; }
            set { _resampleModel = value; NotifyPropertyChanged(); }
        }

        public int ContextSize
        {
            get { return _contextSize; }
            set { _contextSize = value; NotifyPropertyChanged(); }
        }

        public IEnumerable<DiffuserType> GetDiffusers()
        {
            if (_enableTextToImage)
                yield return DiffuserType.TextToImage;
            if (_enableImageToImage)
                yield return DiffuserType.ImageToImage;
            if (_enableImageInpaint)
                yield return DiffuserType.ImageInpaint;
            if (_enableImageInpaintLegacy)
                yield return DiffuserType.ImageInpaintLegacy;
            if (_enableControlNet)
                yield return DiffuserType.ControlNet;
            if (_enableControlNetImage)
                yield return DiffuserType.ControlNetImage;
            if (_enableControlNetVideo)
                yield return DiffuserType.ControlNetVideo;
            if (_enableTextToVideo)
                yield return DiffuserType.TextToVideo;
            if (_enableImageToVideo)
                yield return DiffuserType.ImageToVideo;
            if (_enableVideoToVideo)
                yield return DiffuserType.VideoToVideo;
        }


        public static UpdateModelSetViewModel FromModelSet(AmuseSettings settings, StableDiffusionModelSetJson modelset)
        {
            return new UpdateModelSetViewModel
            {
                EnableImageInpaint = modelset.Diffusers.Contains(DiffuserType.ImageInpaint),
                EnableImageInpaintLegacy = modelset.Diffusers.Contains(DiffuserType.ImageInpaintLegacy),
                EnableImageToImage = modelset.Diffusers.Contains(DiffuserType.ImageToImage),
                EnableTextToImage = modelset.Diffusers.Contains(DiffuserType.TextToImage),
                EnableControlNet = modelset.Diffusers.Contains(DiffuserType.ControlNet),
                EnableControlNetImage = modelset.Diffusers.Contains(DiffuserType.ControlNetImage),
                EnableControlNetVideo = modelset.Diffusers.Contains(DiffuserType.ControlNetVideo),
                EnableTextToVideo = modelset.Diffusers.Contains(DiffuserType.TextToVideo),
                EnableImageToVideo = modelset.Diffusers.Contains(DiffuserType.ImageToVideo),
                EnableVideoToVideo = modelset.Diffusers.Contains(DiffuserType.VideoToVideo),
                DeviceId = modelset.DeviceId ?? settings.DefaultExecutionDevice.DeviceId,
                ExecutionProvider = modelset.ExecutionProvider ?? settings.DefaultExecutionDevice.Provider,
                Name = modelset.Name,
                PipelineType = modelset.PipelineType,
                SampleSize = modelset.SampleSize,
                BlankTokenId = modelset.TokenizerConfig.BlankTokenId,
                PadTokenId = modelset.TokenizerConfig.PadTokenId,
                TokenizerLimit = modelset.TokenizerConfig.TokenizerLimit,
                Tokenizer2Limit = modelset.Tokenizer2Config?.TokenizerLimit ?? 256,
                TokenizerLength = modelset.TokenizerConfig.TokenizerLength,
                Tokenizer2Length = modelset.Tokenizer2Config?.TokenizerLength ?? 1280,
                Tokenizer3Length = modelset.Tokenizer3Config?.TokenizerLength ?? 4096,
                ModelType = modelset.UnetConfig.ModelType,
                ScaleFactor = modelset.VaeDecoderConfig.ScaleFactor,
                ContextSize = modelset.UnetConfig.ContextSize,
                UnetModel = new ModelFileViewModel
                {
                    OnnxModelPath = modelset.UnetConfig.OnnxModelPath,
                    DeviceId = modelset.UnetConfig.DeviceId ?? modelset.DeviceId,
                    ExecutionProvider = modelset.UnetConfig.ExecutionProvider ?? modelset.ExecutionProvider,
                    IsOverrideEnabled = modelset.UnetConfig.DeviceId.HasValue || modelset.UnetConfig.ExecutionProvider.HasValue
                },

                Unet2Model = modelset.Unet2Config is null ? default : new ModelFileViewModel
                {
                    OnnxModelPath = modelset.Unet2Config.OnnxModelPath,
                    DeviceId = modelset.Unet2Config.DeviceId ?? modelset.DeviceId,
                    ExecutionProvider = modelset.Unet2Config.ExecutionProvider ?? modelset.ExecutionProvider,
                    IsOverrideEnabled = modelset.Unet2Config.DeviceId.HasValue || modelset.Unet2Config.ExecutionProvider.HasValue
                },

                TokenizerModel = new ModelFileViewModel
                {
                    OnnxModelPath = modelset.TokenizerConfig.OnnxModelPath,
                    DeviceId = modelset.TokenizerConfig.DeviceId ?? modelset.DeviceId,
                    ExecutionProvider = modelset.TokenizerConfig.ExecutionProvider ?? modelset.ExecutionProvider,
                    IsOverrideEnabled = modelset.TokenizerConfig.DeviceId.HasValue || modelset.TokenizerConfig.ExecutionProvider.HasValue
                },

                Tokenizer2Model = modelset.Tokenizer2Config is null ? default : new ModelFileViewModel
                {
                    OnnxModelPath = modelset.Tokenizer2Config.OnnxModelPath,
                    DeviceId = modelset.Tokenizer2Config.DeviceId ?? modelset.DeviceId,
                    ExecutionProvider = modelset.Tokenizer2Config.ExecutionProvider ?? modelset.ExecutionProvider,
                    IsOverrideEnabled = modelset.Tokenizer2Config.DeviceId.HasValue || modelset.Tokenizer2Config.ExecutionProvider.HasValue
                },

                Tokenizer3Model = modelset.Tokenizer3Config is null ? default : new ModelFileViewModel
                {
                    OnnxModelPath = modelset.Tokenizer3Config.OnnxModelPath,
                    DeviceId = modelset.Tokenizer3Config.DeviceId ?? modelset.DeviceId,
                    ExecutionProvider = modelset.Tokenizer3Config.ExecutionProvider ?? modelset.ExecutionProvider,
                    IsOverrideEnabled = modelset.Tokenizer3Config.DeviceId.HasValue || modelset.Tokenizer3Config.ExecutionProvider.HasValue
                },

                TextEncoderModel = new ModelFileViewModel
                {
                    OnnxModelPath = modelset.TextEncoderConfig.OnnxModelPath,
                    DeviceId = modelset.TextEncoderConfig.DeviceId ?? modelset.DeviceId,
                    ExecutionProvider = modelset.TextEncoderConfig.ExecutionProvider ?? modelset.ExecutionProvider,
                    IsOverrideEnabled = modelset.TextEncoderConfig.DeviceId.HasValue || modelset.TextEncoderConfig.ExecutionProvider.HasValue
                },

                TextEncoder2Model = modelset.TextEncoder2Config is null ? default : new ModelFileViewModel
                {
                    OnnxModelPath = modelset.TextEncoder2Config.OnnxModelPath,
                    DeviceId = modelset.TextEncoder2Config.DeviceId ?? modelset.DeviceId,
                    ExecutionProvider = modelset.TextEncoder2Config.ExecutionProvider ?? modelset.ExecutionProvider,
                    IsOverrideEnabled = modelset.TextEncoder2Config.DeviceId.HasValue || modelset.TextEncoder2Config.ExecutionProvider.HasValue
                },

                TextEncoder3Model = modelset.TextEncoder3Config is null ? default : new ModelFileViewModel
                {
                    OnnxModelPath = modelset.TextEncoder3Config.OnnxModelPath,
                    DeviceId = modelset.TextEncoder3Config.DeviceId ?? modelset.DeviceId,
                    ExecutionProvider = modelset.TextEncoder3Config.ExecutionProvider ?? modelset.ExecutionProvider,
                    IsOverrideEnabled = modelset.TextEncoder3Config.DeviceId.HasValue || modelset.TextEncoder3Config.ExecutionProvider.HasValue
                },

                VaeDecoderModel = new ModelFileViewModel
                {
                    OnnxModelPath = modelset.VaeDecoderConfig.OnnxModelPath,
                    DeviceId = modelset.VaeDecoderConfig.DeviceId ?? modelset.DeviceId,
                    ExecutionProvider = modelset.VaeDecoderConfig.ExecutionProvider ?? modelset.ExecutionProvider,
                    IsOverrideEnabled = modelset.VaeDecoderConfig.DeviceId.HasValue || modelset.VaeDecoderConfig.ExecutionProvider.HasValue

                },

                VaeEncoderModel = new ModelFileViewModel
                {
                    OnnxModelPath = modelset.VaeEncoderConfig.OnnxModelPath,
                    DeviceId = modelset.VaeEncoderConfig.DeviceId ?? modelset.DeviceId,
                    ExecutionProvider = modelset.VaeEncoderConfig.ExecutionProvider ?? modelset.ExecutionProvider,
                    IsOverrideEnabled = modelset.VaeEncoderConfig.DeviceId.HasValue || modelset.VaeEncoderConfig.ExecutionProvider.HasValue
                },

                ControlNetModel = modelset.ControlNetUnetConfig is null ? default : new ModelFileViewModel
                {
                    OnnxModelPath = modelset.ControlNetUnetConfig.OnnxModelPath,
                    DeviceId = modelset.ControlNetUnetConfig.DeviceId ?? modelset.DeviceId,
                    ExecutionProvider = modelset.ControlNetUnetConfig.ExecutionProvider ?? modelset.ExecutionProvider,
                    IsOverrideEnabled = modelset.ControlNetUnetConfig.DeviceId.HasValue || modelset.ControlNetUnetConfig.ExecutionProvider.HasValue
                },

                FlowEstimationModel = modelset.FlowEstimationConfig is null ? default : new ModelFileViewModel
                {
                    OnnxModelPath = modelset.FlowEstimationConfig.OnnxModelPath,
                    DeviceId = modelset.FlowEstimationConfig.DeviceId ?? modelset.DeviceId,
                    ExecutionProvider = modelset.FlowEstimationConfig.ExecutionProvider ?? modelset.ExecutionProvider,
                    IsOverrideEnabled = modelset.FlowEstimationConfig.DeviceId.HasValue || modelset.FlowEstimationConfig.ExecutionProvider.HasValue
                },

                ResampleModel = modelset.ResampleModelConfig is null ? default : new ModelFileViewModel
                {
                    OnnxModelPath = modelset.ResampleModelConfig.OnnxModelPath,
                    DeviceId = modelset.ResampleModelConfig.DeviceId ?? modelset.DeviceId,
                    ExecutionProvider = modelset.ResampleModelConfig.ExecutionProvider ?? modelset.ExecutionProvider,
                    IsOverrideEnabled = modelset.ResampleModelConfig.DeviceId.HasValue || modelset.ResampleModelConfig.ExecutionProvider.HasValue
                }
            };
        }

        public static StableDiffusionModelSetJson ToModelSet(UpdateModelSetViewModel modelset)
        {
            return new StableDiffusionModelSetJson
            {
                Name = modelset.Name,
                PipelineType = modelset.PipelineType,
                SampleSize = modelset.SampleSize,
                Diffusers = new List<DiffuserType>(modelset.GetDiffusers()),

                DeviceId = modelset.DeviceId,
                ExecutionProvider = modelset.ExecutionProvider,

                UnetConfig = new UNetConditionModelJson
                {
                    ModelType = modelset.ModelType,
                    ContextSize = modelset.ContextSize,
                    OnnxModelPath = modelset.UnetModel.OnnxModelPath,
                    DeviceId = modelset.UnetModel.IsOverrideEnabled && modelset.DeviceId != modelset.UnetModel.DeviceId ? modelset.UnetModel.DeviceId : default,
                    ExecutionProvider = modelset.UnetModel.IsOverrideEnabled && modelset.ExecutionProvider != modelset.UnetModel.ExecutionProvider ? modelset.UnetModel.ExecutionProvider : default,
                },

                Unet2Config = modelset.Unet2Model is null ? default : new UNetConditionModelJson
                {
                    ModelType = modelset.ModelType,
                    OnnxModelPath = modelset.Unet2Model.OnnxModelPath,
                    DeviceId = modelset.Unet2Model.IsOverrideEnabled && modelset.DeviceId != modelset.Unet2Model.DeviceId ? modelset.Unet2Model.DeviceId : default,
                    ExecutionProvider = modelset.Unet2Model.IsOverrideEnabled && modelset.ExecutionProvider != modelset.Unet2Model.ExecutionProvider ? modelset.Unet2Model.ExecutionProvider : default,
                },

                TokenizerConfig = new TokenizerModelJson
                {
                    BlankTokenId = modelset.BlankTokenId,
                    PadTokenId = modelset.PadTokenId,
                    TokenizerLimit = modelset.TokenizerLimit,
                    TokenizerLength = modelset.TokenizerLength,
                    OnnxModelPath = modelset.TokenizerModel.OnnxModelPath,
                    DeviceId = modelset.TokenizerModel.IsOverrideEnabled && modelset.DeviceId != modelset.TokenizerModel.DeviceId ? modelset.TokenizerModel.DeviceId : default,
                    ExecutionProvider = modelset.TokenizerModel.IsOverrideEnabled && modelset.ExecutionProvider != modelset.TokenizerModel.ExecutionProvider ? modelset.TokenizerModel.ExecutionProvider : default,
                },

                Tokenizer2Config = modelset.Tokenizer2Model is null ? default : new TokenizerModelJson
                {
                    BlankTokenId = modelset.BlankTokenId,
                    PadTokenId = modelset.PadTokenId,
                    TokenizerLimit = modelset.Tokenizer2Limit,
                    TokenizerLength = modelset.Tokenizer2Length,
                    OnnxModelPath = modelset.Tokenizer2Model.OnnxModelPath,
                    DeviceId = modelset.Tokenizer2Model.IsOverrideEnabled && modelset.DeviceId != modelset.Tokenizer2Model.DeviceId ? modelset.Tokenizer2Model.DeviceId : default,
                    ExecutionProvider = modelset.Tokenizer2Model.IsOverrideEnabled && modelset.ExecutionProvider != modelset.Tokenizer2Model.ExecutionProvider ? modelset.Tokenizer2Model.ExecutionProvider : default,
                },


                Tokenizer3Config = modelset.Tokenizer3Model is null ? default : new TokenizerModelJson
                {
                    BlankTokenId = modelset.BlankTokenId,
                    PadTokenId = modelset.PadTokenId,
                    TokenizerLimit = modelset.TokenizerLimit,
                    TokenizerLength = modelset.Tokenizer3Length,
                    OnnxModelPath = modelset.Tokenizer3Model.OnnxModelPath,
                    DeviceId = modelset.Tokenizer3Model.IsOverrideEnabled && modelset.DeviceId != modelset.Tokenizer3Model.DeviceId ? modelset.Tokenizer3Model.DeviceId : default,
                    ExecutionProvider = modelset.Tokenizer3Model.IsOverrideEnabled && modelset.ExecutionProvider != modelset.Tokenizer3Model.ExecutionProvider ? modelset.Tokenizer3Model.ExecutionProvider : default,
                },

                TextEncoderConfig = new TextEncoderModelJson
                {
                    OnnxModelPath = modelset.TextEncoderModel.OnnxModelPath,
                    DeviceId = modelset.TextEncoderModel.IsOverrideEnabled && modelset.DeviceId != modelset.TextEncoderModel.DeviceId ? modelset.TextEncoderModel.DeviceId : default,
                    ExecutionProvider = modelset.TextEncoderModel.IsOverrideEnabled && modelset.ExecutionProvider != modelset.TextEncoderModel.ExecutionProvider ? modelset.TextEncoderModel.ExecutionProvider : default,
                },

                TextEncoder2Config = modelset.TextEncoder2Model is null ? default : new TextEncoderModelJson
                {
                    OnnxModelPath = modelset.TextEncoder2Model.OnnxModelPath,
                    DeviceId = modelset.TextEncoder2Model.IsOverrideEnabled && modelset.DeviceId != modelset.TextEncoder2Model.DeviceId ? modelset.TextEncoder2Model.DeviceId : default,
                    ExecutionProvider = modelset.TextEncoder2Model.IsOverrideEnabled && modelset.ExecutionProvider != modelset.TextEncoder2Model.ExecutionProvider ? modelset.TextEncoder2Model.ExecutionProvider : default,
                },

                TextEncoder3Config = modelset.TextEncoder3Model is null ? default : new TextEncoderModelJson
                {
                    OnnxModelPath = modelset.TextEncoder3Model.OnnxModelPath,
                    DeviceId = modelset.TextEncoder3Model.IsOverrideEnabled && modelset.DeviceId != modelset.TextEncoder3Model.DeviceId ? modelset.TextEncoder3Model.DeviceId : default,
                    ExecutionProvider = modelset.TextEncoder3Model.IsOverrideEnabled && modelset.ExecutionProvider != modelset.TextEncoder3Model.ExecutionProvider ? modelset.TextEncoder3Model.ExecutionProvider : default,
                },

                VaeDecoderConfig = new AutoEncoderModelJson
                {
                    ScaleFactor = modelset.ScaleFactor,
                    OnnxModelPath = modelset.VaeDecoderModel.OnnxModelPath,
                    DeviceId = modelset.VaeDecoderModel.IsOverrideEnabled && modelset.DeviceId != modelset.VaeDecoderModel.DeviceId ? modelset.VaeDecoderModel.DeviceId : default,
                    ExecutionProvider = modelset.VaeDecoderModel.IsOverrideEnabled && modelset.ExecutionProvider != modelset.VaeDecoderModel.ExecutionProvider ? modelset.VaeDecoderModel.ExecutionProvider : default
                },

                VaeEncoderConfig = new AutoEncoderModelJson
                {
                    ScaleFactor = modelset.ScaleFactor,
                    OnnxModelPath = modelset.VaeEncoderModel.OnnxModelPath,
                    DeviceId = modelset.VaeEncoderModel.IsOverrideEnabled && modelset.DeviceId != modelset.VaeEncoderModel.DeviceId ? modelset.VaeEncoderModel.DeviceId : default,
                    ExecutionProvider = modelset.VaeEncoderModel.IsOverrideEnabled && modelset.ExecutionProvider != modelset.VaeEncoderModel.ExecutionProvider ? modelset.VaeEncoderModel.ExecutionProvider : default
                },

                ControlNetUnetConfig = modelset.ControlNetModel is null ? default : new UNetConditionModelJson
                {
                    ModelType = modelset.ModelType,
                    OnnxModelPath = modelset.ControlNetModel.OnnxModelPath,
                    DeviceId = modelset.ControlNetModel.IsOverrideEnabled && modelset.DeviceId != modelset.ControlNetModel.DeviceId ? modelset.ControlNetModel.DeviceId : default,
                    ExecutionProvider = modelset.ControlNetModel.IsOverrideEnabled && modelset.ExecutionProvider != modelset.ControlNetModel.ExecutionProvider ? modelset.ControlNetModel.ExecutionProvider : default
                },

                FlowEstimationConfig = modelset.FlowEstimationModel is null ? default : new FlowEstimationModelJson
                {
                    OnnxModelPath = modelset.FlowEstimationModel.OnnxModelPath,
                    DeviceId = modelset.FlowEstimationModel.IsOverrideEnabled && modelset.DeviceId != modelset.FlowEstimationModel.DeviceId ? modelset.FlowEstimationModel.DeviceId : default,
                    ExecutionProvider = modelset.FlowEstimationModel.IsOverrideEnabled && modelset.ExecutionProvider != modelset.FlowEstimationModel.ExecutionProvider ? modelset.FlowEstimationModel.ExecutionProvider : default
                },

                ResampleModelConfig = modelset.ResampleModel is null ? default : new ResampleModelJson
                {
                    OnnxModelPath = modelset.ResampleModel.OnnxModelPath,
                    DeviceId = modelset.ResampleModel.IsOverrideEnabled && modelset.DeviceId != modelset.ResampleModel.DeviceId ? modelset.ResampleModel.DeviceId : default,
                    ExecutionProvider = modelset.ResampleModel.IsOverrideEnabled && modelset.ExecutionProvider != modelset.ResampleModel.ExecutionProvider ? modelset.ResampleModel.ExecutionProvider : default
                },
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
