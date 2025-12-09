using Amuse.UI.Commands;
using Amuse.UI.Models;
using Amuse.UI.Services;
using OnnxStack.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Amuse.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for ModelDownloadDialog.xaml
    /// </summary>
    public partial class ModelDownloadDialog : BaseDialog
    {
        private bool _isDownloading;
        private string _errorMessage;
        private readonly AmuseSettings _settings;
        private readonly IModelDownloadService _modelDownloadService;
        private readonly IDialogService _dialogService;
        private bool _isLicenceAccepted;

        public ModelDownloadDialog(AmuseSettings settings, IModelDownloadService modelDownloadService, IDialogService dialogService)
        {
            _settings = settings;
            _modelDownloadService = modelDownloadService;
            _dialogService = dialogService;
            DownloadCommand = new AsyncRelayCommand(DownloadModel, CanExecuteDownload);
            ChangeLocationCommand = new AsyncRelayCommand(ChangeDownloadLocation, CanExecuteChangeDownloadLocation);
            ModelTemplates = new ObservableCollection<ModelTemplateViewModel>();
            InitializeComponent();
        }

        public AsyncRelayCommand DownloadCommand { get; }
        public AsyncRelayCommand ChangeLocationCommand { get; }
        public ObservableCollection<ModelTemplateViewModel> ModelTemplates { get; }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; NotifyPropertyChanged(); }
        }

        public bool IsDownloading
        {
            get { return _isDownloading; }
            set { _isDownloading = value; NotifyPropertyChanged(); }
        }

        public bool IsLicenceAccepted
        {
            get { return _isLicenceAccepted; }
            set { _isLicenceAccepted = value; NotifyPropertyChanged(); }
        }


        public async Task<bool> ShowDialogAsync(params ModelTemplateViewModel[] modelTemplates)
        {
            foreach (var modelTemplate in modelTemplates)
            {
                if (modelTemplate is null)
                    continue;
                if (modelTemplate.IsInstalled)
                    continue;

                ModelTemplates.Add(modelTemplate);
            }
            return await base.ShowDialogAsync();
        }


        protected override async Task CancelAsync()
        {
            IsDownloading = false;
            await CancelDownload();
            await base.CancelAsync();
        }


        protected override async Task WindowClose()
        {
            IsDownloading = false;
            await CancelDownload();
            await base.WindowClose();
        }


        private async Task DownloadModel()
        {
            try
            {
                UpdateLicenses();
                IsDownloading = true;
                ErrorMessage = string.Empty;
                await _settings.SaveAsync();
                await Task.WhenAll(ModelTemplates.Select(template => _modelDownloadService.DownloadModelAsync(template, _settings.DirectoryModel)));
                foreach (var template in ModelTemplates)
                {
                    if (!string.IsNullOrEmpty(template.ErrorMessage))
                        throw new Exception(template.ErrorMessage);

                    template.CancellationTokenSource.Token.ThrowIfCancellationRequested();
                }

                await App.UIInvokeAsync(async () =>
                {
                    await _settings.SaveAsync();
                    await SaveAsync();
                });
            }
            catch (OperationCanceledException)
            {
                // download Canceled
            }
            catch (Exception)
            {
                ErrorMessage = "Failed to download required model files\nIf problems persist please try again later";
            }
            IsDownloading = false;
        }


        private bool CanExecuteDownload()
        {
            if (ModelTemplates.IsNullOrEmpty())
                return false;

            return _isLicenceAccepted;
        }


        private async Task CancelDownload()
        {
            try
            {
                foreach (var template in ModelTemplates)
                {
                    if (template.CancellationTokenSource is not null)
                    {
                        await template.CancellationTokenSource.CancelAsync();
                        while (template.IsDownloading)
                            await Task.Delay(500);
                    }
                }
            }
            catch (Exception)
            {
                // download Canceled
            }
        }


        private void UpdateLicenses()
        {
            foreach (var template in ModelTemplates)
            {
                template.IsLicenceAccepted = true;
            }
        }


        private async Task ChangeDownloadLocation()
        {
            var folder = await _dialogService.OpenFolderDialogAsync("New Model Location", _settings.DirectoryModel);
            if (!string.IsNullOrEmpty(folder))
            {
                _settings.DirectoryModel = folder;
                await _settings.SaveAsync();
            }
        }


        private bool CanExecuteChangeDownloadLocation()
        {
            return !IsDownloading;
        }
    }
}
