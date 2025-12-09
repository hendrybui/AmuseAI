using OnnxStack.StableDiffusion.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amuse.UI.Models.StableDiffusion
{
    public record ControlNetModelTemplate : INotifyPropertyChanged
    {
        private bool _invertInput;
        private List<PipelineType> _pipelineTypes;
        private int _layerCount;
        private bool _disablePooledProjection;

        public List<PipelineType> PipelineTypes
        {
            get { return _pipelineTypes; }
            set { _pipelineTypes = value; NotifyPropertyChanged(); }
        }

        public bool InvertInput
        {
            get { return _invertInput; }
            set { _invertInput = value; NotifyPropertyChanged(); }
        }

        public int LayerCount
        {
            get { return _layerCount; }
            set { _layerCount = value; NotifyPropertyChanged(); }
        }
        public bool DisablePooledProjection
        {
            get { return _disablePooledProjection; }
            set { _disablePooledProjection = value; NotifyPropertyChanged(); }
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
