using OnnxStack.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Amuse.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for TextInputDialog.xaml
    /// </summary>
    public partial class TextInputDialog : BaseDialog
    {
        private int _minLength;
        private int _maxLength;
        private string _fieldName;
        private string _textResult;
        private string _errorMessage;
        private List<string> _invalidOptions;

        public TextInputDialog()
        {
            InitializeComponent();
            ErrorMessage = string.Empty;
        }

        public string TextResult
        {
            get { return _textResult; }
            set { _textResult = value; NotifyPropertyChanged(); ErrorMessage = string.Empty; }
        }

        public List<string> InvalidOptions
        {
            get { return _invalidOptions; }
            set { _invalidOptions = value; NotifyPropertyChanged(); }
        }

        public int MinLength
        {
            get { return _minLength; }
            set { _minLength = value; NotifyPropertyChanged(); }
        }

        public int MaxLength
        {
            get { return _maxLength; }
            set { _maxLength = value; NotifyPropertyChanged(); }
        }

        public string FieldName
        {
            get { return _fieldName; }
            set { _fieldName = value; NotifyPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; NotifyPropertyChanged(); }
        }


        public Task<bool> ShowDialogAsync(string title, string fieldName, int minLength = 0, int maxLength = 256, List<string> invalidOptions = null)
        {
            Title = title;
            FieldName = fieldName ?? "Text";
            MinLength = minLength;
            MaxLength = maxLength;
            InvalidOptions = invalidOptions;
            return base.ShowDialogAsync();
        }


        protected override Task SaveAsync()
        {
            var result = TextResult.Trim();
            if (!InvalidOptions.IsNullOrEmpty() && InvalidOptions.Contains(result))
            {
                ErrorMessage = $"{result} is an invalid option";
                return Task.CompletedTask;
            }

            _textResult = result;
            return base.SaveAsync();
        }


        protected override bool CanExecuteSave()
        {
            var result = TextResult?.Trim() ?? string.Empty;
            return result.Length > MinLength && result.Length <= MaxLength;
        }

    }
}
