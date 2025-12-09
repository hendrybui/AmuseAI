using Amuse.UI.Models.FeatureExtractor;
using Amuse.UI.Models.StableDiffusion;
using Amuse.UI.Models.Upscale;
using OnnxStack.FeatureExtractor.Pipelines;
using OnnxStack.StableDiffusion.Models;
using OnnxStack.StableDiffusion.Pipelines;
using System.Threading.Tasks;

namespace Amuse.UI.Services
{
    public interface IModelCacheService
    {
        Task<ControlNetModel> LoadModelAsync(ControlNetModelSetViewModel model);
        Task<FeatureExtractorPipeline> LoadModelAsync(FeatureExtractorModelSetViewModel model);
        Task<IPipeline> LoadModelAsync(StableDiffusionModelSetViewModel model, bool isControlNet);
        Task<ImageUpscalePipeline> LoadModelAsync(UpscaleModelSetViewModel model);
        Task<ContentFilterPipeline> LoadModelAsync(ContentFilterModelSetViewModel model);

        Task<bool> UnloadModelAsync(FeatureExtractorModelSetViewModel model);
        Task<bool> UnloadModelAsync(StableDiffusionModelSetViewModel model);
        Task<bool> UnloadModelAsync(UpscaleModelSetViewModel model);
        Task<bool> UnloadModelAsync(ControlNetModelSetViewModel model);
        Task<bool> UnloadModelAsync(ContentFilterModelSetViewModel model);

        bool IsModelLoaded(StableDiffusionModelSetViewModel model);
        bool IsModelLoaded(ControlNetModelSetViewModel model);
        bool IsModelLoaded(UpscaleModelSetViewModel model);
        bool IsModelLoaded(FeatureExtractorModelSetViewModel model);
        bool IsModelLoaded(ContentFilterModelSetViewModel model);
    }
}