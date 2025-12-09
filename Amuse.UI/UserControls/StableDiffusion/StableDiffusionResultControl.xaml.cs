using Amuse.UI.Commands;
using Amuse.UI.Models;
using Amuse.UI.Models.StableDiffusion;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Amuse.UI.UserControls
{
    public partial class StableDiffusionResultControl : UserControl, INotifyPropertyChanged
    {
        private bool _isSplitterEnabled;

        /// <summary>Initializes a new instance of the <see cref="StableDiffusionResultControl" /> class.</summary>
        public StableDiffusionResultControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SchedulerOptionsProperty =
            DependencyProperty.Register(nameof(SchedulerOptions), typeof(SchedulerOptionsModel), typeof(StableDiffusionResultControl));

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(ImageInput), typeof(StableDiffusionResultControl));

        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register(nameof(Result), typeof(ImageResult), typeof(StableDiffusionResultControl));

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(nameof(Progress), typeof(ProgressInfo), typeof(StableDiffusionResultControl));

        public static readonly DependencyProperty SaveImageCommandProperty =
            DependencyProperty.Register(nameof(SaveImageCommand), typeof(AsyncRelayCommand<ImageResult>), typeof(StableDiffusionResultControl));

        public static readonly DependencyProperty RemoveImageCommandProperty =
            DependencyProperty.Register(nameof(RemoveImageCommand), typeof(AsyncRelayCommand<ImageResult>), typeof(StableDiffusionResultControl));

        public static readonly DependencyProperty CopyImageCommandProperty =
            DependencyProperty.Register(nameof(CopyImageCommand), typeof(AsyncRelayCommand<ImageResult>), typeof(StableDiffusionResultControl));

        public static readonly DependencyProperty UpdateSeedCommandProperty =
            DependencyProperty.Register(nameof(UpdateSeedCommand), typeof(AsyncRelayCommand<int>), typeof(StableDiffusionResultControl));

        public static readonly DependencyProperty PreviewImageProperty =
            DependencyProperty.Register(nameof(PreviewImage), typeof(BitmapSource), typeof(StableDiffusionResultControl));

        public SchedulerOptionsModel SchedulerOptions
        {
            get { return (SchedulerOptionsModel)GetValue(SchedulerOptionsProperty); }
            set { SetValue(SchedulerOptionsProperty, value); }
        }

        public ImageInput Source
        {
            get { return (ImageInput)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public ImageResult Result
        {
            get { return (ImageResult)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

        public ProgressInfo Progress
        {
            get { return (ProgressInfo)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public AsyncRelayCommand<ImageResult> SaveImageCommand
        {
            get { return (AsyncRelayCommand<ImageResult>)GetValue(SaveImageCommandProperty); }
            set { SetValue(SaveImageCommandProperty, value); }
        }

        public AsyncRelayCommand<ImageResult> RemoveImageCommand
        {
            get { return (AsyncRelayCommand<ImageResult>)GetValue(RemoveImageCommandProperty); }
            set { SetValue(RemoveImageCommandProperty, value); }
        }

        public AsyncRelayCommand<ImageResult> CopyImageCommand
        {
            get { return (AsyncRelayCommand<ImageResult>)GetValue(CopyImageCommandProperty); }
            set { SetValue(CopyImageCommandProperty, value); }
        }

        public AsyncRelayCommand<int> UpdateSeedCommand
        {
            get { return (AsyncRelayCommand<int>)GetValue(UpdateSeedCommandProperty); }
            set { SetValue(UpdateSeedCommandProperty, value); }
        }

        public BitmapSource PreviewImage
        {
            get { return (BitmapSource)GetValue(PreviewImageProperty); }
            set { SetValue(PreviewImageProperty, value); }
        }

        public bool IsSplitterEnabled
        {
            get { return _isSplitterEnabled; }
            set { _isSplitterEnabled = value; NotifyPropertyChanged(); }
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
