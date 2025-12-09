using Amuse.UI.Commands;
using Amuse.UI.Helpers;
using Amuse.UI.Models;
using Amuse.UI.Services;
using Microsoft.Extensions.Logging;
using OnnxStack.StableDiffusion.Common;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;

namespace Amuse.UI.Views
{
    public class ViewBase : UserControl, INotifyPropertyChanged
    {
        public ViewBase()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                Logger = App.GetService<ILogger<ViewBase>>();
                FileService = App.GetService<IFileService>();
                DialogService = App.GetService<IDialogService>();
            }
            Progress = new ProgressInfo();
            OpenDirectoryCommand = new AsyncRelayCommand<string>(OpenDirectory);
        }

        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register(nameof(Settings), typeof(AmuseSettings), typeof(ViewBase), new PropertyMetadata<ViewBase>((c) => c.OnSettingsChanged()));

        public static readonly DependencyProperty TaskbarItemInfoProperty =
            DependencyProperty.Register(nameof(TaskbarItemInfo), typeof(TaskbarItemInfo), typeof(ViewBase));

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(nameof(Progress), typeof(ProgressInfo), typeof(ViewBase));

        public static readonly DependencyProperty StatisticsProperty =
            DependencyProperty.Register(nameof(Statistics), typeof(StatisticsModel), typeof(ViewBase));

        public static readonly DependencyProperty IsGeneratingProperty =
            DependencyProperty.Register(nameof(IsGenerating), typeof(bool), typeof(ViewBase));

        public ILogger<ViewBase> Logger { get; }
        public IFileService FileService { get; }
        public IDialogService DialogService { get; }
        public AsyncRelayCommand<string> OpenDirectoryCommand { get; }

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        public AmuseSettings Settings
        {
            get { return (AmuseSettings)GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }

        /// <summary>
        /// Gets or sets the taskbar item information.
        /// </summary>
        public TaskbarItemInfo TaskbarItemInfo
        {
            get { return (TaskbarItemInfo)GetValue(TaskbarItemInfoProperty); }
            set { SetValue(TaskbarItemInfoProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is generating.
        /// </summary>
        public bool IsGenerating
        {
            get { return (bool)GetValue(IsGeneratingProperty); }
            set { SetValue(IsGeneratingProperty, value); }
        }

        public ProgressInfo Progress
        {
            get { return (ProgressInfo)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        /// <summary>
        /// Gets or sets the statistics.
        /// </summary>
        public StatisticsModel Statistics
        {
            get { return (StatisticsModel)GetValue(StatisticsProperty); }
            set { SetValue(StatisticsProperty, value); }
        }

        /// <summary>
        /// Gets or sets the cancelation token source.
        /// </summary>
        protected CancellationTokenSource CancelationTokenSource { get; set; }


        /// <summary>
        /// Called when AmuseSettings changed.
        /// </summary>
        /// <returns></returns>
        protected virtual Task OnSettingsChanged()
        {
            Settings.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(Settings.DefaultExecutionDevice))
                    await OnDefaultExecutionDeviceChanged();
            };
            return Task.CompletedTask;
        }


        /// <summary>
        /// Called when the DefaultExecutionDevic has changed.
        /// </summary>
        /// <returns></returns>
        protected virtual Task OnDefaultExecutionDeviceChanged()
        {
            return Task.CompletedTask;
        }


        /// <summary>
        /// Starts the statistics.
        /// </summary>
        protected void StartStatistics()
        {
            Statistics = new StatisticsModel();
        }


        /// <summary>
        /// Updates the statistics.
        /// </summary>
        /// <param name="step">The step.</param>
        protected void UpdateStatistics(DiffusionProgress progress)
        {
            Statistics?.Update(progress);
        }


        /// <summary>
        /// Stops the statistics.
        /// </summary>
        protected void StopStatistics()
        {
            Statistics?.Complete();
        }


        /// <summary>
        /// Clears the statistics.
        /// </summary>
        protected void ClearStatistics()
        {
            Statistics = null;
        }





        /// <summary>
        /// Updates the progress.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="indeterminate">if set to <c>true</c> [indeterminate].</param>
        protected void UpdateProgress(string message, bool indeterminate = false)
        {
            if (indeterminate)
            {
                Progress.Indeterminate(message);
                return;
            }
            Progress.Update(message);
        }


        /// <summary>
        /// Updates the progress.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="maximum">The maximum.</param>
        /// <param name="message">The message.</param>
        protected void UpdateProgress(int value, int maximum, string message = null)
        {
            Progress.Update(value, maximum, message);
            UpdateTaskbarProgress(value, maximum);
        }


        protected void ClearProgress()
        {
            Progress.Clear();
            UpdateTaskbarProgress(0, 1);
        }


        /// <summary>
        /// Updates the taskbar progress.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="maximum">The maximum.</param>
        private void UpdateTaskbarProgress(int value, int maximum)
        {
            var pecent = maximum == 0 ? 0 : (double)value / maximum;
            if (TaskbarItemInfo.ProgressState != TaskbarItemProgressState.Normal && maximum > 0)
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;

            if (TaskbarItemInfo.ProgressState != TaskbarItemProgressState.None && maximum == 0)
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;

            if (TaskbarItemInfo.ProgressState != TaskbarItemProgressState.Indeterminate && maximum == -1)
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;

            TaskbarItemInfo.ProgressValue = pecent;
        }


        /// <summary>
        /// Opens the directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <returns></returns>
        private Task OpenDirectory(string directory)
        {
            try
            {
                Utils.NavigateToUrl(directory);
            }
            catch (Exception)
            {
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
