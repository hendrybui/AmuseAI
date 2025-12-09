using Amuse.UI.Helpers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Amuse.UI.UserControls
{
    /// <summary>
    /// Interaction logic for FloatBox.xaml
    /// </summary>
    public partial class FloatBox : UserControl
    {
        public FloatBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty FloatValueProperty =
           DependencyProperty.Register(nameof(FloatValue), typeof(float), typeof(FloatBox), new PropertyMetadata<FloatBox>((c) => c.OnValueChanged()) { BindsTwoWayByDefault = true });

        public static readonly DependencyProperty TextValueProperty =
            DependencyProperty.Register(nameof(TextValue), typeof(string), typeof(FloatBox), new PropertyMetadata<FloatBox>((c) => c.OnTextValueChanged()) { DefaultValue = "0" });

        public static readonly DependencyProperty IsTextInvalidProperty =
            DependencyProperty.Register(nameof(IsTextInvalid), typeof(bool), typeof(FloatBox));

        public static readonly DependencyProperty TruncateProperty =
            DependencyProperty.Register(nameof(Truncate), typeof(int), typeof(FloatBox));

        public float FloatValue
        {
            get { return (float)GetValue(FloatValueProperty); }
            set { SetValue(FloatValueProperty, value); }
        }

        public string TextValue
        {
            get { return (string)GetValue(TextValueProperty); }
            set { SetValue(TextValueProperty, value); }
        }

        public bool IsTextInvalid
        {
            get { return (bool)GetValue(IsTextInvalidProperty); }
            set { SetValue(IsTextInvalidProperty, value); }
        }

        public int Truncate
        {
            get { return (int)GetValue(TruncateProperty); }
            set { SetValue(TruncateProperty, value); }
        }


        private Task OnValueChanged()
        {
            SetTextValue();
            return Task.CompletedTask;
        }


        private Task OnTextValueChanged()
        {
            if (float.TryParse(TextValue, out float value))
            {
                FloatValue = value;
                IsTextInvalid = false;
                return Task.CompletedTask;
            }

            IsTextInvalid = true;
            return Task.CompletedTask;
        }


        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            SetTextValue();
            base.OnLostKeyboardFocus(e);
        }


        private void SetTextValue()
        {
            if (TextBoxControl.IsKeyboardFocused)
            {
                TextValue = FloatValue.ToString();
            }
            else
            {
                TextValue = Truncate == 0
                  ? FloatValue.ToString()
                  : FloatValue.ToString($"F{Truncate}");
            }
        }
    }
}
