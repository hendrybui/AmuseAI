using Amuse.UI.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Amuse.UI.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : SettingsViewBase, INavigatable
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsView"/> class.
        /// </summary>
        public SettingsView()
        {
            InitializeComponent();
        }


        public Task NavigateAsync(IImageResult imageResult)
        {
            throw new NotImplementedException();
        }


        public Task NavigateAsync(IVideoResult videoResult)
        {
            throw new NotImplementedException();
        }
 

        protected override async Task SaveAsync()
        {
            var modelSets = Settings.StableDiffusionModelSets.Select(x => x.ModelSet);
            foreach (var modelSet in modelSets)
            {
                modelSet.DeviceId = Settings.DefaultExecutionDevice.DeviceId;
                modelSet.ExecutionProvider = Settings.DefaultExecutionDevice.Provider;
            }

            var sontrolNets = Settings.ControlNetModelSets.Select(x => x.ModelSet);
            foreach (var modelSet in sontrolNets)
            {
                modelSet.DeviceId = Settings.DefaultExecutionDevice.DeviceId;
                modelSet.ExecutionProvider = Settings.DefaultExecutionDevice.Provider;
            }

            var upscalers = Settings.UpscaleModelSets.Select(x => x.ModelSet);
            foreach (var modelSet in upscalers)
            {
                modelSet.DeviceId = Settings.DefaultExecutionDevice.DeviceId;
                modelSet.ExecutionProvider = Settings.DefaultExecutionDevice.Provider;
            }

            var featureExtractors = Settings.FeatureExtractorModelSets.Select(x => x.ModelSet);
            foreach (var modelSet in featureExtractors)
            {
                modelSet.DeviceId = Settings.DefaultExecutionDevice.DeviceId;
                modelSet.ExecutionProvider = Settings.DefaultExecutionDevice.Provider;
            }

            await base.SaveAsync();
        }
    }
}
