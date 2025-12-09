using Amuse.UI.Models.StableDiffusion;
using OnnxStack.Core.Image;
using OnnxStack.StableDiffusion.Config;
using OnnxStack.StableDiffusion.Enums;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace Amuse.UI.Models
{
    public class ImageResult : IImageResult, INotifyPropertyChanged
    {
        private BitmapSource _previewImage;

        [JsonIgnore]
        public BitmapSource Image { get; init; }

        [JsonIgnore]
        public OnnxImage OnnxImage { get; init; }

        public string ModelName => Pipeline.BaseModel.Name;
        public StableDiffusionPipelineModel Pipeline { get; set; }
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public PipelineType PipelineType { get; init; }
        public DiffuserType DiffuserType { get; init; }
        public GenerateOptions PromptOptions { get; init; }
        public SchedulerOptions SchedulerOptions { get; init; }
        public double Elapsed { get; init; }

        public string FilePrefix => SchedulerOptions?.Seed.ToString();
        public int Width => Image?.PixelWidth ?? 0;
        public int Height => Image?.PixelHeight ?? 0;

        [JsonIgnore]
        public BitmapSource PreviewImage
        {
            get { return _previewImage; }
            set { _previewImage = value; NotifyPropertyChanged(); }
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