using OnnxStack.Core.Image;
using OnnxStack.StableDiffusion.Config;
using OnnxStack.StableDiffusion.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Amuse.UI.Models.StableDiffusion
{
    public class PromptOptionsModel : INotifyPropertyChanged
    {
        private string _prompt;
        private string _negativePrompt;
        private float _videoInputFPS = 24;
        private float _videoOutputFPS = 24;
        private bool _hasChanged;
        private ImageBlendingMode _frameBlendingMode = ImageBlendingMode.Overlay;
        private float _previousFrameStrength = 0.3f;
        private float _frameStrength = 0.9f;
        private bool _isFrameBlendEnabled;
        private OptimizationType _optimizationType = OptimizationType.Level1;
        private List<string> _prompts = new List<string>();

        private int _motionFrames = 16;
        private int _motionStrides = 0;
        private int _motionContextOverlap = 3;
        private int _motionNoiseContext = 16;
        private bool _frameResample;
        private int _frameUpSample = 2;
        private int _frameDownSample = 0;

        private int _decoderTileOverlap = 8;
        private TileMode _decoderTileMode = TileMode.ClipBlend;


        [Required]
        [StringLength(512, MinimumLength = 1)]
        public string Prompt
        {
            get { return _prompt; }
            set
            {
                _prompt = value;
                UpdatePromptsList();
                NotifyPropertyChanged();
            }
        }

        [StringLength(512)]
        public string NegativePrompt
        {
            get { return _negativePrompt; }
            set { _negativePrompt = value; NotifyPropertyChanged(); }
        }

        public List<string> Prompts
        {
            get { return _prompts; }
            set { _prompts = value; NotifyPropertyChanged(); }
        }

        public bool HasChanged
        {
            get { return _hasChanged; }
            set { _hasChanged = value; NotifyPropertyChanged(); }
        }

        public float VideoInputFPS
        {
            get { return _videoInputFPS; }
            set { _videoInputFPS = value; NotifyPropertyChanged(); }
        }

        public float VideoOutputFPS
        {
            get { return _videoOutputFPS; }
            set { _videoOutputFPS = value; NotifyPropertyChanged(); }
        }

        public bool IsFrameBlendEnabled
        {
            get { return _isFrameBlendEnabled; }
            set { _isFrameBlendEnabled = value; NotifyPropertyChanged(); }
        }

        public float FrameStrength
        {
            get { return _frameStrength; }
            set { _frameStrength = value; NotifyPropertyChanged(); }
        }

        public float PreviousFrameStrength
        {
            get { return _previousFrameStrength; }
            set { _previousFrameStrength = value; NotifyPropertyChanged(); }
        }

        public ImageBlendingMode FrameBlendingMode
        {
            get { return _frameBlendingMode; }
            set { _frameBlendingMode = value; }
        }

        public OptimizationType OptimizationType
        {
            get { return _optimizationType; }
            set { _optimizationType = value; NotifyPropertyChanged(); }
        }

        public int MotionFrames
        {
            get { return _motionFrames; }
            set { _motionFrames = value; NotifyPropertyChanged(); }
        }

        public int MotionStrides
        {
            get { return _motionStrides; }
            set { _motionStrides = value; NotifyPropertyChanged(); }
        }

        public int MotionContextOverlap
        {
            get { return _motionContextOverlap; }
            set { _motionContextOverlap = value; NotifyPropertyChanged(); }
        }

        public int MotionNoiseContext
        {
            get { return _motionNoiseContext; }
            set { _motionNoiseContext = value; NotifyPropertyChanged(); }
        }

        public bool FrameResample
        {
            get { return _frameResample; }
            set { _frameResample = value; NotifyPropertyChanged(); }
        }

        public int FrameUpSample
        {
            get { return _frameUpSample; }
            set { _frameUpSample = value; NotifyPropertyChanged(); }
        }

        public int FrameDownSample
        {
            get { return _frameDownSample; }
            set { _frameDownSample = value; NotifyPropertyChanged(); }
        }

        public TileMode DecoderTileMode
        {
            get { return _decoderTileMode; }
            set { _decoderTileMode = value; NotifyPropertyChanged(); }
        }

        public int DecoderTileOverlap
        {
            get { return _decoderTileOverlap; }
            set { _decoderTileOverlap = value; NotifyPropertyChanged(); }
        }


        private void UpdatePromptsList()
        {
            _prompts.Clear();
            if (string.IsNullOrEmpty(_prompt))
                return;

            _prompts = _prompt
                .Split('|', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries)
                .ToList();
        }


        public static PromptOptionsModel FromGenerateOptions(GenerateOptions promptOptions)
        {
            return new PromptOptionsModel
            {
                Prompt = promptOptions.Prompt,
                NegativePrompt = promptOptions.NegativePrompt,
                OptimizationType = promptOptions.OptimizationType,
                DecoderTileMode = promptOptions.AutoEncoderTileMode,
                DecoderTileOverlap = promptOptions.AutoEncoderTileOverlap
            };
        }

        public static PromptOptionsModel FromGenerateVideoOptions(GenerateOptions promptOptions)
        {
            return new PromptOptionsModel
            {
                Prompt = promptOptions.Prompt,
                NegativePrompt = promptOptions.NegativePrompt,
                FrameBlendingMode = promptOptions.FrameBlendingMode,
                FrameStrength = promptOptions.FrameStrength,
                IsFrameBlendEnabled = promptOptions.IsFrameBlendEnabled,
                PreviousFrameStrength = promptOptions.PreviousFrameStrength,
                OptimizationType = promptOptions.OptimizationType,
                DecoderTileMode = promptOptions.AutoEncoderTileMode,
                DecoderTileOverlap = promptOptions.AutoEncoderTileOverlap,
                MotionStrides = promptOptions.MotionStrides,
                MotionNoiseContext = promptOptions.MotionNoiseContext,
                MotionContextOverlap = promptOptions.MotionContextOverlap,
                MotionFrames = promptOptions.MotionFrames,
            };
        }


        public static GenerateOptions ToGenerateOptions(PromptOptionsModel promptOptions, SchedulerOptionsModel schedulerOptions, MemoryInfoModel memoryInfo)
        {
            return new GenerateOptions
            {
                Prompt = string.IsNullOrEmpty(promptOptions.Prompt) ? " " : promptOptions.Prompt,
                NegativePrompt = promptOptions.NegativePrompt,
                OptimizationType = promptOptions.OptimizationType,
                AutoEncoderTileMode = promptOptions.DecoderTileMode,
                AutoEncoderTileOverlap = promptOptions.DecoderTileOverlap,
                FrameDownSample = promptOptions.FrameDownSample,
                FrameResample = promptOptions.FrameResample,
                FrameUpSample = promptOptions.FrameUpSample,
                MotionContextOverlap = promptOptions.MotionContextOverlap,
                MotionFrames = promptOptions.MotionFrames,
                MotionNoiseContext = promptOptions.MotionNoiseContext,
                MotionStrides = promptOptions.MotionStrides,

                IsLowMemoryComputeEnabled = memoryInfo.IsLowMemoryComputeEnabled,
                IsLowMemoryEncoderEnabled = memoryInfo.IsLowMemoryEncoderEnabled,
                IsLowMemoryDecoderEnabled = memoryInfo.IsLowMemoryDecoderEnabled,
                IsLowMemoryTextEncoderEnabled = memoryInfo.IsLowMemoryTextEncoderEnabled,
                IsAutoEncoderTileEnabled = memoryInfo.IsVaeTileEnabled,

                SchedulerOptions = SchedulerOptionsModel.ToSchedulerOptions(schedulerOptions)
            };
        }


        public static GenerateOptions ToGenerateVideoOptions(PromptOptionsModel promptOptions, SchedulerOptionsModel schedulerOptions, MemoryInfoModel memoryInfo)
        {
            return new GenerateOptions
            {
                Prompt = string.IsNullOrEmpty(promptOptions.Prompt) ? " " : promptOptions.Prompt,
                NegativePrompt = promptOptions.NegativePrompt,
                PreviousFrameStrength = promptOptions.PreviousFrameStrength,
                IsFrameBlendEnabled = promptOptions.IsFrameBlendEnabled,
                FrameStrength = promptOptions.FrameStrength,
                FrameBlendingMode = promptOptions.FrameBlendingMode,
                OptimizationType = promptOptions.OptimizationType,
                Prompts = promptOptions.Prompts,
                InputFrameRate = promptOptions.VideoInputFPS,
                OutputFrameRate = promptOptions.VideoOutputFPS,
                AutoEncoderTileMode = promptOptions.DecoderTileMode,
                AutoEncoderTileOverlap = promptOptions.DecoderTileOverlap,
                FrameDownSample = promptOptions.FrameDownSample,
                FrameResample = promptOptions.FrameResample,
                FrameUpSample = promptOptions.FrameUpSample,
                MotionContextOverlap = promptOptions.MotionContextOverlap,
                MotionFrames = promptOptions.MotionFrames,
                MotionNoiseContext = promptOptions.MotionNoiseContext,
                MotionStrides = promptOptions.MotionStrides,

                IsLowMemoryComputeEnabled = memoryInfo.IsLowMemoryComputeEnabled,
                IsLowMemoryEncoderEnabled = memoryInfo.IsLowMemoryEncoderEnabled,
                IsLowMemoryDecoderEnabled = memoryInfo.IsLowMemoryDecoderEnabled,
                IsLowMemoryTextEncoderEnabled = memoryInfo.IsLowMemoryTextEncoderEnabled,
                IsAutoEncoderTileEnabled = memoryInfo.IsVaeTileEnabled,

                SchedulerOptions = SchedulerOptionsModel.ToSchedulerOptions(schedulerOptions)
            };
        }


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string property = "")
        {
            if (!property.Equals(nameof(HasChanged)) && !HasChanged)
                HasChanged = true;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion

    }
}
