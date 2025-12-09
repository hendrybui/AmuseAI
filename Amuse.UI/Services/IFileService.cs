using Amuse.UI.Models;
using OnnxStack.Core.Image;
using OnnxStack.Core.Video;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Amuse.UI.Services
{
    public interface IFileService
    {
        /// <summary>
        /// Opens the image file.
        /// </summary>
        /// <returns></returns>
        Task<ImageInput> OpenImageFile(string initialImageFile = default);

        /// <summary>
        /// Opens the image file via the Crop dialog.
        /// </summary>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <param name="initialImage">The initial image.</param>
        /// <param name="initialImageFile">The initial image file.</param>
        /// <returns></returns>
        Task<ImageInput> OpenImageFileCropped(int maxWidth, int maxHeight, BitmapSource initialImage = null, string initialImageFile = default);

        /// <summary>
        /// Opens a video file.
        /// </summary>
        /// <returns></returns>
        Task<VideoInputModel> OpenVideoFile();

        /// <summary>
        /// Saves the image file.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        Task SaveImageFile(BitmapSource image, string filename);

        /// <summary>
        /// Saves image file via FilePicker dialog.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns></returns>
        Task SaveAsImageFile(BitmapSource image);

        /// <summary>
        /// Saves the image file.
        /// </summary>
        /// <param name="imageResult">The image result.</param>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        Task SaveImageFile(IImageResult imageResult, string filename);

        /// <summary>
        /// Saves image file via FilePicker dialog.
        /// </summary>
        /// <param name="imageResult">The image result.</param>
        /// <returns></returns>
        Task SaveAsImageFile(IImageResult imageResult);

        /// <summary>
        /// Automaticly the save image file.
        /// </summary>
        /// <param name="imageResult">The image result.</param>
        /// <param name="prefix">The prefix.</param>
        /// <returns></returns>
        Task AutoSaveImageFile(IImageResult imageResult, string prefix);

        /// <summary>
        /// Saves the temporary image file.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="prefix">The prefix.</param>
        /// <returns></returns>
        Task<string> SaveTempImageFile(OnnxImage image, string prefix = default);

        /// <summary>
        /// Saves the video file.
        /// </summary>
        /// <param name="videoResult">The video result.</param>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        Task SaveVideoFile(IVideoResult videoResult, string filename);

        /// <summary>
        /// Saves the video file.
        /// </summary>
        /// <param name="video">The video.</param>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        Task SaveVideoFile(OnnxVideo video, string filename);

        /// <summary>
        /// Saves the video file via FilePicker dialog.
        /// </summary>
        /// <param name="videoResult">The video result.</param>
        /// <returns></returns>
        Task SaveAsVideoFile(IVideoResult videoResult);

        /// <summary>
        /// Saves the video file via FilePicker dialog.
        /// </summary>
        /// <param name="video">The video result.</param>
        /// <returns></returns>
        Task SaveAsVideoFile(OnnxVideo video);

        /// <summary>
        /// Automaticly the save video file.
        /// </summary>
        /// <param name="videoResult">The video result.</param>
        /// <param name="prefix">The prefix.</param>
        /// <returns></returns>
        Task AutoSaveVideoFile(IVideoResult videoResult, string prefix);


        /// <summary>
        /// Deletes the temporary files.
        /// </summary>
        /// <returns></returns>
        Task DeleteTempFiles();

        /// <summary>
        /// Saves a temporary video file.
        /// </summary>
        /// <param name="videoBytes">The video bytes.</param>
        /// <param name="prefix">The prefix.</param>
        /// <returns></returns>
        Task<string> SaveTempVideoFile(OnnxVideo video, string prefix = default);

        /// <summary>
        /// Deletes the temporary video file.
        /// </summary>
        /// <param name="videoResult">The video result.</param>
        /// <returns></returns>
        Task DeleteTempVideoFile(IVideoResult videoResult);

        /// <summary>
        /// Deletes the temporary video files.
        /// </summary>
        /// <param name="videoResults">The video results.</param>
        /// <returns></returns>
        Task DeleteTempVideoFile(IEnumerable<IVideoResult> videoResults);

        /// <summary>
        /// Gets the name of the temporary file.
        /// </summary>
        /// <param name="ext">The ext.</param>
        /// <param name="prefix">The prefix.</param>
        /// <returns></returns>
        string GetTempFileName(string ext, string prefix = default);

        /// <summary>
        /// Gets the random name of the file.
        /// </summary>
        /// <param name="ext">The ext.</param>
        /// <param name="prefix">The prefix.</param>
        /// <returns></returns>
        string GetRandomFileName(string ext, string prefix = default);
    }
}