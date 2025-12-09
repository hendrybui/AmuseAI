using OnnxStack.StableDiffusion.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Amuse.UI.Models.StableDiffusion
{
    public class ControlNetModelSetViewModel : INotifyPropertyChanged
    {
        private bool _isLoaded;
        private bool _isLoading;
        private string _variant;
        private ModelTemplateViewModel _template;
        public Guid Id { get; set; }

        public ControlNetModelJson ModelSet { get; set; }
        public List<PipelineType> PipelineTypes { get; set; }

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
