using Amuse.UI.Models;
using Microsoft.Extensions.Logging;
using OnnxStack.Core.Image;
using OnnxStack.Core.Video;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Amuse.UI.Services
{
    public class SuperResolutionService : ISuperResolutionService
    {
        private readonly AmuseSettings _settings;
        private readonly IDeviceService _deviceService;
        private readonly ILogger<ISuperResolutionService> _logger;
        private bool _isLoaded;

        public SuperResolutionService(AmuseSettings settings, IDeviceService deviceService, ILogger<SuperResolutionService> logger)
        {
            _logger = logger;
            _settings = settings;
            _deviceService = deviceService;
        }

        public bool IsSupported => _deviceService.IsNPUSuperResolutionSupported;

        public bool IsLoaded => _isLoaded;


        /// <summary>
        /// Loads NativeRyzenAI
        /// </summary>
        /// <returns>Task.</returns>
        public Task LoadAsync()
        {
            try
            {
                if (IsSupported)
                {
                    NativeRyzenAI.Initialize("NPU");
                    _isLoaded = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load NativeRyzenAI");
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// Unloads NativeRyzenAI
        /// </summary>
        /// <returns>Task.</returns>
        public Task UnloadAsync()
        {
            try
            {
                if (IsSupported)
                {
                    NativeRyzenAI.Shutdown();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unload NativeRyzenAI");
            }
            _isLoaded = false;
            return Task.CompletedTask;
        }


        /// <summary>
        /// Run the SuperResolution process
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A Task&lt;OnnxImage&gt; representing the asynchronous operation.</returns>
        public async Task<OnnxImage> RunAsync(OnnxImage image, CancellationToken cancellationToken = default)
        {
            var inputSize = GetNearestResolution(image);
            using (var img = image.GetImage().CloneAs<Rgb24>())
            using (var paddedImage = CreatePaddedImage(img, inputSize))
            {
                var resultImage = await ExecuteImageAsync(paddedImage);
                var croppedImage = CreateCroppedImage(img, paddedImage, resultImage, inputSize);
                return new OnnxImage(croppedImage.CloneAs<Rgba32>());
            }
        }


        /// <summary>
        /// Run the SuperResolution process
        /// </summary>
        /// <param name="video">The video.</param>
        /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A Task&lt;OnnxVideo&gt; representing the asynchronous operation.</returns>
        public async Task<OnnxVideo> RunAsync(OnnxVideo video, CancellationToken cancellationToken = default)
        {
            var frames = new OnnxImage[video.FrameCount];
            for (int i = 0; i < video.Frames.Count; i++)
            {
                var frame = video.Frames[i];
                frames[i] = await RunAsync(frame, cancellationToken);
            }
            return new OnnxVideo(frames, video.FrameRate);
        }


        /// <summary>
        /// Executes SuperResolution.
        /// </summary>
        /// <param name="inputWidth">Width of the input.</param>
        /// <param name="inputHeight">Height of the input.</param>
        /// <param name="inputData">The input data.</param>
        /// <param name="outputWidth">Width of the output.</param>
        /// <param name="outputHeight">Height of the output.</param>
        /// <param name="outputData">The output data.</param>
        private static Task ExecuteAsync(int inputWidth, int inputHeight, nint inputData, int outputWidth, int outputHeight, nint outputData)
        {
            return Task.Run(() => NativeRyzenAI.SuperResolution(inputWidth, inputHeight, inputData, outputWidth, outputHeight, outputData));
        }


        /// <summary>
        /// Execute SuperResolution Image
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>A Task&lt;Image`1&gt;</returns>
        private async Task<Image<Rgb24>> ExecuteImageAsync(Image<Rgb24> image)
        {
            var outputWidth = image.Width * 2;
            var outputHeight = image.Height * 2;
            var imageBytes = GetBytesFromImage(image);
            var imagePointer = Marshal.UnsafeAddrOfPinnedArrayElement(imageBytes, 0);
            var outputPointer = Marshal.AllocHGlobal(outputWidth * outputHeight * sizeof(int));
            await ExecuteAsync(image.Width, image.Height, imagePointer, outputWidth, outputHeight, outputPointer);

            var outputImage = GetImageFromPointer(outputPointer, outputWidth, outputHeight);
            Marshal.FreeHGlobal(outputPointer);
            return outputImage;
        }


        /// <summary>
        /// Gets the bytes from image.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>System.Byte[].</returns>
        private static byte[] GetBytesFromImage(Image<Rgb24> input)
        {
            var pixelSize = Unsafe.SizeOf<Rgb24>();
            var totalBytes = input.Width * input.Height * pixelSize;
            var pixelBuffer = new byte[totalBytes];
            input.CopyPixelDataTo(pixelBuffer);
            return pixelBuffer;
        }


        /// <summary>
        /// Gets the image from pointer.
        /// </summary>
        /// <param name="imagePtr">The image PTR.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>Image&lt;Rgb24&gt;.</returns>
        private static Image<Rgb24> GetImageFromPointer(nint imagePtr, int width, int height)
        {
            var bytes = new byte[width * height * 3];
            Marshal.Copy(imagePtr, bytes, 0, bytes.Length);

            var outputImage = new Image<Rgb24>(width, height);
            outputImage.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    var pixelRow = accessor.GetRowSpan(y);
                    for (int x = 0; x < pixelRow.Length; x++)
                    {
                        pixelRow[x] = new Rgb24(bytes[(y * accessor.Width + x) * 3 + 0],  // R
                                                bytes[(y * accessor.Width + x) * 3 + 1],  // G
                                                bytes[(y * accessor.Width + x) * 3 + 2]); // B
                    }
                }
            });
            return outputImage;
        }


        /// <summary>
        /// Creates the padded image.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>Image&lt;Rgb24&gt;.</returns>
        private static Image<Rgb24> CreatePaddedImage(Image<Rgb24> image, SizeOrientation inputSize)
        {
            var paddedImage = new Image<Rgb24>(inputSize.Width, inputSize.Height);
            paddedImage.Mutate(ctx =>
            {
                ctx.DrawImage(image, 1);
                ctx.Rotate(inputSize.Rotation);
            });
            return paddedImage;
        }


        /// <summary>
        /// Creates the cropped image.
        /// </summary>
        /// <param name="originalImage">The original image.</param>
        /// <param name="paddedImage">The padded image.</param>
        /// <param name="resultImage">The result image.</param>
        /// <returns>Image&lt;Rgb24&gt;.</returns>
        private static Image<Rgb24> CreateCroppedImage(Image<Rgb24> originalImage, Image<Rgb24> paddedImage, Image<Rgb24> resultImage, SizeOrientation inputSize)
        {
            var diff = (double)resultImage.Width / paddedImage.Width;
            var outputSize = new Size((int)(originalImage.Width * diff), (int)(originalImage.Height * diff));
            resultImage.Mutate(ctx =>
            {
                ctx.Rotate(inputSize.Rotation == RotateMode.Rotate90 ? RotateMode.Rotate270 : RotateMode.None);
                ctx.Crop(outputSize.Width, outputSize.Height);
            });
            return resultImage;
        }


        /// <summary>
        /// Gets the nearest resolution.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>Size.</returns>
        /// <exception cref="System.Exception">Unsupported image size: {image.Width}x{image.Height}, Maximum: 1920x1080</exception>
        private static SizeOrientation GetNearestResolution(OnnxImage image)
        {
            if (image.Height <= 540 && image.Width <= 960)
                return new SizeOrientation(960, 540, RotateMode.None);
            if (image.Height <= 960 && image.Width <= 540)
                return new SizeOrientation(540, 960, RotateMode.Rotate90);
            else if (image.Height <= 720 && image.Width <= 1280)
                return new SizeOrientation(1280, 720, RotateMode.None);
            else if (image.Height <= 1280 && image.Width <= 720)
                return new SizeOrientation(720, 1280, RotateMode.Rotate90);
            else if (image.Height <= 1080 && image.Width <= 1920)
                return new SizeOrientation(1920, 1080, RotateMode.None);
            else if (image.Height <= 1920 && image.Width <= 1080)
                return new SizeOrientation(1080, 1920, RotateMode.Rotate90);

            throw new Exception($"Unsupported image size: {image.Width}x{image.Height}, Maximum: 1920x1080");
        }
    }

    public record SizeOrientation(int Width, int Height, RotateMode Rotation);

    public static class NativeRyzenAI
    {
        const string DLL_Path = "Plugins\\SuperResolution\\ryzen-ai-platform-invoke.dll";

        [DllImport(DLL_Path, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Initialize(string backend_device);
        [DllImport(DLL_Path, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Shutdown();
        [DllImport(DLL_Path, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SuperResolution(int input_width, int input_height, IntPtr input_data, int output_width, int output_height, IntPtr output_data);
    }
}
