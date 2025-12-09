using Amuse.UI.Models;
using System.Windows;
using System.Windows.Controls;

namespace Amuse.UI.UserControls
{
    public partial class ProgressElement : UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="ProgressElement" /> class.</summary>
        public ProgressElement()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(nameof(Progress), typeof(ProgressInfo), typeof(ProgressElement));

        public ProgressInfo Progress
        {
            get { return (ProgressInfo)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }


        protected override Size MeasureOverride(Size constraint)
        {
            if (ProgressBarControl is not null)
                ProgressBarControl.Width = 0;

            return base.MeasureOverride(constraint);
        }


        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            if (ProgressBarControl is not null)
                ProgressBarControl.Width = arrangeBounds.Width;

            return base.ArrangeOverride(arrangeBounds);
        }

    }
}
