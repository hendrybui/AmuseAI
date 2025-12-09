using Amuse.UI.Models.FeatureExtractor;
using Amuse.UI.Models.StableDiffusion;
using Amuse.UI.Models.Upscale;
using Amuse.UI.Views;
using OnnxStack.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading;


namespace Amuse.UI.Models
{

    public enum ModelTemplateGroup
    {
        Fixed = 0,
        Online = 1,
        Custom = 2
    }


    public enum ModelLicenceType
    {
        [Description("Non-Commercial")]
        NonCommercial = 0,

        [Description("Commercial")]
        Commercial = 1
    }

    public class ModelTemplateViewModel : INotifyPropertyChanged
    {
        private string _name;
        private string _imageIcon;
        private string _author;
        private string _description;

        private bool _isInstalled;
        private bool _isDownloading;
        private bool _isQueued;
        private double _progressValue;
        private string _progressText;
        private string _errorMessage;
        private bool _isUpdateAvailable;

        public Guid Id { get; set; }
        public string FileVersion { get; init; }
        public bool IsUpdateAvailable
        {
            get { return _isUpdateAvailable; }
            set { _isUpdateAvailable = value; NotifyPropertyChanged(); }
        }
        public DateTime Created { get; init; }
        public bool IsProtected { get; set; }
        public bool IsFixedInstall { get; set; }
       
        [JsonIgnore]
        public GroupHeader GroupHeader
        {
            get
            {
                if (Group == ModelTemplateGroup.Fixed)
                    return GroupHeader.InstallTemplates;

                switch (Category)
                {
                    case ModelTemplateCategory.StableDiffusion:
                        return GroupHeader.StableDiffusion;
                    case ModelTemplateCategory.Upscaler:
                        return GroupHeader.Upscaler;
                    case ModelTemplateCategory.ControlNet:
                        return GroupHeader.ControlNet;
                    case ModelTemplateCategory.FeatureExtractor:
                        return GroupHeader.FeatureExtractor;
                    default:
                        break;
                }
                return GroupHeader.StableDiffusion;
            }
        }


        public string Name
        {
            get { return _name; }
            set { _name = value; NotifyPropertyChanged(); }
        }

        public string ImageIcon
        {
            get { return _imageIcon; }
            set { _imageIcon = value; NotifyPropertyChanged(); }
        }

        public string Author
        {
            get { return _author; }
            set { _author = value; NotifyPropertyChanged(); }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; NotifyPropertyChanged(); }
        }

        public int Rank { get; set; }
        public ModelTemplateGroup Group { get; set; }

        [JsonIgnore]
        public bool IsUserTemplate => Group != ModelTemplateGroup.Fixed;
        public string Template { get; set; }

        public ModelTemplateCategory Category { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public UpscaleModelTemplate UpscaleTemplate { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public StableDiffusionModelTemplate StableDiffusionTemplate { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ControlNetModelTemplate ControlNetTemplate { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public FeatureExtractorModelTemplate FeatureExtractorTemplate { get; set; }

        public float MemoryMin { get; set; }
        public float MemoryMax { get; set; }
        public float DownloadSize { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Website { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Licence { get; set; }
        public ModelLicenceType LicenceType { get; set; }
        public bool IsLicenceAccepted { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Repository { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string> RepositoryFiles { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string> PreviewImages { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string> Tags { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string> Variants { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string> Labels { get; set; }

        [JsonIgnore]
        public bool IsInstalled
        {
            get { return _isInstalled; }
            set { _isInstalled = value; NotifyPropertyChanged(); }
        }

        [JsonIgnore]
        public bool IsQueued
        {
            get { return _isQueued; }
            set { _isQueued = value; NotifyPropertyChanged(); }
        }

        [JsonIgnore]
        public bool IsDownloading
        {
            get { return _isDownloading; }
            set
            {
                _isDownloading = value;
                NotifyPropertyChanged();
                DownloadInfo = _isDownloading ? new DownloadInfo(DownloadSize) : default;
            }
        }

        [JsonIgnore]
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

        [JsonIgnore]
        public string ProgressText
        {
            get { return _progressText; }
            set { _progressText = value; NotifyPropertyChanged(); }
        }

        [JsonIgnore]
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; NotifyPropertyChanged(); }
        }

        [JsonIgnore]
        public bool IsRepositoryDownloadEnabled => !RepositoryFiles.IsNullOrEmpty();

        [JsonIgnore]
        public CancellationTokenSource CancellationTokenSource { get; set; }

        private DownloadInfo _downloadInfo;

        [JsonIgnore]
        public DownloadInfo DownloadInfo
        {
            get { return _downloadInfo; }
            set { _downloadInfo = value; NotifyPropertyChanged(); }
        }


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion
    }

    public class DownloadInfo : INotifyPropertyChanged
    {
        public double DownloadSize
        {
            get { return _downloadSize; }
            set { _downloadSize = value; NotifyPropertyChanged(); }
        } // in bytes

        public double DownloadAmount
        {
            get { return _downloadAmount; }
            set { _downloadAmount = value; NotifyPropertyChanged(); }
        } // in bytes

        public double Percentage
        {
            get { return _percentage; }
            set { _percentage = value; NotifyPropertyChanged(); }
        }   // in percentage
        public double DownloadSpeed
        {
            get { return _downloadSpeed; }
            private set { _downloadSpeed = value; NotifyPropertyChanged(); }
        }

        // in bytes per second
        public double TimeRemaining
        {
            get { return _timeRemaining; }
            private set { _timeRemaining = value; NotifyPropertyChanged(); }
        } // in seconds


        private DateTime lastUpdateTime;
        private double _downloadSize;
        private double _percentage;
        private double _downloadSpeed;
        private double _timeRemaining;
        private double _downloadAmount;
        public DownloadInfo(double downloadSize)
        {
            DownloadSize = ConvertGBToBytes(downloadSize);
            Percentage = 0;
            lastUpdateTime = DateTime.Now;
            DownloadAmount = GetAmountDownloaded();
        }

        public double GetAmountDownloaded()
        {
            return (Percentage / 100.0) * DownloadSize;
        }

        public void UpdateDownload(double newPercentage)
        {
            Percentage = newPercentage;
            DateTime currentTime = DateTime.Now;
            double currentAmountDownloaded = GetAmountDownloaded();

            // Calculate download speed
            double timeElapsed = (currentTime - lastUpdateTime).TotalSeconds;
            if (timeElapsed > 0)
            {
                DownloadSpeed = (currentAmountDownloaded - DownloadAmount) / timeElapsed;
            }

            // Calculate time remaining
            if (DownloadSpeed > 0)
            {
                TimeRemaining = (DownloadSize - currentAmountDownloaded) / DownloadSpeed;
            }

            // Update last values
            lastUpdateTime = currentTime;
            DownloadAmount = currentAmountDownloaded;
        }

        public static long ConvertGBToBytes(double gigabytes)
        {
            const long bytesPerGB = 1073741824; // 1 GB = 2^30 Bytes
            return (long)(gigabytes * bytesPerGB);
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
