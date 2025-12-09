using Amuse.UI.Models;
using System.Threading;
using System;
using System.Threading.Tasks;

namespace Amuse.UI.Services
{
    public interface IModelDownloadService
    {
        /// <summary>
        /// Queues the model for download.
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        Task QueueDownloadAsync(ModelTemplateViewModel modelTemplate);

        /// <summary>
        /// Downloads the model.
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        /// <param name="directory">The directory.</param>
        /// <returns></returns>
        Task DownloadModelAsync(ModelTemplateViewModel modelTemplate, string directory);

        /// <summary>
        /// Downloads a file.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="directory">The directory.</param>
        /// <param name="progressCallback">The progress callback.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<DownloadFileInfo> DownloadFileAsync(string url, string directory, Action<string, double, double> progressCallback, CancellationToken cancellationToken = default);
    }
}