using Amuse.UI.Enums;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amuse.UI.Models.Upscale
{
    public class UpdateUpscaleModelSetViewModel : INotifyPropertyChanged
    {
        private string _name;
        private int _deviceId;
        private ExecutionProvider _executionProvider;
        private string _modelFile;
        private UpscaleModelTemplate _modelTemplate;

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

        public UpscaleModelTemplate ModelTemplate
        {
            get { return _modelTemplate; }
            set { _modelTemplate = value; NotifyPropertyChanged(); }
        }


        public static UpdateUpscaleModelSetViewModel FromModelSet(UpscaleModelJson modelset)
        {
            return new UpdateUpscaleModelSetViewModel
            {
                Name = modelset.Name,
                DeviceId = modelset.DeviceId,
                ExecutionProvider = modelset.ExecutionProvider,
                ModelFile = modelset.OnnxModelPath,
                ModelTemplate = new UpscaleModelTemplate
                {
                    SampleSize = modelset.SampleSize,
                    ScaleFactor = modelset.ScaleFactor,
                    Channels = modelset.Channels,
                    NormalizeType = modelset.NormalizeType,
                    TileMode = modelset.TileMode,
                    TileSize = modelset.TileSize,
                    TileOverlap = modelset.TileOverlap
                }
            };
        }

        public static UpscaleModelJson ToModelSet(UpdateUpscaleModelSetViewModel viewModel)
        {
            return new UpscaleModelJson
            {
                Name = viewModel.Name,
                DeviceId = viewModel.DeviceId,
                ExecutionProvider = viewModel.ExecutionProvider,
                OnnxModelPath = viewModel.ModelFile,
                Channels = viewModel.ModelTemplate.Channels,
                ScaleFactor = viewModel.ModelTemplate.ScaleFactor,
                SampleSize = viewModel.ModelTemplate.SampleSize,
                NormalizeType = viewModel.ModelTemplate.NormalizeType,
                TileMode = viewModel.ModelTemplate.TileMode,
                TileSize = viewModel.ModelTemplate.TileSize,
                TileOverlap = viewModel.ModelTemplate.TileOverlap
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
