using OnnxStack.Core.Image;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amuse.UI.Models.FeatureExtractor
{
    public record FeatureExtractorModelTemplate : INotifyPropertyChanged
    {
        private int _sampleSize = 512;
        private int _outputChannels = 1;
        private bool _setOutputToInputAlpha;
        private bool _invertOutput;
        private ImageResizeMode _inputResizeMode;
        private ImageNormalizeType _normalizeType;
        private ImageNormalizeType _normalizeOutputType;
        private bool _isControlNetSupported;

        public int SampleSize
        {
            get { return _sampleSize; }
            set { _sampleSize = value; NotifyPropertyChanged(); }
        }

        public int OutputChannels
        {
            get { return _outputChannels; }
            set { _outputChannels = value; }
        }

        public bool SetOutputToInputAlpha
        {
            get { return _setOutputToInputAlpha; }
            set { _setOutputToInputAlpha = value; NotifyPropertyChanged(); }
        }

        public bool InvertOutput
        {
            get { return _invertOutput; }
            set { _invertOutput = value; NotifyPropertyChanged(); }
        }

        public ImageResizeMode InputResizeMode
        {
            get { return _inputResizeMode; }
            set { _inputResizeMode = value; NotifyPropertyChanged(); }
        }

        public ImageNormalizeType NormalizeType
        {
            get { return _normalizeType; }
            set { _normalizeType = value; NotifyPropertyChanged(); }
        }

        public ImageNormalizeType NormalizeOutputType
        {
            get { return _normalizeOutputType; }
            set { _normalizeOutputType = value; NotifyPropertyChanged(); }
        }

        public bool IsControlNetSupported
        {
            get { return _isControlNetSupported; }
            set { _isControlNetSupported = value; NotifyPropertyChanged(); }
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
