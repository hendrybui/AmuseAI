using Amuse.UI.Models.StableDiffusion;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Amuse.UI.Services
{
    public interface IPreviewService
    {
        Task UnloadAsync();
        Task LoadAsync(StableDiffusionModelSetJson stableDiffusionModelSet);
        Task<BitmapSource> GeneratePreview(DenseTensor<float> latent);
    }
}
