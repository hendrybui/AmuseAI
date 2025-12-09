using Amuse.UI.Commands;
using Amuse.UI.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Amuse.UI.Windows
{
    /// <summary>
    /// Interaction logic for CreateWindow.xaml
    /// </summary>
    public partial class CreateWindow : BaseWindow
    {
        public CreateWindow(AmuseSettings settings, ILogger<CreateWindow> logger)
        {
            Logger = logger;
            Settings = settings;
            IsLogCommandEnabled = true;
            IsUIModeCommandEnabled = true;
            NavigateCommand = new AsyncRelayCommand<NavigationModel>(NavigateAsync);
            InitializeComponent();
        }

        public AsyncRelayCommand<NavigationModel> NavigateCommand { get; }

        protected override Task WindowRestore()
        {
            SizeToContent = System.Windows.SizeToContent.Width;
            return base.WindowRestore();
        }

        protected override Task WindowMaximize()
        {
            SizeToContent = System.Windows.SizeToContent.Manual;
            return base.WindowMaximize();
        }

        private Task NavigateAsync(NavigationModel model)
        {
            return Task.CompletedTask;
        }
    }
}
