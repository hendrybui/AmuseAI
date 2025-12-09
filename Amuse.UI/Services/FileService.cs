using Amuse.UI.Dialogs;
using Amuse.UI.Models;
using Microsoft.Extensions.Logging;
using OnnxStack.Core.Image;
using OnnxStack.Core.Video;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Amuse.UI.Services
{
    public class FileService : IFileService
    {
        private readonly ILogger<FileService> _logger;
        private readonly IDialogService _dialogService;
        private readonly AmuseSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileService"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="dialogService">The dialog service.</param>
        /// <param name="videoService">The video service.</param>
        /// <param name="logger">The logger.</param>
        public FileService(AmuseSettings settings, IDialogService dialogService, ILogger<FileService> logger = default)
        {
            _logger = logger;
            _settings = settings;
            _dialogService = dialogService;
        }

        #region Image Files

        /// <summary>
        /// Opens the image file.
        /// </summary>
        /// <returns></returns>
        public async Task<ImageInput> OpenImageFile(string initialImageFile = default)
        {
            try
            {
                _logger?.LogInformation($"[OpenImageFile] Opening image file dialog...");
                var imageFile = initialImageFile ?? await OpenImageFileDialog();
                if (string.IsNullOrEmpty(imageFile))
                    return default;

                _logger?.LogInformation($"[OpenImageFile] Loading image file: {imageFile}");
                var bitmapImage = Utils.LoadImageFile(imageFile);
                _logger?.LogInformation($"[OpenImageFile] Image file loaded.");
                return new ImageInput
                {
                    Image = bitmapImage,
                    FileName = imageFile
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[OpenImageFile] Error opening image file: {ex.Message}");
                return default;
            }
        }


        /// <summary>
        /// Opens the image file via the Crop dialog.
        /// </summary>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <param name="initialImage">The initial image.</param>
        /// <param name="initialImageFile">The initial image file.</param>
        /// <returns></returns>
        public Task<ImageInput> OpenImageFileCropped(int maxWidth, int maxHeight, BitmapSource initialImage = default, string initialImageFile = default)
        {
            try
            {
                if (!string.IsNullOrEmpty(initialImageFile))
                    initialImage = Utils.LoadImageFile(initialImageFile);


                _logger?.LogInformation($"[OpenImageFile] Opening image crop dialog...");
                var loadImageDialog = _dialogService.GetDialog<CropImageDialog>();
                loadImageDialog.Initialize(maxWidth, maxHeight, initialImage);
                if (loadImageDialog.ShowDialog() != true)
                {
                    _logger?.LogInformation($"[OpenImageFile] Loading image crop canceled");
                    return Task.FromResult<ImageInput>(default);
                }

                _logger?.LogInformation($"[OpenImageFile] Image file cropped and loaded, {maxWidth}x{maxHeight}, {loadImageDialog.ImageFile}");
                return Task.FromResult(new ImageInput
                {
                    Image = loadImageDialog.GetImageResult(),
                    FileName = loadImageDialog.ImageFile,
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[OpenImageFileCropped] Error opening image file: {ex.Message}");
                return Task.FromResult<ImageInput>(default);
            }
        }



        /// <summary>
        /// Saves the image file.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="filename">The filename.</param>
        public async Task SaveImageFile(BitmapSource image, string filename)
        {
            try
            {
                _logger?.LogInformation($"[SaveImageFile] Saving image, File: {filename}");
                await SaveImageFileAsync(image, filename);
                _logger?.LogInformation($"[SaveImageFile] Image file saved");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[SaveImageFile] Error saving image: {ex.Message}");
            }
        }


        /// <summary>
        /// Saves image file via FilePicker dialog.
        /// </summary>
        /// <param name="image">The image.</param>
        public async Task SaveAsImageFile(BitmapSource image)
        {
            try
            {
                _logger?.LogInformation($"[SaveAsImageFile] Saving image file...");
                var saveFileName = await SaveImageFileDialogAsync($"Result");
                if (string.IsNullOrEmpty(saveFileName))
                    return;

                await SaveImageFileAsync(image, saveFileName);
                _logger?.LogInformation($"[SaveAsImageFile] Image file saved, File: {saveFileName}");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[SaveAsImageFile] Error saving image: {ex.Message}");
            }
        }


        /// <summary>
        /// Automaticly the save image file.
        /// </summary>
        /// <param name="imageResult">The image result.</param>
        /// <param name="prefix">The prefix.</param>
        public async Task AutoSaveImageFile(IImageResult imageResult, string prefix)
        {
            try
            {
                if (!_settings.AutoSaveImage)
                    return;

                var imageFilename = Path.Combine(_settings.DirectoryImageAutoSave, GetRandomFileName("png", prefix));
                _logger?.LogInformation($"[AutoSaveImageFile] Saving image, File: {imageFilename}");
                await SaveImageFileAsync(imageResult.Image, imageFilename);


                _logger?.LogInformation($"[AutoSaveImageFile] Image file saved");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[AutoSaveImageFile] Error saving image: {ex.Message}");
            }
        }


        /// <summary>
        /// Saves the image file.
        /// </summary>
        /// <param name="imageResult">The image result.</param>
        /// <param name="filename">The filename.</param>
        public async Task SaveImageFile(IImageResult imageResult, string filename)
        {
            try
            {
                _logger?.LogInformation($"[SaveImageFile] Saving image, File: {filename}");
                await SaveImageFileAsync(imageResult.Image, filename);
                _logger?.LogInformation($"[SaveImageFile] Image file saved");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[SaveImageFile] Error saving image: {ex.Message}");
            }
        }






        /// <summary>
        /// Saves image file via FilePicker dialog.
        /// </summary>
        /// <param name="imageResult">The image result.</param>
        public async Task SaveAsImageFile(IImageResult imageResult)
        {
            try
            {
                _logger?.LogInformation($"[SaveAsImageFile] Saving image file...");
                var saveFileName = await SaveImageFileDialogAsync(imageResult.FilePrefix);
                if (string.IsNullOrEmpty(saveFileName))
                    return;

                await SaveImageFileAsync(imageResult.Image, saveFileName);
                _logger?.LogInformation($"[SaveAsImageFile] Image file saved, File: {saveFileName}");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[SaveAsImageFile] Error saving image: {ex.Message}");
            }
        }


        /// <summary>
        /// Saves a temporary Image file.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="prefix">The prefix.</param>
        /// <returns></returns>
        public async Task<string> SaveTempImageFile(OnnxImage image, string prefix = default)
        {
            try
            {
                _logger?.LogInformation($"[SaveTempImageFile] Saving temporary image file...");
                var tempImageFile = GetTempFileName("png", prefix ?? "Image");
                await image.SaveAsync(tempImageFile);
                _logger?.LogInformation($"[SaveTempImageFile] Temporary image file saved, File: {tempImageFile}");
                return tempImageFile;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[SaveTempImageFile] Error saving temporary image: {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// Opens the image file dialog.
        /// </summary>
        /// <returns></returns>
        private async Task<string> OpenImageFileDialog()
        {
            var selectedFilename = await _dialogService.OpenFileDialogAsync("Open Image", _settings.DirectoryImage, "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.gif;*.tif;*.tiff|All Files|*.*");
            if (string.IsNullOrEmpty(selectedFilename))
            {
                _logger?.LogInformation("[OpenImageFileDialog] Open image file canceled");
                return null;
            }

            return selectedFilename;
        }


        /// <summary>
        /// Saves the image file dialog.
        /// </summary>
        /// <param name="initialFilename">The initial filename.</param>
        /// <returns></returns>
        private async Task<string> SaveImageFileDialogAsync(string initialFilename)
        {
            var saveFilename = await _dialogService.SaveFileDialogAsync("Save Image", $"image-{initialFilename}.png", _settings.DirectoryImageSave, "png files (*.png)|*.png", "png");
            if (string.IsNullOrEmpty(saveFilename))
            {
                _logger?.LogInformation("[SaveImageFileDialog] Saving image canceled");
                return null;
            }

            return saveFilename;
        }


        /// <summary>
        /// Saves the image file.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        private static Task<bool> SaveImageFileAsync(BitmapSource image, string filename)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using (var fileStream = new FileStream(filename, FileMode.Create))
            {
                encoder.Save(fileStream);
            }
            return Task.FromResult(File.Exists(filename));
        }

        #endregion

        #region Video Files

        /// <summary>
        /// Opens a video file.
        /// </summary>
        /// <returns></returns>
        public async Task<VideoInputModel> OpenVideoFile()
        {
            try
            {
                _logger?.LogInformation($"[OpenVideoFile] Opening video file dialog...");
                var videoFile = await OpenVideoFileDialogAsync();
                if (string.IsNullOrEmpty(videoFile))
                    return null;

                _logger?.LogInformation($"[OpenVideoFile] Loading video file: {videoFile}");
                var videoInfo = await VideoHelper.ReadVideoInfoAsync(videoFile);
                _logger?.LogInformation($"[OpenVideoFile] Video file loaded");
                return new VideoInputModel
                {
                    FileName = videoFile,
                    Video = videoInfo
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[OpenVideoFile] Error opening video file: {ex.Message}");
                return null;
            }
        }


        /// <summary>
        /// Automaticly the save video file.
        /// </summary>
        /// <param name="videoResult">The video result.</param>
        /// <param name="prefix">The prefix.</param>
        public async Task AutoSaveVideoFile(IVideoResult videoResult, string prefix)
        {
            try
            {
                if (!_settings.AutoSaveVideo)
                    return;

                var videoFilename = Path.Combine(_settings.DirectoryVideoAutoSave, GetRandomFileName("mp4", prefix));
                _logger?.LogInformation($"[AutoSaveVideoFile] Saving video, File: {videoFilename}");
                if (File.Exists(videoResult.FileName))
                    File.Copy(videoResult.FileName, videoFilename);
                else
                    await videoResult.Video.SaveAsync(videoFilename);

                _logger?.LogInformation($"[SaveVideoFile] Video file saved.");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[AutoSaveVideoFile] Error saving video: {ex.Message}");
            }
        }


        /// <summary>
        /// Saves the video file.
        /// </summary>
        /// <param name="videoResult">The video result.</param>
        /// <param name="filename">The filename.</param>
        public async Task SaveVideoFile(IVideoResult videoResult, string filename)
        {
            try
            {
                _logger?.LogInformation($"[SaveVideoFile] Saving video, File: {filename}");
                if (File.Exists(videoResult.FileName))
                    File.Copy(videoResult.FileName, filename);
                else
                    await videoResult.Video.SaveAsync(filename);

                _logger?.LogInformation($"[SaveVideoFile] Video file saved.");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[SaveVideoFile] Error saving video: {ex.Message}");
            }
        }


        /// <summary>
        /// Saves the video file.
        /// </summary>
        /// <param name="video">The video.</param>
        /// <param name="filename">The filename.</param>
        public async Task SaveVideoFile(OnnxVideo video, string filename)
        {
            try
            {
                _logger?.LogInformation($"[SaveVideoFile] Saving video, File: {filename}");
                await video.SaveAsync(filename);
                _logger?.LogInformation($"[SaveVideoFile] Video file saved.");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[SaveVideoFile] Error saving video: {ex.Message}");
            }
        }


        /// <summary>
        /// Saves the video file via FilePicker dialog.
        /// </summary>
        /// <param name="videoResult">The video result.</param>
        public async Task SaveAsVideoFile(IVideoResult videoResult)
        {
            try
            {
                _logger?.LogInformation($"[SaveAsVideoFile] Saving video file...");
                var saveFileName = await SaveVideoFileDialogAsync($"{videoResult.FilePrefix}");
                if (string.IsNullOrEmpty(saveFileName))
                    return;

                if (File.Exists(videoResult.FileName))
                    File.Copy(videoResult.FileName, saveFileName, true);
                else
                    await videoResult.Video.SaveAsync(saveFileName);

                _logger?.LogInformation($"[SaveAsVideoFile] Video file saved, File: {saveFileName}");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[SaveAsVideoFile] Error saving video: {ex.Message}");
            }
        }


        /// <summary>
        /// Saves the video file via FilePicker dialog.
        /// </summary>
        /// <param name="videoResult">The video result.</param>
        public async Task SaveAsVideoFile(OnnxVideo video)
        {
            try
            {
                _logger?.LogInformation($"[SaveAsVideoFile] Saving video file...");
                var saveFileName = await SaveVideoFileDialogAsync($"Result");
                if (string.IsNullOrEmpty(saveFileName))
                    return;

                await video.SaveAsync(saveFileName);
                _logger?.LogInformation($"[SaveAsVideoFile] Video file saved, File: {saveFileName}");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[SaveAsVideoFile] Error saving video: {ex.Message}");
            }
        }


        /// <summary>
        /// Saves a temporary video file.
        /// </summary>
        /// <param name="videoBytes">The video bytes.</param>
        /// <param name="prefix">The prefix.</param>
        /// <returns></returns>
        public async Task<string> SaveTempVideoFile(OnnxVideo video, string prefix = default)
        {
            try
            {
                _logger?.LogInformation($"[SaveTempVideoFile] Saving temporary video file...");
                var tempVideoFile = GetTempFileName("mp4", prefix ?? "Video");
                await video.SaveAsync(tempVideoFile);
                var videoBytes = await File.ReadAllBytesAsync(tempVideoFile);
                _logger?.LogInformation($"[SaveTempVideoFile] Temporary video file saved, File: {tempVideoFile}");
                return tempVideoFile;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[SaveTempVideoFile] Error saving temporary video: {ex.Message}");
                return default;
            }
        }


        /// <summary>
        /// Deletes the temporary video file.
        /// </summary>
        /// <param name="videoResult">The video result.</param>
        /// <returns></returns>
        public Task DeleteTempVideoFile(IVideoResult videoResult)
        {
            try
            {
                if (string.IsNullOrEmpty(videoResult.FileName))
                    return Task.CompletedTask;

                _logger?.LogInformation($"[DeleteTempVideoFile] Deleting temporary video file: {videoResult.FileName}");
                Task.Run(() =>
                {
                    if (File.Exists(videoResult.FileName))
                    {
                        File.Delete(videoResult.FileName);
                        _logger?.LogInformation($"[DeleteTempVideoFile] Temporary video file deleted.");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[DeleteTempVideoFile] Error deleting temporary video: {ex.Message}");
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// Deletes the temporary video files.
        /// </summary>
        /// <param name="videoResults">The video results.</param>
        /// <returns></returns>
        public Task DeleteTempVideoFile(IEnumerable<IVideoResult> videoResults)
        {
            Task.WhenAll(videoResults.Select(DeleteTempVideoFile));
            return Task.CompletedTask;
        }


        /// <summary>
        /// Opens the video file dialog.
        /// </summary>
        /// <returns></returns>
        private async Task<string> OpenVideoFileDialogAsync()
        {
            var selectedsFileName = await _dialogService.OpenFileDialogAsync("Open Video", _settings.DirectoryVideo, "Video Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.flv;*.gif|Gif Images|*.gif|All Files|*.*");
            if (string.IsNullOrEmpty(selectedsFileName))
            {
                _logger?.LogInformation("[OpenVideoFileDialog] Open video file canceled");
                return null;
            }
            return selectedsFileName;
        }


        /// <summary>
        /// Saves the video file dialog.
        /// </summary>
        /// <param name="initialFilename">The initial filename.</param>
        /// <returns></returns>
        private async Task<string> SaveVideoFileDialogAsync(string initialFilename)
        {
            var saveFilename = await _dialogService.SaveFileDialogAsync("Save Video", $"video-{initialFilename}.mp4", _settings.DirectoryVideoSave, "mp4 files (*.mp4)|*.mp4", "mp4");
            if (string.IsNullOrEmpty(saveFilename))
            {
                _logger?.LogInformation("[SaveImageFileDialog] Saving image canceled");
                return null;
            }

            return saveFilename;
        }

        #endregion


        /// <summary>
        /// Deletes the temporary files.
        /// </summary>
        public async Task DeleteTempFiles()
        {
            try
            {
                await Task.Run(() =>
                {
                    if (Directory.Exists(App.TempDirectory))
                    {
                        _logger?.LogInformation($"[DeleteTempFiles] Deleting Temporary video files...");
                        var tempFiles = new List<string>();
                        tempFiles.AddRange(Directory.EnumerateFiles(App.TempDirectory, "Temp-*"));
                        tempFiles.AddRange(Directory.EnumerateFiles(App.TempDirectory, "Image-*"));
                        tempFiles.AddRange(Directory.EnumerateFiles(App.TempDirectory, "Video-*"));
                        tempFiles.AddRange(Directory.EnumerateFiles(App.TempDirectory, "VideoToVideo-*"));
                        tempFiles.AddRange(Directory.EnumerateFiles(App.TempDirectory, "VideoUpscale-*"));
                        tempFiles.AddRange(Directory.EnumerateFiles(App.TempDirectory, "VideoFeatureExtractor-*"));
                        foreach (var videoFile in tempFiles)
                        {
                            File.Delete(videoFile);
                            _logger?.LogInformation($"[DeleteTempFiles] Deleting temporary video file: {videoFile}");
                        }
                    }
                    _logger?.LogInformation($"[DeleteTempFiles] Temporary video file deleted.");
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[DeleteTempFiles] Error deleting temporary files: {ex.Message}");
            }
        }


        /// <summary>
        /// Gets a random filename
        /// </summary>
        /// <param name="ext">The ext.</param>
        /// <param name="prefix">The prefix.</param>
        /// <returns></returns>
        public string GetRandomFileName(string ext, string prefix = default)
        {
            return $"{prefix ?? "Temp"}-{DateTime.Now.Ticks}.{ext}";
        }


        /// <summary>
        /// Gets a temporary filename.
        /// </summary>
        /// <param name="ext">The ext.</param>
        /// <param name="prefix">The prefix.</param>
        /// <returns></returns>
        public string GetTempFileName(string ext, string prefix = default)
        {
            if (!Directory.Exists(App.TempDirectory))
                Directory.CreateDirectory(App.TempDirectory);
            return Path.Combine(App.TempDirectory, GetRandomFileName(ext, prefix ?? "Temp"));
        }
    }
}
