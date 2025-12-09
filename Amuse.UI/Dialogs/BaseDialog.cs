using Amuse.UI.Commands;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace Amuse.UI.Dialogs
{
    public class BaseDialog : Window, INotifyPropertyChanged
    {
        public BaseDialog()
        {
            SaveCommand = new AsyncRelayCommand(SaveAsync, CanExecuteSave);
            CancelCommand = new AsyncRelayCommand(CancelAsync, CanExecuteCancel);
            WindowCloseCommand = new AsyncRelayCommand(WindowClose);
            WindowRestoreCommand = new AsyncRelayCommand(WindowRestore);
            WindowMinimizeCommand = new AsyncRelayCommand(WindowMinimize);
            WindowMaximizeCommand = new AsyncRelayCommand(WindowMaximize);
            Loaded += (s, e) => CreateOpenAnimation();
        }



        public AsyncRelayCommand SaveCommand { get; init; }
        public AsyncRelayCommand CancelCommand { get; init; }
        public AsyncRelayCommand WindowMinimizeCommand { get; }
        public AsyncRelayCommand WindowRestoreCommand { get; }
        public AsyncRelayCommand WindowMaximizeCommand { get; }
        public AsyncRelayCommand WindowCloseCommand { get; }

        protected virtual Task WindowClose()
        {
            return CloseDialogAsync(false);
        }

        private Task WindowRestore()
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
            return Task.CompletedTask;
        }

        private Task WindowMinimize()
        {
            WindowState = WindowState.Minimized;
            return Task.CompletedTask;
        }

        private Task WindowMaximize()
        {
            WindowState = WindowState.Maximized;
            return Task.CompletedTask;
        }


        public virtual Task<bool> ShowDialogAsync()
        {
            Opacity = 0;
            return Task.FromResult(ShowDialog() ?? false);
        }


        public Task CloseDialogAsync(bool dialogResult)
        {
            Owner.Tag = false; // Dialog Closed
            var tcs = new TaskCompletionSource();
            var fadeOutAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            };

            fadeOutAnimation.Completed += (s, e) =>
            {
                try
                {
                    DialogResult = dialogResult;
                }
                finally
                {
                    tcs.SetResult();
                }
            };
            BeginAnimation(OpacityProperty, fadeOutAnimation);
            return tcs.Task;
        }


        protected virtual Task SaveAsync()
        {
            return CloseDialogAsync(true);
        }


        protected virtual bool CanExecuteSave()
        {
            return true;
        }


        protected virtual Task CancelAsync()
        {
            return CloseDialogAsync(false);
        }


        protected virtual bool CanExecuteCancel()
        {
            return true;
        }


        private void CreateOpenAnimation()
        {
            Owner.Tag = true; // Dialog Open
            var fadeInAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            };
            fadeInAnimation.Completed += (s, e) => { Opacity = 1; };
            BeginAnimation(OpacityProperty, fadeInAnimation);
        }

        protected override void OnContentRendered(EventArgs e)
        {
            InvalidateVisual();
            base.OnContentRendered(e);
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
