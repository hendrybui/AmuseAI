using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amuse.UI.Models
{
    public class ProgressInfo : INotifyPropertyChanged
    {
        private int _value;
        private int _maximum = 1;
        private string _message;

        public int Value
        {
            get { return _value; }
            set { _value = value; NotifyPropertyChanged(); }
        }

        public int Maximum
        {
            get { return _maximum; }
            set { _maximum = value; NotifyPropertyChanged(); }
        }

        public string Message
        {
            get { return _message; }
            set { _message = value; NotifyPropertyChanged(); }
        }


        public void Update(string message = default)
        {
            Message = message;
        }


        public void Update(int value, int maximum, string message = default)
        {
            Value = value;
            Message = message;
            Maximum = Math.Max(1, maximum);
        }


        public void Indeterminate(string message = default)
        {
            Value = 0;
            Maximum = -1;
            Message = message;
        }


        public void Clear()
        {
            Value = 0;
            Maximum = 1;
            Message = null;
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
