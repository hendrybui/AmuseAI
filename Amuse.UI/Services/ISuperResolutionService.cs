using OnnxStack.Core.Image;
using OnnxStack.Core.Video;
using System.Threading;
using System.Threading.Tasks;

namespace Amuse.UI.Services
{
    public interface ISuperResolutionService
    {
        bool IsSupported { get; }
        bool IsLoaded { get; }

        Task LoadAsync();
        Task UnloadAsync();
        Task<OnnxImage> RunAsync(OnnxImage image, CancellationToken cancellationToken = default);
        Task<OnnxVideo> RunAsync(OnnxVideo video, CancellationToken cancellationToken = default);
    }
}
