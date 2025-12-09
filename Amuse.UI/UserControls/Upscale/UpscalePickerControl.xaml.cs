using Amuse.UI.Commands;
using Amuse.UI.Models;
using Amuse.UI.Models.Upscale;
using System.Windows;
using System.Windows.Controls;

namespace Amuse.UI.UserControls
{
    /// <summary>
    /// Interaction logic for UpscalePickerControl.xaml
    /// </summary>
    public partial class UpscalePickerControl : UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="UpscalePickerControl" /> class.</summary>
        public UpscalePickerControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register(nameof(Settings), typeof(AmuseSettings), typeof(UpscalePickerControl));

        public static readonly DependencyProperty SelectedModelProperty =
            DependencyProperty.Register(nameof(SelectedModel), typeof(UpscaleModelSetViewModel), typeof(UpscalePickerControl));

        public static readonly DependencyProperty LoadCommandProperty =
           DependencyProperty.Register(nameof(LoadCommand), typeof(AsyncRelayCommand), typeof(UpscalePickerControl));

        public static readonly DependencyProperty UnloadCommandProperty =
            DependencyProperty.Register(nameof(UnloadCommand), typeof(AsyncRelayCommand), typeof(UpscalePickerControl));

        public AmuseSettings Settings
        {
            get { return (AmuseSettings)GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }

        public UpscaleModelSetViewModel SelectedModel
        {
            get { return (UpscaleModelSetViewModel)GetValue(SelectedModelProperty); }
            set { SetValue(SelectedModelProperty, value); }
        }

        public AsyncRelayCommand LoadCommand
        {
            get { return (AsyncRelayCommand)GetValue(LoadCommandProperty); }
            set { SetValue(LoadCommandProperty, value); }
        }

        public AsyncRelayCommand UnloadCommand
        {
            get { return (AsyncRelayCommand)GetValue(UnloadCommandProperty); }
            set { SetValue(UnloadCommandProperty, value); }
        }
    }
}
