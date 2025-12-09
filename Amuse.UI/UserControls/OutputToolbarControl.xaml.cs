using Amuse.UI.Commands;
using Amuse.UI.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Amuse.UI.UserControls
{
    /// <summary>
    /// Interaction logic for OutputToolbarControl.xaml
    /// </summary>
    public partial class OutputToolbarControl : UserControl, INotifyPropertyChanged
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputToolbarControl"/> class.
        /// </summary>
        public OutputToolbarControl()
        {
            EnableHistoryViewCommand = new RelayCommand<bool>(enableHistory => IsHistoryView = enableHistory);
            InitializeComponent();
        }

        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings", typeof(AmuseSettings), typeof(OutputToolbarControl));

        public static readonly DependencyProperty IsHistoryViewProperty =
            DependencyProperty.Register("IsHistoryView", typeof(bool), typeof(OutputToolbarControl));

        public static readonly DependencyProperty HistoryColumnsProperty =
            DependencyProperty.Register("HistoryColumns", typeof(int), typeof(OutputToolbarControl), new PropertyMetadata(4));

        public static readonly DependencyProperty StatisticsProperty =
            DependencyProperty.Register("Statistics", typeof(StatisticsModel), typeof(OutputToolbarControl));

        public static readonly DependencyProperty IsIterationsEnabledProperty =
            DependencyProperty.Register("IsIterationsEnabled", typeof(bool), typeof(OutputToolbarControl), new PropertyMetadata(true));

        public RelayCommand<bool> EnableHistoryViewCommand { get; }

        /// <summary>
        /// Gets or sets the UI settings.
        /// </summary>
        public AmuseSettings Settings
        {
            get { return (AmuseSettings)GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }

        public bool IsHistoryView
        {
            get { return (bool)GetValue(IsHistoryViewProperty); }
            set { SetValue(IsHistoryViewProperty, value); }
        }

        public int HistoryColumns
        {
            get { return (int)GetValue(HistoryColumnsProperty); }
            set { SetValue(HistoryColumnsProperty, value); }
        }

        public StatisticsModel Statistics
        {
            get { return (StatisticsModel)GetValue(StatisticsProperty); }
            set { SetValue(StatisticsProperty, value); }
        }

        public bool IsIterationsEnabled
        {
            get { return (bool)GetValue(IsIterationsEnabledProperty); }
            set { SetValue(IsIterationsEnabledProperty, value); }
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
