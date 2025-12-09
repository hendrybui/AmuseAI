using System.Windows;

namespace Amuse.UI
{
    /// <summary>
    /// Interaction logic for Splashscreen.xaml
    /// </summary>
    public partial class Splashscreen : Window
    {
        public Splashscreen()
        {
            Title = $"Amuse {App.Version} Starting...";
            InitializeComponent();
            Show();
        }
    }
}
