using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Amuse.UI.Models.FeatureExtractor
{
    public class FeatureExtractorModelSetViewModel : INotifyPropertyChanged
    {
        private bool _isControlNetSupported;
        private bool _isLoaded;
        private bool _isLoading;
        private string _variant;
        private ModelTemplateViewModel _template;

        public Guid Id { get; set; }
        public FeatureExtractorModelJson ModelSet { get; set; }

        public bool IsControlNetSupported
        {
            get { return _isControlNetSupported; }
            set { _isControlNetSupported = value; NotifyPropertyChanged(); }
        }

        [JsonIgnore]
        public string Name
        {
            get { return ModelSet?.Name; }
        }

        [JsonIgnore]
        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { _isLoaded = value; NotifyPropertyChanged(); }
        }

        [JsonIgnore]
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; NotifyPropertyChanged(); }
        }

        [JsonIgnore]
        public string Variant
        {
            get { return _variant; }
            set { _variant = value; NotifyPropertyChanged(); }
        }

        [JsonIgnore]
        public ModelTemplateViewModel Template
        {
            get { return _template; }
            set { _template = value; NotifyPropertyChanged(); }
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
