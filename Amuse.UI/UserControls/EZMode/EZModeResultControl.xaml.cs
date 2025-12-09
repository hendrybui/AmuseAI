using Amuse.UI.Commands;
using Amuse.UI.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Amuse.UI.UserControls
{
    /// <summary>
    /// Interaction logic for EZModeResultControl.xaml
    /// </summary>
    public partial class EZModeResultControl : UserControl
    {
        public EZModeResultControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty OriginalProperty =
            DependencyProperty.Register(nameof(Original), typeof(ImageInput), typeof(EZModeResultControl));

        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register(nameof(Result), typeof(ImageResult), typeof(EZModeResultControl));

        public static readonly DependencyProperty SaveImageCommandProperty =
            DependencyProperty.Register(nameof(SaveImageCommand), typeof(AsyncRelayCommand<ImageResult>), typeof(EZModeResultControl));

        public static readonly DependencyProperty CopyImageCommandProperty =
            DependencyProperty.Register(nameof(CopyImageCommand), typeof(AsyncRelayCommand<ImageResult>), typeof(EZModeResultControl));

        public static readonly DependencyProperty PreviewProperty =
            DependencyProperty.Register(nameof(Preview), typeof(BitmapSource), typeof(EZModeResultControl));

        public static readonly DependencyProperty IsSplitterEnabledProperty =
            DependencyProperty.Register(nameof(IsSplitterEnabled), typeof(bool), typeof(EZModeResultControl));

        public AsyncRelayCommand<ImageResult> SaveImageCommand
        {
            get { return (AsyncRelayCommand<ImageResult>)GetValue(SaveImageCommandProperty); }
            set { SetValue(SaveImageCommandProperty, value); }
        }

        public AsyncRelayCommand<ImageResult> CopyImageCommand
        {
            get { return (AsyncRelayCommand<ImageResult>)GetValue(CopyImageCommandProperty); }
            set { SetValue(CopyImageCommandProperty, value); }
        }

        public ImageInput Original
        {
            get { return (ImageInput)GetValue(OriginalProperty); }
            set { SetValue(OriginalProperty, value); }
        }

        public ImageResult Result
        {
            get { return (ImageResult)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

        public BitmapSource Preview
        {
            get { return (BitmapSource)GetValue(PreviewProperty); }
            set { SetValue(PreviewProperty, value); }
        }

        public bool IsSplitterEnabled
        {
            get { return (bool)GetValue(IsSplitterEnabledProperty); }
            set { SetValue(IsSplitterEnabledProperty, value); }
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseEnter" /> attached event is raised on this element. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            Keyboard.Focus(this);
            base.OnMouseEnter(e);
        }


        /// <summary>
        /// Handles the PreviewMouseLeftButtonDown event of the ResultImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void ResultImage_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.Source is Image image)
            {
                DragDrop.DoDragDrop(image, new DataObject(typeof(BitmapImage), image.Source), DragDropEffects.All);
            }
        }
    }
}
