using Amuse.UI.Helpers;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Amuse.UI.UserControls
{
    /// <summary>
    /// Interaction logic for CachedMedia.xaml
    /// </summary>
    public partial class CachedMedia : UserControl, INotifyPropertyChanged
    {
        private static ConcurrentDictionary<string, BitmapImage> _imageCache = new ConcurrentDictionary<string, BitmapImage>();
        private readonly HttpClient _httpClient;
        private BitmapSource _imageSource;
        private string _videoSource;
        private bool _isVideoSource;
        private Stretch _stretch = Stretch.Uniform;

        public CachedMedia()
        {
            _httpClient = new HttpClient();
            InitializeComponent();
        }

        public static readonly DependencyProperty CacheWidthProperty =
            DependencyProperty.Register(nameof(CacheWidth), typeof(int), typeof(CachedMedia), new PropertyMetadata(512));

        public static readonly DependencyProperty IsLazyCacheProperty =
            DependencyProperty.Register(nameof(IsLazyCache), typeof(bool), typeof(CachedMedia), new PropertyMetadata(false));

        public static readonly DependencyProperty CacheNameProperty =
            DependencyProperty.Register(nameof(CacheName), typeof(string), typeof(CachedMedia), new PropertyMetadata<CachedMedia>((c) => c.GetMediaAsync()));

        public static readonly DependencyProperty MediaUrlProperty =
            DependencyProperty.Register(nameof(MediaUrl), typeof(string), typeof(CachedMedia), new PropertyMetadata<CachedMedia, string>((c, v) => c.GetOrDownloadMedia(v)));

        public static readonly DependencyProperty MediaFileProperty =
            DependencyProperty.Register(nameof(MediaFile), typeof(string), typeof(CachedMedia), new PropertyMetadata<CachedMedia>((c) => c.GetMediaAsync()));

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(nameof(Placeholder), typeof(BitmapSource), typeof(CachedMedia), new PropertyMetadata(async (s, e) =>
            {
                if (s is CachedMedia cachedImage && cachedImage.ImageSource is null)
                    await cachedImage.GetOrDownloadMedia(cachedImage.MediaUrl);
            }));


        public string MediaUrl
        {
            get { return (string)GetValue(MediaUrlProperty); }
            set { SetValue(MediaUrlProperty, value); }
        }

        public string MediaFile
        {
            get { return (string)GetValue(MediaFileProperty); }
            set { SetValue(MediaFileProperty, value); }
        }

        public string CacheName
        {
            get { return (string)GetValue(CacheNameProperty); }
            set { SetValue(CacheNameProperty, value); }
        }

        public int CacheWidth
        {
            get { return (int)GetValue(CacheWidthProperty); }
            set { SetValue(CacheWidthProperty, value); }
        }

        public bool IsLazyCache
        {
            get { return (bool)GetValue(IsLazyCacheProperty); }
            set { SetValue(IsLazyCacheProperty, value); }
        }

        public BitmapSource Placeholder
        {
            get { return (BitmapSource)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public Stretch Stretch
        {
            get { return _stretch; }
            set { _stretch = value; NotifyPropertyChanged(); }
        }

        public BitmapSource ImageSource
        {
            get { return _imageSource; }
            set { _imageSource = value; NotifyPropertyChanged(); }
        }

        public string VideoSource
        {
            get { return _videoSource; }
            set { _videoSource = value; NotifyPropertyChanged(); }
        }

        public bool IsVideoSource
        {
            get { return _isVideoSource; }
            set { _isVideoSource = value; NotifyPropertyChanged(); }
        }


        public async Task GetMediaAsync()
        {
            if (string.IsNullOrEmpty(MediaFile))
                return;

            if (string.IsNullOrEmpty(CacheName))
                return;

            var cacheWidth = CacheWidth;
            var directory = GetCacheDirectory(CacheName ?? ".default");
            var existingMedia = Path.Combine(directory, MediaFile);
            if (File.Exists(existingMedia))
            {
                ImageSource = await LoadImageAsync(existingMedia, cacheWidth);
                InvalidateVisual();
            }
        }


        /// <summary>
        /// Gets or downloads the image.
        /// </summary>
        /// <param name="mediaUrl">The image URL.</param>
        public async Task GetOrDownloadMedia(string mediaUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(mediaUrl))
                {
                    ImageSource = Placeholder;
                    return;
                }

                var cacheName = CacheName;
                var cacheWidth = CacheWidth;
                var filename = Path.GetFileName(mediaUrl);
                var directory = GetCacheDirectory(CacheName ?? ".default");
                var existingImage = Path.Combine(directory, filename);
                IsVideoSource = !mediaUrl.EndsWith(".png");
                if (File.Exists(existingImage))
                {
                    if (IsVideoSource)
                    {
                        VideoSource = existingImage;
                    }
                    else
                    {
                        ImageSource = await LoadImageAsync(existingImage, cacheWidth);
                    }
                    InvalidateVisual();
                    return;
                }

                if (IsLazyCache)
                {
                    if (IsVideoSource)
                    {
                        var video = await DownloadVideoAsync(mediaUrl, existingImage);
                        if (mediaUrl == MediaUrl)
                        {
                            VideoSource = video;
                            InvalidateVisual();
                        }
                    }
                    else
                    {
                        if (_imageCache.TryGetValue(existingImage, out var cachedImage))
                        {
                            ImageSource = Placeholder;
                            InvalidateVisual();
                            return;
                        }

                        if (_imageCache.TryAdd(existingImage, default))
                        {
                            var image = await DownloadImageAsync(mediaUrl, existingImage, cacheWidth);
                            _imageCache.TryUpdate(existingImage, image, default);
                            if (mediaUrl == MediaUrl)
                            {
                                ImageSource = image;
                                InvalidateVisual();
                            }
                        }
                    }
                }
                else
                {
                    if (IsVideoSource)
                    {
                        VideoSource = await DownloadVideoAsync(mediaUrl, existingImage);
                    }
                    else
                    {
                        ImageSource = await DownloadImageAsync(mediaUrl, existingImage, CacheWidth);
                    }
                    InvalidateVisual();
                }
            }
            catch (Exception)
            {
                ImageSource = Placeholder;
                InvalidateVisual();
            }
        }


        private string GetCacheDirectory(string subDirectory)
        {
            var cachePath = Path.Combine(App.CacheDirectory, subDirectory);
            if (!Directory.Exists(cachePath))
                Directory.CreateDirectory(cachePath);
            return cachePath;
        }


        /// <summary>
        /// Loads the image from the cahe directory.
        /// </summary>
        /// <param name="imageFile">The image file.</param>
        private async Task<BitmapImage> LoadImageAsync(string imageFile, int decodePixelWidth)
        {
            var tcs = new TaskCompletionSource<BitmapImage>();
            try
            {
                if (_imageCache.TryGetValue(imageFile, out var bitmapImage))
                    return bitmapImage;

                using (var fileStream = new FileStream(imageFile, FileMode.Open, FileAccess.Read))
                {
                    bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = fileStream;
                    bitmapImage.DecodePixelWidth = decodePixelWidth;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    _imageCache.TryAdd(imageFile, bitmapImage);
                    tcs.SetResult(bitmapImage);
                }
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            return await tcs.Task;
        }


        /// <summary>
        /// Downloads the image and saves it to the cache directory.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        private async Task<BitmapImage> DownloadImageAsync(string source, string destination, int decodePixelWidth)
        {
            try
            {
                var bitmapImage = new BitmapImage();
                using (var imageStream = new MemoryStream(await _httpClient.GetByteArrayAsync(source)))
                {
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = imageStream;
                    bitmapImage.DecodePixelWidth = decodePixelWidth;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                    Directory.CreateDirectory(Path.GetDirectoryName(destination));
                    using (var fileStream = new FileStream(destination, FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }
                    return bitmapImage;
                }
            }
            catch (HttpRequestException)
            {
                return default;
            }
        }


        private async Task<string> DownloadVideoAsync(string source, string destination)
        {
            try
            {
                var videobytes = await _httpClient.GetByteArrayAsync(source);
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                await File.WriteAllBytesAsync(destination, videobytes);
                return destination;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }



        /// <summary>
        /// Handles the Loaded event of the MediaElement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MediaElement_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not MediaElement mediaElement)
                return;
            mediaElement.LoadedBehavior = MediaState.Play;
        }


        /// <summary>
        /// Handles the MediaOpened event of the MediaElement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (sender is not MediaElement mediaElement)
                return;
            mediaElement.LoadedBehavior = MediaState.Play;
        }


        /// <summary>
        /// Handles the MediaEnded event of the MediaElement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (sender is not MediaElement mediaElement)
                return;

            mediaElement.LoadedBehavior = MediaState.Stop;
            mediaElement.LoadedBehavior = MediaState.Play;
        }


        /// <summary>
        /// Handles the MouseDown event of the MediaElement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void MediaElement_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is not MediaElement mediaElement)
                return;

            mediaElement.LoadedBehavior = mediaElement.LoadedBehavior == MediaState.Pause
                ? MediaState.Play
                : MediaState.Pause;
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
