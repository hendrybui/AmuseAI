using Amuse.UI.Models;
using Amuse.UI.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Amuse.UI.Helpers;
using System.Threading.Tasks;
using Amuse.UI.Enums;

namespace Amuse.UI.UserControls
{
    /// <summary>
    /// Interaction logic for DevicePickerControl.xaml
    /// </summary>
    public partial class DevicePickerControl : UserControl, INotifyPropertyChanged
    {
        private readonly IReadOnlyList<Device> _devices;
        private readonly IDeviceService _deviceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevicePickerControl"/> class.
        /// </summary>
        public DevicePickerControl()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _deviceService = App.GetService<IDeviceService>();
                _devices = _deviceService.Devices
                    .Where(x => x.DeviceType != DeviceType.NPU)
                    .ToList();
            }
            InitializeComponent();
        }

        public static readonly DependencyProperty SelectedDeviceProperty =
          DependencyProperty.Register(nameof(SelectedDevice), typeof(Device), typeof(DevicePickerControl), new PropertyMetadata<DevicePickerControl>(c => c.OnSelectedDeviceChanged()));

        public static readonly DependencyProperty DeviceIdProperty =
            DependencyProperty.Register(nameof(DeviceId), typeof(int?), typeof(DevicePickerControl), new PropertyMetadata<DevicePickerControl>(c => c.OnDefaultDeviceChanged()));

        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.Register(nameof(Provider), typeof(ExecutionProvider?), typeof(DevicePickerControl), new PropertyMetadata<DevicePickerControl>(c => c.OnDefaultDeviceChanged()));

        /// <summary>
        /// Gets the devices.
        /// </summary>
        public IReadOnlyList<Device> Devices => _devices;

        /// <summary>
        /// Gets or sets the selected device.
        /// </summary>
        public Device SelectedDevice
        {
            get { return (Device)GetValue(SelectedDeviceProperty); }
            set { SetValue(SelectedDeviceProperty, value); OnSelectedDeviceChanged(); }
        }

        /// <summary>
        /// Gets or sets the DeviceId.
        /// </summary>
        public int? DeviceId
        {
            get { return (int?)GetValue(DeviceIdProperty); }
            set { SetValue(DeviceIdProperty, value); }
        }

        /// <summary>
        /// Gets or sets the ExecutionProvider.
        /// </summary>
        public ExecutionProvider? Provider
        {
            get { return (ExecutionProvider?)GetValue(ProviderProperty); }
            set { SetValue(ProviderProperty, value); }
        }


        /// <summary>
        /// Called when SelectedDevice changed.
        /// </summary>
        private Task OnSelectedDeviceChanged()
        {
            DeviceId = SelectedDevice.DeviceId;
            Provider = SelectedDevice.Provider;
            return Task.CompletedTask;
        }


        /// <summary>
        /// Called when default DeviceId or Provider changed.
        /// </summary>
        /// <returns>Task.</returns>
        private Task OnDefaultDeviceChanged()
        {
            if (SelectedDevice == null && DeviceId.HasValue && Provider.HasValue)
            {
                SelectedDevice = _devices.FirstOrDefault(x => x.DeviceId == DeviceId && x.Provider == Provider);
            }
            return Task.CompletedTask;
        }


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion
    }
}
