using Amuse.UI.Commands;
using Amuse.UI.Models;
using Amuse.UI.Models.Upscale;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Amuse.UI.UserControls
{
    public partial class UpscaleResultControl : UserControl, INotifyPropertyChanged
    {
        /// <summary>Initializes a new instance of the <see cref="UpscaleResultControl" /> class.</summary>
        public UpscaleResultControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty UpscaleInfoProperty =
            DependencyProperty.Register(nameof(UpscaleInfo), typeof(UpscaleInfoModel), typeof(UpscaleResultControl));

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(ImageInput), typeof(UpscaleResultControl));

        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register(nameof(Result), typeof(UpscaleResult), typeof(UpscaleResultControl));

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(nameof(Progress), typeof(ProgressInfo), typeof(UpscaleResultControl));

        public static readonly DependencyProperty SaveImageCommandProperty =
            DependencyProperty.Register(nameof(SaveImageCommand), typeof(AsyncRelayCommand<UpscaleResult>), typeof(UpscaleResultControl));

        public static readonly DependencyProperty RemoveImageCommandProperty =
            DependencyProperty.Register(nameof(RemoveImageCommand), typeof(AsyncRelayCommand<UpscaleResult>), typeof(UpscaleResultControl));

        public static readonly DependencyProperty CopyImageCommandProperty =
            DependencyProperty.Register(nameof(CopyImageCommand), typeof(AsyncRelayCommand<UpscaleResult>), typeof(UpscaleResultControl));

        public UpscaleInfoModel UpscaleInfo
        {
            get { return (UpscaleInfoModel)GetValue(UpscaleInfoProperty); }
            set { SetValue(UpscaleInfoProperty, value); }
        }

        public ImageInput Source
        {
            get { return (ImageInput)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public UpscaleResult Result
        {
            get { return (UpscaleResult)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

        public ProgressInfo Progress
        {
            get { return (ProgressInfo)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public AsyncRelayCommand<UpscaleResult> SaveImageCommand
        {
            get { return (AsyncRelayCommand<UpscaleResult>)GetValue(SaveImageCommandProperty); }
            set { SetValue(SaveImageCommandProperty, value); }
        }

        public AsyncRelayCommand<UpscaleResult> RemoveImageCommand
        {
            get { return (AsyncRelayCommand<UpscaleResult>)GetValue(RemoveImageCommandProperty); }
            set { SetValue(RemoveImageCommandProperty, value); }
        }

        public AsyncRelayCommand<UpscaleResult> CopyImageCommand
        {
            get { return (AsyncRelayCommand<UpscaleResult>)GetValue(CopyImageCommandProperty); }
            set { SetValue(CopyImageCommandProperty, value); }
        }


        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (App.Current.MainWindow.IsActive)
                Focus();
            base.OnMouseEnter(e);
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
