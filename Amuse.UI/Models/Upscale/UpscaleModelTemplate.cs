using OnnxStack.Core.Image;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amuse.UI.Models.Upscale
{
    public record UpscaleModelTemplate : INotifyPropertyChanged
    {
        private int _scaleFactor = 1;
        private int _sampleSize = 0;
        private int _channels = 3;
        private ImageNormalizeType _normalizeType;
        private TileMode _tileMode =  TileMode.ClipBlend;
        private int _tileSize = 256;
        private int _tileOverlap = 8;

        public int ScaleFactor
        {
            get { return _scaleFactor; }
            set { _scaleFactor = value; NotifyPropertyChanged(); }
        }

        public int SampleSize
        {
            get { return _sampleSize; }
            set { _sampleSize = value; NotifyPropertyChanged(); }
        }

        public int Channels
        {
            get { return _channels; }
            set { _channels = value; }
        }

        public ImageNormalizeType NormalizeType
        {
            get { return _normalizeType; }
            set { _normalizeType = value; NotifyPropertyChanged(); }
        }

        public TileMode TileMode
        {
            get { return _tileMode; }
            set { _tileMode = value; NotifyPropertyChanged(); }
        }

        public int TileSize
        {
            get { return _tileSize; }
            set { _tileSize = value; NotifyPropertyChanged(); }
        }

        public int TileOverlap
        {
            get { return _tileOverlap; }
            set { _tileOverlap = value; NotifyPropertyChanged(); }
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
