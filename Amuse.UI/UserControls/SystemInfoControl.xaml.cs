using Amuse.UI.Models;
using Amuse.UI.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Amuse.UI.UserControls
{
    /// <summary>
    /// Interaction logic for SystemInfoControl.xaml
    /// </summary>
    public partial class SystemInfoControl : UserControl
    {
        private readonly IDeviceService _deviceService;
        private readonly DispatcherTimer _updateTimer;
        private ObservableCollection<Device> _deviceCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemInfoControl"/> class.
        /// </summary>
        public SystemInfoControl()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _deviceService = App.GetService<IDeviceService>();
                _deviceCollection = new ObservableCollection<Device>(_deviceService.Devices);
                _updateTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(300), DispatcherPriority.Normal, UpdateDevices, Dispatcher);
                _updateTimer.Start();
            }
            InitializeComponent();
        }

        public ObservableCollection<Device> DeviceCollection
        {
            get { return _deviceCollection; }
            set { _deviceCollection = value; }
        }


        private void UpdateDevices(object sender, EventArgs e)
        {
            foreach (var device in DeviceCollection)
            {
                device.NotifyPropertyChanged(nameof(Device.Usage));
                device.NotifyPropertyChanged(nameof(Device.MemoryUsage));
                device.NotifyPropertyChanged(nameof(Device.MemoryRemaining));
                device.NotifyPropertyChanged(nameof(Device.ProcessMemoryUsage));
                device.NotifyPropertyChanged(nameof(Device.ProgressValue));
                device.NotifyPropertyChanged(nameof(Device.ProgressSubValue));
            }
        }
    }
}
