using Amuse.UI.Commands;
using Amuse.UI.Helpers;
using Amuse.UI.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Amuse.UI.UserControls
{
    public partial class EZModeVideoResultControl : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EZModeVideoResultControl" /> class.
        /// </summary>
        public EZModeVideoResultControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty OriginalProperty =
            DependencyProperty.Register(nameof(Original), typeof(VideoInputModel), typeof(EZModeVideoResultControl));

        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register(nameof(Result), typeof(VideoResultModel), typeof(EZModeVideoResultControl));

        public static readonly DependencyProperty PreviewProperty =
             DependencyProperty.Register(nameof(Preview), typeof(BitmapSource), typeof(EZModeVideoResultControl));

        public static readonly DependencyProperty SaveVideoCommandProperty =
            DependencyProperty.Register(nameof(SaveVideoCommand), typeof(AsyncRelayCommand<VideoResultModel>), typeof(EZModeVideoResultControl));

        public static readonly DependencyProperty CopyVideoCommandProperty =
            DependencyProperty.Register(nameof(CopyVideoCommand), typeof(AsyncRelayCommand<VideoResultModel>), typeof(EZModeVideoResultControl));

        public static readonly DependencyProperty VideoSyncProperty =
            DependencyProperty.Register(nameof(VideoSync), typeof(bool), typeof(EZModeVideoResultControl));

        public static readonly DependencyProperty IsSplitterEnabledProperty =
            DependencyProperty.Register(nameof(IsSplitterEnabled), typeof(bool), typeof(EZModeVideoResultControl));

        public VideoInputModel Original
        {
            get { return (VideoInputModel)GetValue(OriginalProperty); }
            set { SetValue(OriginalProperty, value); }
        }

        public VideoResultModel Result
        {
            get { return (VideoResultModel)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

        public BitmapSource Preview
        {
            get { return (BitmapSource)GetValue(PreviewProperty); }
            set { SetValue(PreviewProperty, value); }
        }

        public AsyncRelayCommand<VideoResultModel> SaveVideoCommand
        {
            get { return (AsyncRelayCommand<VideoResultModel>)GetValue(SaveVideoCommandProperty); }
            set { SetValue(SaveVideoCommandProperty, value); }
        }

        public AsyncRelayCommand<VideoResultModel> CopyVideoCommand
        {
            get { return (AsyncRelayCommand<VideoResultModel>)GetValue(CopyVideoCommandProperty); }
            set { SetValue(CopyVideoCommandProperty, value); }
        }

        public bool IsSplitterEnabled
        {
            get { return (bool)GetValue(IsSplitterEnabledProperty); }
            set { SetValue(IsSplitterEnabledProperty, value); }
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
