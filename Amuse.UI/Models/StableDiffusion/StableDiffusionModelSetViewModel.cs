using OnnxStack.Core;
using OnnxStack.StableDiffusion.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Amuse.UI.Models.StableDiffusion
{
    public class StableDiffusionModelSetViewModel : INotifyPropertyChanged
    {
        private bool _isLoaded;
        private bool _isLoading;
        private string _variant;
        private StableDiffusionModelSetJson _modelSet;
        private ModelTemplateViewModel _template;

        public Guid Id { get; set; }

        public StableDiffusionModelSetJson ModelSet
        {
            get { return _modelSet; }
            set { _modelSet = value; NotifyPropertyChanged(); }
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
        public ModelTemplateViewModel Template
        {
            get { return _template; }
            set { _template = value; NotifyPropertyChanged(); }
        }

        [JsonIgnore]
        public bool IsControlNet => ModelSet.Diffusers.Contains(DiffuserType.ControlNet) || ModelSet.Diffusers.Contains(DiffuserType.ControlNetImage);

        [JsonIgnore]
        public bool IsVideo => ModelSet.Diffusers.Contains(DiffuserType.TextToVideo) || ModelSet.Diffusers.Contains(DiffuserType.ImageToVideo) || ModelSet.Diffusers.Contains(DiffuserType.VideoToVideo);

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Variant
        {
            get { return _variant; }
            set { _variant = value; NotifyPropertyChanged(); }
        }

        [JsonIgnore]
        public IReadOnlyList<string> Variants => Template.Variants;

        [JsonIgnore]
        public bool HasVariants => !Template.Variants.IsNullOrEmpty();


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion
    }
}
