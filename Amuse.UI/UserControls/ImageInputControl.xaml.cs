using Amuse.UI.Commands;
using Amuse.UI.Models;
using Amuse.UI.Services;
using OnnxStack.Core;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Amuse.UI.UserControls
{
    public partial class ImageInputControl : UserControl, INotifyPropertyChanged
    {
        private readonly IFileService _fileService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageInputControl" /> class.
        /// </summary>
        public ImageInputControl()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
                _fileService = App.GetService<IFileService>();

            LoadImageCommand = new AsyncRelayCommand(LoadImage);
            ClearImageCommand = new AsyncRelayCommand(ClearImage, CanClearImage);
            CopyImageCommand = new AsyncRelayCommand(CopyImage, CanCopyImage);
            PasteImageCommand = new AsyncRelayCommand(PasteImage);
            SaveImageCommand = new AsyncRelayCommand(SaveImage, CanSaveImage);
            InitializeComponent();
        }

        public AsyncRelayCommand CopyImageCommand { get; }
        public AsyncRelayCommand PasteImageCommand { get; }
        public AsyncRelayCommand SaveImageCommand { get; }

        public static readonly DependencyProperty LoadImageCommandProperty =
            DependencyProperty.Register(nameof(LoadImageCommand), typeof(AsyncRelayCommand), typeof(ImageInputControl));

        public static readonly DependencyProperty ClearImageCommandProperty =
            DependencyProperty.Register(nameof(ClearImageCommand), typeof(AsyncRelayCommand), typeof(ImageInputControl));

        public static readonly DependencyProperty ImageWidthProperty =
            DependencyProperty.Register(nameof(ImageWidth), typeof(int), typeof(ImageInputControl), new PropertyMetadata(512, OnSizeChanged));

        public static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.Register(nameof(ImageHeight), typeof(int), typeof(ImageInputControl), new PropertyMetadata(512, OnSizeChanged));

        public static readonly DependencyProperty SourceImageProperty =
            DependencyProperty.Register(nameof(SourceImage), typeof(ImageInput), typeof(ImageInputControl), new PropertyMetadata(OnSourceChanged));

        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register(nameof(Result), typeof(ImageInput), typeof(ImageInputControl));

        public static readonly DependencyProperty IsCropDialogEnabledProperty =
            DependencyProperty.Register(nameof(IsCropDialogEnabled), typeof(bool), typeof(ImageInputControl));

        public static readonly DependencyProperty ToolbarVisibilityProperty =
            DependencyProperty.Register(nameof(ToolbarVisibility), typeof(Visibility), typeof(ImageInputControl));

        public AsyncRelayCommand LoadImageCommand
        {
            get { return (AsyncRelayCommand)GetValue(LoadImageCommandProperty); }
            set { SetValue(LoadImageCommandProperty, value); }
        }

        public AsyncRelayCommand ClearImageCommand
        {
            get { return (AsyncRelayCommand)GetValue(ClearImageCommandProperty); }
            set { SetValue(ClearImageCommandProperty, value); }
        }

        public int ImageWidth
        {
            get { return (int)GetValue(ImageWidthProperty); }
            set { SetValue(ImageWidthProperty, value); }
        }

        public int ImageHeight
        {
            get { return (int)GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        public ImageInput SourceImage
        {
            get { return (ImageInput)GetValue(SourceImageProperty); }
            set { SetValue(SourceImageProperty, value); }
        }

        public ImageInput Result
        {
            get { return (ImageInput)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

        public Visibility ToolbarVisibility
        {
            get { return (Visibility)GetValue(ToolbarVisibilityProperty); }
            set { SetValue(ToolbarVisibilityProperty, value); }
        }

        public bool IsCropDialogEnabled
        {
            get { return (bool)GetValue(IsCropDialogEnabledProperty); }
            set { SetValue(IsCropDialogEnabledProperty, value); }
        }


        /// <summary>
        /// Loads the image.
        /// </summary>
        /// <returns></returns>
        private async Task LoadImage()
        {
            await ShowDialog();
        }


        /// <summary>
        /// Clears the image.
        /// </summary>
        /// <returns></returns>
        private Task ClearImage()
        {
            Result = null;
            SetCurrentValue(SourceImageProperty, null);
            return Task.CompletedTask;
        }


        /// <summary>
        /// Can ClearImage
        /// </summary>
        /// <returns><c>true</c> if this instance [can clear image]; otherwise, <c>false</c>.</returns>
        private bool CanClearImage()
        {
            return Result?.Image != null;
        }


        /// <summary>
        /// Copies the image.
        /// </summary>
        /// <returns></returns>
        private Task CopyImage()
        {
            Clipboard.SetImage(Result.Image);
            return Task.CompletedTask;
        }


        /// <summary>
        /// Determines whether this instance can copy the image.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can copy image; otherwise, <c>false</c>.
        /// </returns>
        private bool CanCopyImage()
        {
            return Result?.Image != null;
        }


        /// <summary>
        /// Saves the image.
        /// </summary>
        private async Task SaveImage()
        {
            await _fileService.SaveAsImageFile(Result.Image.ToBitmapImage());
        }


        /// <summary>
        /// Determines whether this instance can save the image.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can save image; otherwise, <c>false</c>.
        /// </returns>
        private bool CanSaveImage()
        {
            return Result?.Image != null;
        }


        /// <summary>
        /// Paste the image.
        /// </summary>
        /// <returns></returns>
        private async Task PasteImage()
        {
            if (Clipboard.ContainsImage())
                await ShowDialog(Clipboard.GetImage());
            else if (Clipboard.ContainsFileDropList())
            {
                var imageFile = Clipboard.GetFileDropList()
                    .OfType<string>()
                    .FirstOrDefault();
                await ShowDialog(null, imageFile);
            }
        }


        /// <summary>
        /// Shows the image dialog.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sourceFile">The source file.</param>
        private async Task ShowDialog(BitmapSource source = null, string sourceFile = null)
        {
            if (IsCropDialogEnabled)
                await ShowCropImageDialog(source, sourceFile);
            else
                await ShowImageDialog(source, sourceFile);
        }


        /// <summary>
        /// Shows the crop image dialog.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sourceFile">The source file.</param>
        private async Task ShowCropImageDialog(BitmapSource source = null, string sourceFile = null)
        {
            var imageResult = await _fileService.OpenImageFileCropped(ImageWidth, ImageHeight, source, sourceFile);
            if (imageResult == null)
                return;

            imageResult.FileName ??= sourceFile ?? "Image Data";
            SetCurrentValue(SourceImageProperty, imageResult);
        }


        /// <summary>
        /// Shows the image picker dialog.
        /// </summary>
        private async Task ShowImageDialog(BitmapSource source = null, string sourceFile = null)
        {
            var imageResult = source == null
                ? await _fileService.OpenImageFile(sourceFile)
                : new ImageInput { Image = source, FileName = "Image Data" };

            if (imageResult == null)
                return;

            SetCurrentValue(SourceImageProperty, imageResult);
        }


        /// <summary>
        /// Refreshes this Image
        /// </summary>
        private Task Refresh()
        {
            Result = new ImageInput
            {
                Image = SourceImage.Image,
                FileName = SourceImage.FileName,
            };
            return Task.CompletedTask;
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
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseLeave" /> attached event is raised on this element. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            Keyboard.ClearFocus();
            base.OnMouseLeave(e);
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.DragDrop.PreviewDrop" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.DragEventArgs" /> that contains the event data.</param>
        protected override async void OnPreviewDrop(DragEventArgs e)
        {
            var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (!fileNames.IsNullOrEmpty())
                await ShowDialog(null, fileNames.FirstOrDefault());

            var bitmapImage = e.Data.GetData(typeof(BitmapImage)) as BitmapImage;
            if (bitmapImage is not null)
                await ShowImageDialog(bitmapImage);
        }


        /// <summary>
        /// Handles the <see cref="E:SourceChanged" /> event.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static async void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageInputControl control && control.SourceImage is not null)
                await control.Refresh();
        }


        /// <summary>
        /// Handles the <see cref="E:SizeChanged" /> event.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static async void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageInputControl control && control.SourceImage is not null)
                await control.Refresh();
        }


        /// <summary>
        /// Handles the PreviewMouseLeftButtonUp event of the Control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private async void Control_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                await ShowDialog();
            }
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
