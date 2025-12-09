using Amuse.UI.Enums;
using Amuse.UI.Models;
using System.Windows;
using System.Windows.Controls;

namespace Amuse.UI.UserControls
{
    public partial class MemoryModeControl : UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="MemoryModeControl" /> class.</summary>
        public MemoryModeControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MemoryInfoProperty =
           DependencyProperty.Register(nameof(MemoryInfo), typeof(MemoryInfoModel), typeof(MemoryModeControl));

        public static readonly DependencyProperty MemoryModeProperty =
            DependencyProperty.Register(nameof(MemoryMode), typeof(MemoryMode), typeof(MemoryModeControl), new FrameworkPropertyMetadata(MemoryMode.Auto, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty IsMinimizedProperty =
            DependencyProperty.Register(nameof(IsMinimized), typeof(bool), typeof(MemoryModeControl), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty IsPipelineLoadedProperty =
            DependencyProperty.Register(nameof(IsPipelineLoaded), typeof(bool), typeof(MemoryModeControl), new PropertyMetadata(false));

        public MemoryInfoModel MemoryInfo
        {
            get { return (MemoryInfoModel)GetValue(MemoryInfoProperty); }
            set { SetValue(MemoryInfoProperty, value); }
        }

        public MemoryMode MemoryMode
        {
            get { return (MemoryMode)GetValue(MemoryModeProperty); }
            set { SetValue(MemoryModeProperty, value); }
        }

        public bool IsMinimized
        {
            get { return (bool)GetValue(IsMinimizedProperty); }
            set { SetValue(IsMinimizedProperty, value); }
        }

        public bool IsPipelineLoaded
        {
            get { return (bool)GetValue(IsPipelineLoadedProperty); }
            set { SetValue(IsPipelineLoadedProperty, value); }
        }
    }
}
