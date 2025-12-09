using Amuse.UI.Enums;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amuse.UI.Models.StableDiffusion
{
    public class UpdateControlNetModelSetViewModel : INotifyPropertyChanged
    {
        private string _name;
        private int _deviceId;
        private ExecutionProvider _executionProvider;
        private string _modelFile;
        private ControlNetModelTemplate _modelTemplate;

        public string Name
        {
            get { return _name; }
            set { _name = value; NotifyPropertyChanged(); }
        }

        public string ModelFile
        {
            get { return _modelFile; }
            set { _modelFile = value; NotifyPropertyChanged(); }
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

        public ControlNetModelTemplate ModelTemplate
        {
            get { return _modelTemplate; }
            set { _modelTemplate = value; NotifyPropertyChanged(); }
        }


        public static UpdateControlNetModelSetViewModel FromModelSet(ControlNetModelJson modelset)
        {
            return new UpdateControlNetModelSetViewModel
            {
                Name = modelset.Name,
                DeviceId = modelset.DeviceId,
                ExecutionProvider = modelset.ExecutionProvider,
                ModelFile = modelset.OnnxModelPath,
                ModelTemplate = new ControlNetModelTemplate
                {
                    InvertInput = modelset.InvertInput,
                    LayerCount = modelset.LayerCount,
                    DisablePooledProjection = modelset.DisablePooledProjection
                }
            };
        }

        public static ControlNetModelJson ToModelSet(UpdateControlNetModelSetViewModel modelset)
        {
            return new ControlNetModelJson
            {
                Name = modelset.Name,
                DeviceId = modelset.DeviceId,
                ExecutionProvider = modelset.ExecutionProvider,
                OnnxModelPath = modelset.ModelFile,
                InvertInput = modelset.ModelTemplate.InvertInput,
                LayerCount = modelset.ModelTemplate.LayerCount,
                DisablePooledProjection = modelset.ModelTemplate.DisablePooledProjection
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
