using Amuse.UI.Commands;
using Amuse.UI.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Amuse.UI.Windows
{
    /// <summary>
    /// Interaction logic for GenerateWindow.xaml
    /// </summary>
    public partial class GenerateWindow : BaseWindow
    {
        public GenerateWindow(AmuseSettings settings, ILogger<GenerateWindow> logger)
        {
            Logger = logger;
            Settings = settings;
            IsLogCommandEnabled = true;
            IsUIModeCommandEnabled = true;
            NavigateCommand = new AsyncRelayCommand<NavigationModel>(NavigateAsync);
            InitializeComponent();
        }

        public AsyncRelayCommand<NavigationModel> NavigateCommand { get; }

        private Task NavigateAsync(NavigationModel model)
        {
            return Task.CompletedTask;
        }
    }


}
