using Amuse.UI.Models.FeatureExtractor;
using Amuse.UI.Models.StableDiffusion;
using Amuse.UI.Models.Upscale;

namespace Amuse.UI.Services
{
    public interface IModelFactory
    {
        UpscaleModelJson CreateUpscaleModelSet(string name, string filename, string modelTemplateType);
        UpscaleModelJson CreateUpscaleModelSet(string name, string filename, UpscaleModelTemplate modelTemplate);

        StableDiffusionModelSetJson CreateStableDiffusionModelSet(string name, string folder, string modelTemplateType);
        StableDiffusionModelSetJson CreateStableDiffusionModelSet(string name, string folder, StableDiffusionModelTemplate modelTemplate);

        ControlNetModelJson CreateControlNetModelSet(string name, string filename, string modelTemplateType);
        ControlNetModelJson CreateControlNetModelSet(string name, string filename, ControlNetModelTemplate modelTemplate);

        FeatureExtractorModelJson CreateFeatureExtractorModelSet(string name, string filename, string modelTemplateType);
        FeatureExtractorModelJson CreateFeatureExtractorModelSet(string name, string filename, FeatureExtractorModelTemplate modelTemplate);
    }
}