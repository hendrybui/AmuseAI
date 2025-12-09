namespace Amuse.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for AppDisclaimer.xaml
    /// </summary>
    public partial class AppDisclaimer : BaseDialog
    {
        private bool _isDisclaimerAccepted;

        public AppDisclaimer()
        {
            InitializeComponent();
        }


        public bool IsDisclaimerAccepted
        {
            get { return _isDisclaimerAccepted; }
            set { _isDisclaimerAccepted = value; NotifyPropertyChanged(); }
        }


        protected override bool CanExecuteSave()
        {
            return _isDisclaimerAccepted;
        }

    }
}
