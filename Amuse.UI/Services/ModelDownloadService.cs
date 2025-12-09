using Amuse.UI.Helpers;
using Amuse.UI.Models;
using Amuse.UI.Models.FeatureExtractor;
using Amuse.UI.Models.StableDiffusion;
using Amuse.UI.Models.Upscale;
using Microsoft.Extensions.Logging;
using OnnxStack.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Amuse.UI.Services
{
    public class ModelDownloadService : IModelDownloadService
    {
        private readonly ILogger<ModelDownloadService> _logger;
        private readonly AmuseSettings _settings;
        private readonly IModelFactory _modelFactory;
        private readonly AsyncQueue<ModelTemplateViewModel> _downloadQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelDownloadService"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="modelFactory">The model factory.</param>
        /// <param name="logger">The logger.</param>
        public ModelDownloadService(AmuseSettings settings, IModelFactory modelFactory, ILogger<ModelDownloadService> logger)
        {
            _logger = logger;
            _settings = settings;
            _modelFactory = modelFactory;
            _downloadQueue = new AsyncQueue<ModelTemplateViewModel>(DownloadQueuedModelAsync);
        }


        /// <summary>
        /// Queues the model for download.
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        public Task QueueDownloadAsync(ModelTemplateViewModel modelTemplate)
        {
            if (modelTemplate.IsInstalled && !modelTemplate.IsUpdateAvailable)
                return Task.CompletedTask;

            modelTemplate.IsQueued = true;
            modelTemplate.ErrorMessage = null;
            modelTemplate.IsDownloading = true;
            _downloadQueue.QueueItem(modelTemplate);
            return Task.CompletedTask;
        }


        /// <summary>
        /// Download and install the ModelTemplate.
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        /// <param name="directory">The install directory.</param>
        public async Task DownloadModelAsync(ModelTemplateViewModel modelTemplate, string directory)
        {
            _logger.LogInformation("[DownloadModelAsync] - Downloading model: Name: {Name}, Id: {Id}", modelTemplate.Name, modelTemplate.Id);
            modelTemplate.IsQueued = false;
            modelTemplate.IsDownloading = true;
            modelTemplate.CancellationTokenSource = new CancellationTokenSource();
            modelTemplate.ErrorMessage = null;
            modelTemplate.ProgressValue = 1;
            modelTemplate.ProgressText = $"Starting Download...";

            try
            {
                var progressCallback = CreateProgressCallback(modelTemplate);
                var modelDownloadResult = await DownloadHttpAsync(modelTemplate, directory, progressCallback, modelTemplate.CancellationTokenSource.Token);
                if (modelTemplate.Category == ModelTemplateCategory.Upscaler)
                    await DownloadUpscaleModelComplete(modelTemplate, modelDownloadResult);
                if (modelTemplate.Category == ModelTemplateCategory.ControlNet)
                    await DownloadControlNetModelComplete(modelTemplate, modelDownloadResult);
                if (modelTemplate.Category == ModelTemplateCategory.StableDiffusion)
                    await DownloadStableDiffusionModelComplete(modelTemplate, modelDownloadResult);
                if (modelTemplate.Category == ModelTemplateCategory.FeatureExtractor)
                    await DownloadFeatureExtractorModelComplete(modelTemplate, modelDownloadResult);

                _logger.LogInformation("[DownloadModelAsync] - Downloading model complete, Model: {Name}", modelTemplate.Name);
            }
            catch (OperationCanceledException)
            {
                await HandleError(modelTemplate, "Download canceled");
                _logger.LogError("Download canceled, {Name}", modelTemplate.Name);
            }
            catch (Exception ex)
            {
                await HandleError(modelTemplate, ex.Message);
                _logger.LogError("An error occured downloading model {Name}\nError: {Message}", modelTemplate.Name, ex.Message);
            }
        }


        public async Task<DownloadFileInfo> DownloadFileAsync(string url, string directory, Action<string, double, double> progressCallback, CancellationToken cancellationToken = default)
        {
            var filename = Path.GetFileName(url.Split('?').First());
            var destination = Path.Combine(directory, filename);
            var downloadFileInfo = new DownloadFileInfo(url, destination);
            return await DownloadFileAsync(downloadFileInfo, progressCallback, cancellationToken);
        }


        /// <summary>
        /// Download queued model
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task DownloadQueuedModelAsync(ModelTemplateViewModel modelTemplate)
        {
            try
            {
                if (!modelTemplate.IsQueued)
                    return;
                if (modelTemplate.IsInstalled && !modelTemplate.IsUpdateAvailable)
                    return;

                await DownloadModelAsync(modelTemplate, _settings.DirectoryModel);
                await App.UIInvokeAsync(_settings.SaveAsync);
            }
            catch (Exception ex)
            {
                _logger.LogError("[DownloadQueuedModelAsync] - Error: {Message}", ex.Message);
            }
        }


        /// <summary>
        /// Completes the StableDiffusion model download
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        /// <param name="downloadResult">The download result.</param>
        private async Task DownloadStableDiffusionModelComplete(ModelTemplateViewModel modelTemplate, ModelDownloadResult downloadResult)
        {
            var modelSet = _modelFactory.CreateStableDiffusionModelSet(modelTemplate.Name, downloadResult.OutputDirectory, modelTemplate.StableDiffusionTemplate);
            var configurations = new OnnxModelJson[]
            {
                modelSet.UnetConfig,
                modelSet.Unet2Config,
                modelSet.TokenizerConfig,
                modelSet.Tokenizer2Config,
                modelSet.Tokenizer3Config,
                modelSet.TextEncoderConfig,
                modelSet.TextEncoder2Config,
                modelSet.TextEncoder3Config,
                modelSet.VaeDecoderConfig,
                modelSet.VaeEncoderConfig,
                modelSet.ControlNetUnetConfig
            };

            foreach (var configuration in configurations)
            {
                if (configuration == null)
                    continue;

                if (!File.Exists(configuration.OnnxModelPath))
                {
                    await HandleError(modelTemplate);
                    return;
                }
            }

            await App.UIInvokeAsync(() =>
            {
                _settings.StableDiffusionModelSets.Remove(x => x.Id == modelTemplate.Id);
                _settings.StableDiffusionModelSets.Add(new StableDiffusionModelSetViewModel
                {
                    Id = modelTemplate.Id,
                    ModelSet = modelSet,
                    Template = modelTemplate,
                });

                modelTemplate.IsInstalled = true;
                modelTemplate.IsDownloading = false;
                return Task.CompletedTask;
            });
        }


        /// <summary>
        /// Completes the StableDiffusion model download
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        /// <param name="downloadResult">The download result.</param>
        private async Task DownloadControlNetModelComplete(ModelTemplateViewModel modelTemplate, ModelDownloadResult downloadResult)
        {
            var modelFile = downloadResult.Files.FirstOrDefault(x => x.FileName.EndsWith("model.onnx"));
            var modelSet = _modelFactory.CreateControlNetModelSet(modelTemplate.Name, modelFile.FileName, modelTemplate.ControlNetTemplate);
            var isModelSetValid = File.Exists(modelSet.OnnxModelPath);
            if (!isModelSetValid)
            {
                await HandleError(modelTemplate);
                return;
            }

            await App.UIInvokeAsync(() =>
            {
                _settings.ControlNetModelSets.Remove(x => x.Id == modelTemplate.Id);
                _settings.ControlNetModelSets.Add(new ControlNetModelSetViewModel
                {
                    Id = modelTemplate.Id,
                    ModelSet = modelSet,
                    PipelineTypes = modelTemplate.ControlNetTemplate.PipelineTypes,
                    Template = modelTemplate
                });

                modelTemplate.IsInstalled = true;
                modelTemplate.IsDownloading = false;
                return Task.CompletedTask;
            });
        }


        /// <summary>
        /// Completes the StableDiffusion model download
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        /// <param name="downloadResult">The download result.</param>
        private async Task DownloadUpscaleModelComplete(ModelTemplateViewModel modelTemplate, ModelDownloadResult downloadResult)
        {
            var modelFile = downloadResult.Files.FirstOrDefault(x => x.FileName.EndsWith("model.onnx"));
            var modelSet = _modelFactory.CreateUpscaleModelSet(modelTemplate.Name, modelFile.FileName, modelTemplate.UpscaleTemplate);
            var isModelSetValid = File.Exists(modelSet.OnnxModelPath);
            if (!isModelSetValid)
            {
                await HandleError(modelTemplate);
                return;
            }

            await App.UIInvokeAsync(() =>
            {
                _settings.UpscaleModelSets.Remove(x => x.Id == modelTemplate.Id);
                _settings.UpscaleModelSets.Add(new UpscaleModelSetViewModel
                {
                    Id = modelTemplate.Id,
                    ModelSet = modelSet,
                    Template = modelTemplate
                });

                modelTemplate.IsInstalled = true;
                modelTemplate.IsDownloading = false;
                return Task.CompletedTask;
            });
        }


        /// <summary>
        /// Completes the StableDiffusion model download
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        /// <param name="downloadResult">The download result.</param>
        private async Task DownloadFeatureExtractorModelComplete(ModelTemplateViewModel modelTemplate, ModelDownloadResult downloadResult)
        {
            var modelFile = downloadResult.Files.FirstOrDefault(x => x.FileName.EndsWith("model.onnx"));
            var modelSet = _modelFactory.CreateFeatureExtractorModelSet(modelTemplate.Name, modelFile.FileName, modelTemplate.FeatureExtractorTemplate);
            var isModelSetValid = File.Exists(modelSet.OnnxModelPath);
            if (!isModelSetValid)
            {
                await HandleError(modelTemplate);
                return;
            }

            await App.UIInvokeAsync(() =>
            {
                _settings.FeatureExtractorModelSets.Remove(x => x.Id == modelTemplate.Id);
                _settings.FeatureExtractorModelSets.Add(new FeatureExtractorModelSetViewModel
                {
                    Id = modelTemplate.Id,
                    ModelSet = modelSet,
                    IsControlNetSupported = modelTemplate.FeatureExtractorTemplate.IsControlNetSupported,
                    Template = modelTemplate
                });

                modelTemplate.IsInstalled = true;
                modelTemplate.IsDownloading = false;
                return Task.CompletedTask;
            });
        }


        /// <summary>
        /// Handles the error response.
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        /// <param name="errorMessage">The error message.</param>
        private static async Task HandleError(ModelTemplateViewModel modelTemplate, string errorMessage = null)
        {
            await App.UIInvokeAsync(() =>
            {
                modelTemplate.ErrorMessage = errorMessage ?? "Error: Download completed but ModelSet is invalid";
                modelTemplate.IsQueued = false;
                modelTemplate.IsDownloading = false;
                modelTemplate.ProgressText = null;
                modelTemplate.ProgressValue = 0;
                return Task.CompletedTask;
            });
        }


        /// <summary>
        /// Creates the progress callback.
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        /// <returns></returns>
        private static Action<string, double, double> CreateProgressCallback(ModelTemplateViewModel modelTemplate)
        {
            return (f, fp, tp) =>
            {
                App.UIInvoke(() =>
                {
                    modelTemplate.ProgressText = $"{f}";
                    modelTemplate.ProgressValue = tp;
                });
            };
        }


        /// <summary>
        /// Downloads the model files.
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        /// <param name="outputDirectory">The output directory.</param>
        /// <param name="progressCallback">The progress callback.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="increment">The increment.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// Queried file headers returned 0 bytes
        /// or
        /// Error: {ex.Message}
        /// </exception>
        private Task<ModelDownloadResult> DownloadHttpAsync(ModelTemplateViewModel modelTemplate, string outputDirectory, Action<string, double, double> progressCallback, CancellationToken cancellationToken = default)
        {
            return Task.Run(async () =>
            {
                ArgumentException.ThrowIfNullOrEmpty(nameof(modelTemplate.Repository));

                var repositoryName = modelTemplate.Repository.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                var modelDirectory = Path.Combine(outputDirectory, repositoryName);
                if (modelTemplate.IsUpdateAvailable)
                {
                    if (Directory.Exists(modelDirectory))
                        Directory.Delete(modelDirectory, true);
                }

                var modelFiles = GetFileInfo(modelTemplate.RepositoryFiles, modelDirectory);
                if (modelFiles.All(x => x.Exists))
                {
                    _logger.LogInformation("[DownloadHttpAsync] - All files exist on local drive, skipping download, Model: {Name}", modelTemplate.Name);
                    return new ModelDownloadResult(modelTemplate.Id, modelDirectory, modelFiles);
                }

                var remainingFiles = modelFiles.Where(x => !x.Exists).ToList();
                using (var httpClient = new HttpClient())
                {
                    var totalDownloadSize = await GetTotalSizeFromHeadersAsync(remainingFiles, httpClient, cancellationToken);
                    if (totalDownloadSize == 0)
                    {
                        throw new Exception("Queried file headers returned 0 bytes");
                    }

                    var increment = GetIncrement(modelTemplate.DownloadSize);
                    var totalBytes = (modelTemplate.DownloadSize * 1024 * 1024 * 1024);
                    var totalBytesRead = totalBytes - totalDownloadSize;
                    foreach (var file in remainingFiles)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        try
                        {
                            var lastProgress = 0d;
                            long existingFileSize = 0;
                            var tempFilename = $"{file.FileName}.download";
                            if (File.Exists(tempFilename))
                            {
                                FileInfo fileInfo = new FileInfo(tempFilename);
                                existingFileSize = fileInfo.Length;
                                totalBytesRead += existingFileSize;
                            }

                            Directory.CreateDirectory(Path.GetDirectoryName(tempFilename));
                            using (FileStream fileStream = new FileStream(tempFilename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                            {
                                httpClient.DefaultRequestHeaders.Range = null;
                                if (existingFileSize > 0)
                                {
                                    fileStream.Seek(existingFileSize, SeekOrigin.Begin);
                                    httpClient.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(existingFileSize, null);
                                }

                                using (var response = await httpClient.GetAsync(file.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                                {
                                    response.EnsureSuccessStatusCode();

                                    var fileBytesRead = 0;
                                    var fileBuffer = new byte[8192];
                                    var fileSize = existingFileSize + response.Content.Headers.ContentLength ?? -1;
                                    using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                                    {
                                        while (true)
                                        {
                                            cancellationToken.ThrowIfCancellationRequested();
                                            var readSize = await contentStream.ReadAsync(fileBuffer, cancellationToken);
                                            if (readSize == 0)
                                                break;

                                            await fileStream.WriteAsync(fileBuffer.AsMemory(0, readSize), cancellationToken);

                                            fileBytesRead += readSize;
                                            totalBytesRead += readSize;
                                            var fileProgress = Math.Round(fileBytesRead * 100.0 / fileSize, 3);
                                            var totalProgressValue = Math.Round(totalBytesRead * 100.0 / totalBytes, 3);
                                            if (totalProgressValue > lastProgress || totalProgressValue >= 100)
                                            {
                                                lastProgress = totalProgressValue + increment;
                                                progressCallback?.Invoke(file.Url, fileProgress, totalProgressValue);
                                            }
                                        }
                                    }
                                }
                            }

                            // File Complete, Rename
                            File.Move(tempFilename, file.FileName, true);
                            file.Exists = File.Exists(file.FileName);
                            _logger.LogInformation("[DownloadHttpAsync] - Download Success, Model: {Name}, File:{FileName}", modelTemplate.Name, file.FileName);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Model download failed: {ex.Message}");
                        }
                    }

                    // Model Download Complete
                    modelTemplate.IsUpdateAvailable = false;
                    return new ModelDownloadResult(modelTemplate.Id, modelDirectory, remainingFiles);
                }
            });
        }


        private Task<DownloadFileInfo> DownloadFileAsync(DownloadFileInfo file, Action<string, double, double> progressCallback, CancellationToken cancellationToken = default, double increment = 1)
        {
            return Task.Run(async () =>
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        var totalDownloadSize = await GetTotalSizeFromHeadersAsync(file, httpClient, cancellationToken);
                        if (totalDownloadSize == 0)
                            throw new Exception("Queried file headers returned 0 bytes");

                        var totalBytesRead = 0L;
                        using (var response = await httpClient.GetAsync(file.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                        {
                            response.EnsureSuccessStatusCode();
                            var fileSize = response.Content.Headers.ContentLength ?? -1;
                            var canReportProgress = fileSize != -1 && progressCallback != null;
                            var buffer = new byte[8192];
                            var bytesRead = 0;

                            var lastProgress = 0d;
                            using (var fileStream = File.Create(file.FileName))
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            {
                                while (true)
                                {
                                    cancellationToken.ThrowIfCancellationRequested();

                                    var readSize = await stream.ReadAsync(buffer, 0, buffer.Length);
                                    if (readSize == 0)
                                        break;

                                    await fileStream.WriteAsync(buffer, 0, readSize);
                                    totalBytesRead += readSize;
                                    bytesRead += readSize;

                                    if (canReportProgress)
                                    {
                                        var fileProgress = Math.Round((bytesRead * 100.0 / fileSize), 3);
                                        var totalProgressValue = Math.Round((totalBytesRead * 100.0 / totalDownloadSize), 3);
                                        if (totalProgressValue > lastProgress || totalProgressValue >= 100)
                                        {
                                            lastProgress = totalProgressValue + increment;
                                            progressCallback?.Invoke(file.Url, fileProgress, totalProgressValue);
                                        }
                                    }
                                }
                            }

                            file.Exists = File.Exists(file.FileName);
                            _logger.LogInformation("[DownloadHttpAsync] - Download Success, File:{FileName}", file.FileName);
                            return file;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[DownloadFileAsync] - An Exception occured downloading file, File:{FileName}", file.FileName);
                    TryDelete(file.FileName);
                    throw new Exception($"Model download error: {ex.Message}");
                }
            });
        }

        private static double GetIncrement(double gigabytes)
        {
            if (gigabytes > 20)
                return 0.05;
            if (gigabytes > 10)
                return 0.1;
            if (gigabytes > 5)
                return 0.2;

            return 0.3;
        }


        private static async Task<long> GetTotalSizeFromHeadersAsync(DownloadFileInfo file, HttpClient httpClient, CancellationToken cancellationToken)
        {
            return await GetTotalSizeFromHeadersAsync([file], httpClient, cancellationToken);
        }


        /// <summary>
        /// Gets the total size from headers.
        /// </summary>
        /// <param name="fileList">The file list.</param>
        /// <param name="httpClient">The HTTP client.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Failed to query file headers, {ex.Message}</exception>
        private static async Task<long> GetTotalSizeFromHeadersAsync(IEnumerable<DownloadFileInfo> fileList, HttpClient httpClient, CancellationToken cancellationToken)
        {
            var totalDownloadSize = 0L;
            foreach (var file in fileList)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using (var response = await httpClient.GetAsync(file.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                    {
                        response.EnsureSuccessStatusCode();
                        totalDownloadSize += response.Content.Headers.ContentLength ?? 0;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error: {ex.Message}\nUrl: {file.Url}");
                }
            }
            return totalDownloadSize;
        }


        /// <summary>
        /// Tries the delete.
        /// </summary>
        /// <param name="filename">The filename.</param>
        private static void TryDelete(string filename)
        {
            try
            {
                if (File.Exists(filename))
                    File.Delete(filename);
            }
            catch (Exception)
            {
                // LOG ME
            }
        }


        /// <summary>
        /// Gets the file input and output targets.
        /// </summary>
        /// <param name="urlFileList">The URL file list.</param>
        /// <param name="outputDirectory">The output directory.</param>
        /// <returns></returns>
        private static List<DownloadFileInfo> GetFileInfo(IEnumerable<string> urlFileList, string outputDirectory)
        {
            var files = new List<DownloadFileInfo>();
            var repositoryUrls = urlFileList.Select(x => new Uri(x)).ToList();
            var baseUrlSegmentLength = GetBaseUrlSegmentLength(repositoryUrls);
            foreach (var repositoryUrl in repositoryUrls)
            {
                var filename = repositoryUrl.Segments.Last().Trim('\\', '/');
                var subFolder = Path.Combine(repositoryUrl.Segments
                    .Where(x => x != repositoryUrl.Segments.Last())
                    .Select(x => x.Trim('\\', '/'))
                    .Skip(baseUrlSegmentLength)
                    .ToArray()) ?? string.Empty;
                var destination = Path.Combine(outputDirectory, subFolder);
                var destinationFile = Path.Combine(destination, filename);

                Directory.CreateDirectory(destination);
                files.Add(new DownloadFileInfo(repositoryUrl.OriginalString, destinationFile, File.Exists(destinationFile)));
            }
            return files;
        }


        /// <summary>
        /// Gets the length of the base URL segment.
        /// </summary>
        /// <param name="repositoryUrls">The repository urls.</param>
        /// <returns></returns>
        private static int GetBaseUrlSegmentLength(List<Uri> repositoryUrls)
        {
            var minUrlSegmentLength = repositoryUrls.Select(x => x.Segments.Length).Min();
            for (int i = 0; i < minUrlSegmentLength; i++)
            {
                if (repositoryUrls.Select(x => x.Segments[i]).Distinct().Count() > 1)
                {
                    return i;
                }
            }
            return minUrlSegmentLength;
        }
    }

    public record ModelDownloadResult(Guid ModelId, string OutputDirectory, List<DownloadFileInfo> Files);

    public record DownloadFileInfo(string Url, string FileName)
    {
        public DownloadFileInfo(string Url, string FileName, bool exists) : this(Url, FileName)
        {
            Exists = exists;
        }

        public bool Exists { get; set; }
    }

}
