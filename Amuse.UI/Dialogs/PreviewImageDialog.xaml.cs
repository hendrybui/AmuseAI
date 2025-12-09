using Amuse.UI.Commands;
using Amuse.UI.Models;
using Amuse.UI.Services;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Amuse.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for PreviewImageDialog.xaml
    /// </summary>
    public partial class PreviewImageDialog : BaseDialog
    {
        private readonly IFileService _fileService;
        private readonly int _previewMaximum = 680;

        private int _imageHeight;
        private int _imageWidth;
        private bool _isActualSize;
        private Stretch _imageStrech;
        private IImageResult _imageResult;
        private BitmapSource _imageSource;
        private ScrollBarVisibility _scrollBarVisibility;


        public PreviewImageDialog(IFileService fileService)
        {
            _fileService = fileService;
            CopyImageCommand = new AsyncRelayCommand(CopyImage);
            SaveImageCommand = new AsyncRelayCommand(SaveImage);
            ImageHeight = _previewMaximum;
            ImageWidth = _previewMaximum;
            InitializeComponent();
        }

        public AsyncRelayCommand CopyImageCommand { get; }
        public AsyncRelayCommand SaveImageCommand { get; }

        public BitmapSource ImageSource
        {
            get { return _imageSource; }
            set { _imageSource = value; NotifyPropertyChanged(); }
        }

        public ScrollBarVisibility ScrollBarVisibility
        {
            get { return _scrollBarVisibility; }
            set { _scrollBarVisibility = value; NotifyPropertyChanged(); }
        }

        public bool IsActualSize
        {
            get { return _isActualSize; }
            set { _isActualSize = value; NotifyPropertyChanged(); UpdateScrollBar(); }
        }

        public Stretch ImageStrech
        {
            get { return _imageStrech; }
            set { _imageStrech = value; NotifyPropertyChanged(); }
        }
 
        public int ImageHeight
        {
            get { return _imageHeight; }
            set { _imageHeight = value; NotifyPropertyChanged(); }
        }
     
        public int ImageWidth
        {
            get { return _imageWidth; }
            set { _imageWidth = value; NotifyPropertyChanged(); }
        }


        private void UpdateScrollBar()
        {
            ImageStrech = _isActualSize
                ? Stretch.None
                : Stretch.Uniform;
            ScrollBarVisibility = _isActualSize && ImageSource.PixelHeight > _previewMaximum
                ? ScrollBarVisibility.Auto
                : ScrollBarVisibility.Disabled;
            WindowState = _isActualSize && ImageSource.PixelHeight > _previewMaximum
                ? WindowState.Maximized
                : WindowState.Normal;

            ImageWidth = _isActualSize ? ImageSource.PixelWidth : _previewMaximum;
            ImageHeight = _isActualSize  ? ImageSource.PixelHeight : _previewMaximum;
        }


        public Task<bool> ShowDialogAsync(string title, IImageResult imageResult)
        {
            _imageResult = imageResult;
            Title = title;
            ImageSource = imageResult.Image;
            UpdateScrollBar();
            return base.ShowDialogAsync();
        }


        private Task CopyImage()
        {
            Clipboard.SetImage(_imageSource);
            return Task.CompletedTask;
        }


        private async Task SaveImage()
        {
            await _fileService.SaveAsImageFile(_imageResult);
        }
    }
}
