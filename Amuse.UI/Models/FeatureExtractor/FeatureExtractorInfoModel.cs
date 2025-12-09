using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amuse.UI.Models.FeatureExtractor
{
    public record FeatureExtractorInfoModel : INotifyPropertyChanged
    {
        private string _name;
        private int _sampleSize;
        private int _channels;

        public string Name
        {
            get { return _name; }
            set { _name = value; NotifyPropertyChanged(); }
        }

        public int SampleSize
        {
            get { return _sampleSize; }
            set { _sampleSize = value; NotifyPropertyChanged();  }
        }

        public int Channels
        {
            get { return _channels; }
            set { _channels = value; NotifyPropertyChanged(); }
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
