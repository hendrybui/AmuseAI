using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Amuse.UI.UserControls
{
    public partial class SliderElement : UserControl, INotifyPropertyChanged
    {
        private string _valueText;
        private string _valueFormat;

        /// <summary>Initializes a new instance of the <see cref="SliderElement" /> class.</summary>
        public SliderElement()
        {
            InitializeComponent();
            SetCurrentValue(FontSizeProperty, 11.0);
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(SliderElement));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(SliderElement), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));



        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(SliderElement));
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(SliderElement));
        public static readonly DependencyProperty TickFrequencyProperty = DependencyProperty.Register(nameof(TickFrequency), typeof(double), typeof(SliderElement));
        public static readonly DependencyProperty ValueFontSizeProperty = DependencyProperty.Register(nameof(ValueFontSize), typeof(double), typeof(SliderElement), new PropertyMetadata(9.0));
           public event RoutedPropertyChangedEventHandler<double> ValueChanged;

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public double TickFrequency
        {
            get { return (double)GetValue(TickFrequencyProperty); }
            set { SetValue(TickFrequencyProperty, value); }
        }

        public double ValueFontSize
        {
            get { return (double)GetValue(ValueFontSizeProperty); }
            set { SetValue(ValueFontSizeProperty, value); }
        }

        public string ValueText
        {
            get { return _valueText; }
            set { _valueText = value; NotifyPropertyChanged(); }
        }

        public string ValueFormat
        {
            get { return _valueFormat; }
            set { _valueFormat = value; NotifyPropertyChanged(); UpdateValueText(); }
        }


        private void UpdateValueText()
        {
            if (string.IsNullOrEmpty(_valueFormat))
            {
                ValueText = Value.ToString();
                return;
            }

            ValueText = Value.ToString(_valueFormat);
        }


        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SliderElement sliderElement)
                sliderElement.UpdateValueText();
        }


        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateValueText();
            ValueChanged?.Invoke(this, e);
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
