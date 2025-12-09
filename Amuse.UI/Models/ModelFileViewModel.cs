using Amuse.UI.Enums;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amuse.UI.Models
{
    public class ModelFileViewModel : INotifyPropertyChanged
    {
        private string _onnnxModelPath;
        private int? _deviceId;
        private ExecutionProvider? _executionProvider;
        private bool _isOverrideEnabled;
        private bool _hasChanged;
        private int _requiredMemory;

        public string OnnxModelPath
        {
            get { return _onnnxModelPath; }
            set { _onnnxModelPath = value; NotifyPropertyChanged(); }
        }

        public int? DeviceId
        {
            get { return _deviceId; }
            set { _deviceId = value; NotifyPropertyChanged(); }
        }

        public ExecutionProvider? ExecutionProvider
        {
            get { return _executionProvider; }
            set { _executionProvider = value; NotifyPropertyChanged(); }
        }

        public int RequiredMemory
        {
            get { return _requiredMemory; }
            set { _requiredMemory = value; NotifyPropertyChanged(); }
        }

        public bool IsOverrideEnabled
        {
            get { return _isOverrideEnabled; }
            set { _isOverrideEnabled = value; NotifyPropertyChanged(); }
        }

        public bool HasChanged
        {
            get { return _hasChanged; }
            set { _hasChanged = value; NotifyPropertyChanged(); }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string property = "")
        {
            if (!property.Equals(nameof(HasChanged)) && !HasChanged)
                HasChanged = true;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion
    }

}
