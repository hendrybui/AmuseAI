using Amuse.UI.Helpers;
using OnnxStack.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Amuse.UI.UserControls
{
    /// <summary>
    /// Interaction logic for VariantPicker.xaml
    /// </summary>
    public partial class VariantPicker : UserControl, INotifyPropertyChanged
    {
        private string _variantOption;
        private List<string> _variantOptions;

        public VariantPicker()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty VariantsProperty =
           DependencyProperty.Register(nameof(Variants), typeof(List<string>), typeof(VariantPicker), new PropertyMetadata<VariantPicker, List<string>>((c, v) => c.OnValueChanged(v)));

        public static readonly DependencyProperty SelectedVariantProperty =
            DependencyProperty.Register(nameof(SelectedVariant), typeof(string), typeof(VariantPicker), new PropertyMetadata<VariantPicker, string>((c, v) => c.OnVariantChanged(v)));

        public List<string> Variants
        {
            get { return (List<string>)GetValue(VariantsProperty); }
            set { SetValue(VariantsProperty, value); }
        }

        public string SelectedVariant
        {
            get { return (string)GetValue(SelectedVariantProperty); }
            set { SetValue(SelectedVariantProperty, value); }
        }

        public List<string> VariantOptions
        {
            get { return _variantOptions; }
            set { _variantOptions = value; NotifyPropertyChanged(); }
        }

        public string VariantOption
        {
            get { return _variantOption; }
            set
            {
                _variantOption = value;
                SelectedVariant = _variantOption == "Default" ? null : _variantOption;
                NotifyPropertyChanged();
            }
        }


        private Task OnValueChanged(List<string> variants)
        {
            if (variants.IsNullOrEmpty())
            {
                VariantOptions = ["Default"];
                return Task.CompletedTask;
            }

            VariantOptions = ["Default", .. variants];
            if (string.IsNullOrEmpty(_variantOption))
                VariantOption = VariantOptions[0];
            return Task.CompletedTask;
        }


        private Task OnVariantChanged(string variant)
        {
            if (variant != _variantOption)
            {
                _variantOption = variant == null ? "Default" : variant;
                NotifyPropertyChanged(nameof(VariantOption));
            }
            return Task.CompletedTask;
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
