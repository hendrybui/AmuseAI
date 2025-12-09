using Microsoft.ML.OnnxRuntime.Tensors;
using OnnxStack.Core;
using OnnxStack.Core.Image;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Amuse.UI.Models.FeatureExtractor;
using Amuse.UI.Models;
using System.ComponentModel.DataAnnotations;

namespace Amuse.UI
{
    public static class Utils
    {
        /// <summary>
        /// Gets the application version.
        /// </summary>
        /// <returns></returns>
        public static string GetAppVersion()
        {
            var version = Assembly.GetEntryAssembly().GetName().Version;
            return $"v{version.Major}.{version.Minor}.{version.Build}";
        }


        /// <summary>
        /// Gets the display version.
        /// </summary>
        /// <returns></returns>
        public static string GetDisplayVersion()
        {
            var version = Assembly.GetEntryAssembly().GetName().Version;
            return $"v{version.Major}.{version.Minor}.{version.Build}";
        }


        /// <summary>
        /// Navigates to URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        public static void NavigateToUrl(string url)
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }


        /// <summary>
        /// Converts OnnxImage to BitmapSource async.
        /// </summary>
        /// <param name="onnxImage">The OnnxImage.</param>
        /// <returns></returns>
        public static async Task<BitmapSource> ToBitmapAsync(this OnnxImage onnxImage)
        {
            return await App.UIInvokeAsync(onnxImage.ToWriteableBitmapAsync);
        }


