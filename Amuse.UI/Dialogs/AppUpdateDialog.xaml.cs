using Amuse.UI.Commands;
using Amuse.UI.Models;
using Amuse.UI.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Amuse.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for AppUpdateDialog.xaml
    /// </summary>
    public partial class AppUpdateDialog : BaseDialog
    {
        private string _errorMessage;
        private readonly IModelDownloadService _modelDownloadService;
        private CancellationTokenSource _cancellationTokenSource;
        private AmuseUpdate _amuseUpdate;
        private double _progressValue;
        private double _progressMax;
        private DownloadInfo _downloadInfo;
        private DownloadFileInfo _downloadFile;

        public AppUpdateDialog(IModelDownloadService modelDownloadService)
        {
            _modelDownloadService = modelDownloadService;
            DownloadCommand = new AsyncRelayCommand(Download);
            InitializeComponent();
        }

        public AsyncRelayCommand DownloadCommand { get; }
        public DownloadFileInfo DownloadFile => _downloadFile;

        public AmuseUpdate AmuseUpdate
        {
            get { return _amuseUpdate; }
            set { _amuseUpdate = value; NotifyPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; NotifyPropertyChanged(); }
        }

        public DownloadInfo DownloadInfo
        {
            get { return _downloadInfo; }
            set { _downloadInfo = value; NotifyPropertyChanged(); }
        }

        public double ProgressMax
        {
            get { return _progressMax; }
            set { _progressMax = value; NotifyPropertyChanged(); }
        }

        public double ProgressValue
        {
            get { return _progressValue; }
            set
            {
                _progressValue = value;
                NotifyPropertyChanged();
                DownloadInfo?.UpdateDownload(_progressValue);
            }
        }


        public async Task<bool> ShowDialogAsync(AmuseUpdate amuseUpdate)
        {
            AmuseUpdate = amuseUpdate;
            DownloadInfo = new DownloadInfo(amuseUpdate.DownloadSize);
            return await base.ShowDialogAsync();
        }


        protected override async Task CancelAsync()
        {
            await CancelDownload();
            await base.CancelAsync();
        }


        protected override async Task WindowClose()
        {
            await CancelDownload();
            await base.WindowClose();
        }


        private async Task Download()
        {
            try
            {
                ErrorMessage = string.Empty;
                _cancellationTokenSource = new CancellationTokenSource();
                var downloadLink = $"{_amuseUpdate.DownloadLink}?version={App.Version}";
                _downloadFile = await _modelDownloadService.DownloadFileAsync(downloadLink, App.TempDirectory, (f, p, t) =>
                {
                    if (ProgressMax != t)
                        ProgressMax = t;

                    if (p > ProgressValue)
                        ProgressValue = p;

                }, _cancellationTokenSource.Token);
                await SaveAsync();
            }
            catch (OperationCanceledException)
            {
                // download Canceled
            }
            catch (Exception)
            {
                ErrorMessage = "Failed to download update files\nIf problems persist please try again later";
            }
        }


        private Task CancelDownload()
        {
            _cancellationTokenSource?.Cancel();
            return Task.CompletedTask;
        }
    }
}
