using Amuse.UI.Enums;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amuse.UI.Models.FeatureExtractor
{
    public class UpdateFeatureExtractorModelSetViewModel : INotifyPropertyChanged
    {
        private string _name;
        private int _deviceId;
        private ExecutionProvider _executionProvider;
        private string _modelFile;
        private FeatureExtractorModelTemplate _modelTemplate;

        public string Name
        {
            get { return _name; }
            set { _name = value; NotifyPropertyChanged(); }
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


        public string ModelFile
        {
            get { return _modelFile; }
            set { _modelFile = value; NotifyPropertyChanged(); }
        }

        public FeatureExtractorModelTemplate ModelTemplate
        {
            get { return _modelTemplate; }
            set { _modelTemplate = value; NotifyPropertyChanged(); }
        }


        public static UpdateFeatureExtractorModelSetViewModel FromModelSet(FeatureExtractorModelJson modelset)
        {
            return new UpdateFeatureExtractorModelSetViewModel
            {
                Name = modelset.Name,
                DeviceId = modelset.DeviceId,
                ExecutionProvider = modelset.ExecutionProvider,
                ModelFile = modelset.OnnxModelPath,
                ModelTemplate = new FeatureExtractorModelTemplate
                {
                    SampleSize = modelset.SampleSize,
                    OutputChannels = modelset.OutputChannels,
                    InputResizeMode = modelset.InputResizeMode,
                    NormalizeType = modelset.NormalizeType,
                    NormalizeOutputType = modelset.NormalizeOutputType,
                    SetOutputToInputAlpha = modelset.SetOutputToInputAlpha,
                    InvertOutput = modelset.InvertOutput,
                }
            };
        }

        public static FeatureExtractorModelJson ToModelSet(UpdateFeatureExtractorModelSetViewModel viewModel)
        {
            return new FeatureExtractorModelJson
            {
                Name = viewModel.Name,
                DeviceId = viewModel.DeviceId,
                ExecutionProvider = viewModel.ExecutionProvider,
                OutputChannels = viewModel.ModelTemplate.OutputChannels,
                SampleSize = viewModel.ModelTemplate.SampleSize,
                SetOutputToInputAlpha = viewModel.ModelTemplate.SetOutputToInputAlpha,
                InvertOutput = viewModel.ModelTemplate.InvertOutput,
                NormalizeType = viewModel.ModelTemplate.NormalizeType,
                NormalizeOutputType = viewModel.ModelTemplate.NormalizeOutputType,
                InputResizeMode = viewModel.ModelTemplate.InputResizeMode,
                OnnxModelPath = viewModel.ModelFile
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