        /// <summary>
        /// Converts OnnxImage to WriteableBitmap.
        /// </summary>
        /// <param name="image">The image.</param>
        private static Task<WriteableBitmap> ToWriteableBitmapAsync(this OnnxImage image)
        {
            int width = image.Width;
            int height = image.Height;
            var writeableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            writeableBitmap.Lock();

            try
            {
                var buffer = writeableBitmap.BackBuffer;
                var stride = writeableBitmap.BackBufferStride;
                image.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < height; y++)
                    {
                        var rowPtr = buffer + y * stride;
                        var pixelRow = accessor.GetRowSpan(y);
                        for (int x = 0; x < width; x++)
                        {
                            var pixel = pixelRow[x];
                            int bgra = (pixel.R << 16) | (pixel.G << 8) | (pixel.B << 0) | (pixel.A << 24);
                            Marshal.WriteInt32(rowPtr, x * 4, bgra);
                        }
                    }
                });
                writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            }
            finally
            {
                writeableBitmap.Unlock();
            }
            return Task.FromResult(writeableBitmap);
        }



        /// <summary>
        /// Converts to BitmapImage.
        /// </summary>
        /// <param name="bitmapSource">The bitmap source.</param>
        /// <returns></returns>
        public static BitmapImage ToBitmapImage(this BitmapSource bitmapSource)
        {
            BitmapImage bmImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(stream);
                bmImage.BeginInit();
                bmImage.CacheOption = BitmapCacheOption.OnLoad;
                bmImage.StreamSource = stream;
                bmImage.EndInit();
                bmImage.Freeze();
            }
            return bmImage;
        }


        /// <summary>
        /// Creates an empty BitmapImage.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        public static BitmapImage CreateEmptyBitmapImage(int width, int height)
        {
            var wbm = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            var bmImage = new BitmapImage();
            using (var stream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbm));
                encoder.Save(stream);
                bmImage.BeginInit();
                bmImage.CacheOption = BitmapCacheOption.OnLoad;
                bmImage.StreamSource = stream;
                bmImage.EndInit();
                bmImage.Freeze();
            }
            return bmImage;
        }


        /// <summary>
        /// Gets the BitmapSource bytes.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns></returns>
        public static byte[] GetImageBytes(this BitmapSource image)
        {
            if (image == null)
                return null;

            using (var stream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(stream);
                return stream.ToArray();
            }
        }


        /// <summary>
        /// Forces the notify collection changed event.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        public static void ForceNotifyCollectionChanged<T>(this ObservableCollection<T> collection)
        {
            // Hack: Moving an item will invoke a collection changed event
            collection?.Move(0, 0);
        }


        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public static bool Remove<T>(this ICollection<T> collection, Func<T, bool> predicate)
        {
            if (collection.IsNullOrEmpty())
                return false;

            var item = collection.FirstOrDefault(predicate);
            if (item is null)
                return false;

            return collection.Remove(item);
        }


        /// <summary>
        /// Removes all items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool RemoveAll<T>(this ICollection<T> collection, Func<T, bool> predicate)
        {
            if (collection.IsNullOrEmpty())
                return false;

            var items = collection.Where(predicate).ToList();
            if (items.Count == 0)
                return false;

            foreach (var item in items)
                collection.Remove(item);

            return true;
        }


        /// <summary>
        /// PerformanceCounter can be disposed between compare calls, so wrap in catch block for simplicity
        /// </summary>
        /// <param name="counter">The counter.</param>
        /// <returns></returns>
        public static float TryGetNextValue(this PerformanceCounter counter)
        {
            try
            {
                return counter.NextValue();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static readonly string _orientationQuery = "System.Photo.Orientation";

        public static BitmapSource LoadImageFile(string path)
        {
            var rotation = Rotation.Rotate0;
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var bitmapFrame = BitmapFrame.Create(fileStream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
                var bitmapMetadata = bitmapFrame.Metadata as BitmapMetadata;
                if ((bitmapMetadata != null) && (bitmapMetadata.ContainsQuery(_orientationQuery)))
                {
                    var queryResult = bitmapMetadata.GetQuery(_orientationQuery);
                    if (queryResult is ushort orientation)
                    {
                        switch (orientation)
                        {
                            case 6:
                                rotation = Rotation.Rotate90;
                                break;
                            case 3:
                                rotation = Rotation.Rotate180;
                                break;
                            case 8:
                                rotation = Rotation.Rotate270;
                                break;
                        }
                    }
                }
            }

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path);
            image.Rotation = rotation;
            image.EndInit();
            image.Freeze();
            return image;
        }


        /// <summary>
        /// Converts to bitmapimage.
        /// </summary>
        /// <param name="tensor">The tensor.</param>
        /// <returns>BitmapSource.</returns>
        public static Task<BitmapSource> ToBitmapAsync(this DenseTensor<float> tensor)
        {
            var width = tensor.Dimensions[3];
            var height = tensor.Dimensions[2];
            var channels = tensor.Dimensions[1];
            var normalizedData = Normalize(tensor, channels, height, width);
            var writeableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Rgb24, null);
            writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), normalizedData, width * 3, 0);
            return Task.FromResult<BitmapSource>(writeableBitmap);
        }


        private static byte[] Normalize(DenseTensor<float> imageTensor, int channels, int height, int width)
        {
            var tensorData = imageTensor.Buffer.Span;
            var normalizedData = new byte[width * height * 3];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int pixelIndex = i * width + j;
                    normalizedData[pixelIndex * 3 + 0] = Normalize(tensorData[pixelIndex]); // Red
                    normalizedData[pixelIndex * 3 + 1] = Normalize(tensorData[width * height + pixelIndex]); // Green
                    normalizedData[pixelIndex * 3 + 2] = Normalize(tensorData[2 * width * height + pixelIndex]); // Blue
                }
            }
            return normalizedData;
        }


        private static byte Normalize(float value)
        {
            // Map from [-1, 1] to [0, 255]
            return (byte)Math.Clamp(((value + 1) * 0.5f) * 255, 0, 255);
        }


        public static string GetDescription(this Enum enumObj)
        {
            var fieldInfo = enumObj.GetType().GetField(enumObj.ToString());
            var attribArray = fieldInfo.GetCustomAttributes(false);
            if (attribArray.Length > 0)
            {
                var displayAttribute = attribArray
                    .OfType<DisplayAttribute>()
                    .FirstOrDefault();
                if (displayAttribute != null)
                    return displayAttribute.Name;
            }
            return enumObj.ToString();
        }


        public static Guid InternalExtractorCanny = Guid.Parse("1C61393C-187C-4378-8635-C70126EB9204");
        public static Guid InternalExtractorSoftEdge = Guid.Parse("9F2785F9-5CA5-4B59-8384-0E805A0DC496");


        public static void AddInternalFeatureExtractors(AmuseSettings settings)
        {
            if (!settings.FeatureExtractorModelSets.Any(x => x.Id == InternalExtractorCanny))
            {
                settings.FeatureExtractorModelSets.Add(new FeatureExtractorModelSetViewModel
                {
                    Id = InternalExtractorCanny,
                    IsControlNetSupported = true,
                    ModelSet = new FeatureExtractorModelJson { Name = "Canny", OutputChannels = 1 }
                });
            }
            if (!settings.FeatureExtractorModelSets.Any(x => x.Id == InternalExtractorSoftEdge))
            {
                settings.FeatureExtractorModelSets.Add(new FeatureExtractorModelSetViewModel
                {
                    Id = InternalExtractorSoftEdge,
                    IsControlNetSupported = true,
                    ModelSet = new FeatureExtractorModelJson { Name = "SoftEdge", OutputChannels = 1 }
                });
            }
        }



        public static BitmapSource InvertColors(this BitmapSource source)
        {
            if (source.Format != PixelFormats.Pbgra32)
            {
                source = new FormatConvertedBitmap(source, PixelFormats.Pbgra32, null, 0);
            }

            var wb = new WriteableBitmap(source);
            wb.Lock();

            unsafe
            {
                int bytesPerPixel = (wb.Format.BitsPerPixel + 7) / 8;
                int stride = wb.BackBufferStride;
                byte* buffer = (byte*)wb.BackBuffer.ToPointer();

                for (int y = 0; y < wb.PixelHeight; y++)
                {
                    byte* row = buffer + y * stride;
                    for (int x = 0; x < wb.PixelWidth; x++)
                    {
                        byte* pixel = row + x * bytesPerPixel;

                        byte a = pixel[3];
                        if (a != 0)
                        {
                            pixel[0] = (byte)(255 - pixel[0]); // B
                            pixel[1] = (byte)(255 - pixel[1]); // G
                            pixel[2] = (byte)(255 - pixel[2]); // R
                                                               // A (pixel[3]) unchanged
                        }
                    }
                }
            }

            wb.AddDirtyRect(new Int32Rect(0, 0, wb.PixelWidth, wb.PixelHeight));
            wb.Unlock();

            return wb;
        }



        public static BitmapSource InvertMaskAlpha(this BitmapSource source)
        {
            if (source.Format != PixelFormats.Pbgra32)
            {
                source = new FormatConvertedBitmap(source, PixelFormats.Pbgra32, null, 0);
            }

            var wb = new WriteableBitmap(source);
            wb.Lock();

            unsafe
            {
                int bytesPerPixel = (wb.Format.BitsPerPixel + 7) / 8;
                int stride = wb.BackBufferStride;
                byte* buffer = (byte*)wb.BackBuffer.ToPointer();

                for (int y = 0; y < wb.PixelHeight; y++)
                {
                    byte* row = buffer + y * stride;
                    for (int x = 0; x < wb.PixelWidth; x++)
                    {
                        byte* pixel = row + x * bytesPerPixel;

                        byte a = pixel[3];
                        if (a > 0)
                        {
                            // Make transparent
                            pixel[0] = 0; // B
                            pixel[1] = 0; // G
                            pixel[2] = 0; // R
                            pixel[3] = 0; // A
                        }
                        else
                        {
                            // Make solid black
                            pixel[0] = 0;
                            pixel[1] = 0;
                            pixel[2] = 0;
                            pixel[3] = 255;
                        }
                    }
                }
            }

            wb.AddDirtyRect(new Int32Rect(0, 0, wb.PixelWidth, wb.PixelHeight));
            wb.Unlock();

            return wb;
        }
    }
}
