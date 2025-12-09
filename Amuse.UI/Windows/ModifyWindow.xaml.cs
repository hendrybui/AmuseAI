using Amuse.UI.Commands;
using Amuse.UI.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Amuse.UI.Windows
{
    /// <summary>
    /// Interaction logic for ModifyWindow.xaml
    /// </summary>
    public partial class ModifyWindow : BaseWindow
    {
        public ModifyWindow(AmuseSettings settings, ILogger<ModifyWindow> logger)
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
