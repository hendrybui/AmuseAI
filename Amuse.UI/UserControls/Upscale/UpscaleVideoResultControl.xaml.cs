using Amuse.UI.Commands;
using Amuse.UI.Models;
using Amuse.UI.Models.Upscale;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Amuse.UI.UserControls
{
    public partial class UpscaleVideoResultControl : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpscaleVideoResultControl" /> class.
        /// </summary>
        public UpscaleVideoResultControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty UpscaleInfoProperty =
            DependencyProperty.Register(nameof(UpscaleInfo), typeof(UpscaleInfoModel), typeof(UpscaleVideoResultControl));

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(VideoInputModel), typeof(UpscaleVideoResultControl));

        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register(nameof(Result), typeof(UpscaleVideoResultModel), typeof(UpscaleVideoResultControl));

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(nameof(Progress), typeof(ProgressInfo), typeof(UpscaleVideoResultControl));

        public static readonly DependencyProperty SaveVideoCommandProperty =
            DependencyProperty.Register(nameof(SaveVideoCommand), typeof(AsyncRelayCommand<UpscaleVideoResultModel>), typeof(UpscaleVideoResultControl));

        public static readonly DependencyProperty RemoveVideoCommandProperty =
            DependencyProperty.Register(nameof(RemoveVideoCommand), typeof(AsyncRelayCommand<UpscaleVideoResultModel>), typeof(UpscaleVideoResultControl));

        public static readonly DependencyProperty CopyVideoCommandProperty =
            DependencyProperty.Register(nameof(CopyVideoCommand), typeof(AsyncRelayCommand<UpscaleVideoResultModel>), typeof(UpscaleVideoResultControl));

        public static readonly DependencyProperty PreviewImageProperty =
            DependencyProperty.Register(nameof(PreviewImage), typeof(BitmapSource), typeof(UpscaleVideoResultControl));

        public static readonly DependencyProperty VideoSyncProperty =
            DependencyProperty.Register(nameof(VideoSync), typeof(bool), typeof(UpscaleVideoResultControl));

        public UpscaleInfoModel UpscaleInfo
        {
            get { return (UpscaleInfoModel)GetValue(UpscaleInfoProperty); }
            set { SetValue(UpscaleInfoProperty, value); }
        }

        public VideoInputModel Source
        {
            get { return (VideoInputModel)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public UpscaleVideoResultModel Result
        {
            get { return (UpscaleVideoResultModel)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

        public ProgressInfo Progress
        {
            get { return (ProgressInfo)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public AsyncRelayCommand<UpscaleVideoResultModel> SaveVideoCommand
        {
            get { return (AsyncRelayCommand<UpscaleVideoResultModel>)GetValue(SaveVideoCommandProperty); }
            set { SetValue(SaveVideoCommandProperty, value); }
        }

        public AsyncRelayCommand<UpscaleVideoResultModel> RemoveVideoCommand
        {
            get { return (AsyncRelayCommand<UpscaleVideoResultModel>)GetValue(RemoveVideoCommandProperty); }
            set { SetValue(RemoveVideoCommandProperty, value); }
        }

        public AsyncRelayCommand<UpscaleVideoResultModel> CopyVideoCommand
        {
            get { return (AsyncRelayCommand<UpscaleVideoResultModel>)GetValue(CopyVideoCommandProperty); }
            set { SetValue(CopyVideoCommandProperty, value); }
        }

        public BitmapSource PreviewImage
        {
            get { return (BitmapSource)GetValue(PreviewImageProperty); }
            set { SetValue(PreviewImageProperty, value); }
        }

        public bool VideoSync
        {
            get { return (bool)GetValue(VideoSyncProperty); }
            set { SetValue(VideoSyncProperty, value); }
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseEnter" /> attached event is raised on this element. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
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
