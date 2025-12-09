using Amuse.UI.Dialogs;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static Amuse.UI.Dialogs.MessageDialog;

namespace Amuse.UI.Services
{
    public class DialogService : IDialogService
    {

        public T GetDialog<T>() where T : Window
        {
            return Resolve<T>(Application.Current.MainWindow);
        }

        public T GetDialog<T>(Window owner) where T : Window
        {
            return Resolve<T>(owner);
        }

        private T Resolve<T>(Window owner) where T : Window
        {
            var dialog = App.GetService<T>();
            dialog.Owner = owner;
            return dialog;
        }


        public async Task<bool> ShowMessageDialogAsync(string title, string message, MessageDialogType dialogType = MessageDialogType.Ok, MessageBoxIconType messageBoxIcon = MessageBoxIconType.None, MessageBoxStyleType messageBoxStyle = MessageBoxStyleType.None)
        {
            var dialog = GetDialog<MessageDialog>();
            return await dialog.ShowDialogAsync(title, message, dialogType, messageBoxIcon, messageBoxStyle);
        }

        public async Task<bool> ShowErrorMessageAsync(string title, string message)
        {
            var dialog = GetDialog<MessageDialog>();
            return await dialog.ShowDialogAsync(title, SanitizeOnnxRuntimeErrorMessage(message), MessageDialogType.Ok, MessageBoxIconType.Error, MessageBoxStyleType.Danger);
        }

        public async Task<DialogResult> ShowMessageDialogAsync(string title, string message, MessageDialogType dialogType = MessageDialogType.Ok, MessageBoxIconType messageBoxIcon = MessageBoxIconType.None, MessageBoxStyleType messageBoxStyle = MessageBoxStyleType.None, bool isDontAskEnabled = false)
        {
            var dialog = GetDialog<MessageDialog>();
            var result = await dialog.ShowDialogAsync(title, message, dialogType, messageBoxIcon, messageBoxStyle, isDontAskEnabled);
            return new DialogResult(result, dialog.IsDontAskSelected);
        }

        public Task<string> OpenFolderDialogAsync(string title, string initialDirectory = default)
        {
            var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = title,
                InitialDirectory = initialDirectory,
                UseDescriptionForTitle = true,
                AutoUpgradeEnabled = true
            };

            var dialogResult = folderBrowserDialog.ShowDialog(App.CurrentWindow);
            if (dialogResult == System.Windows.Forms.DialogResult.OK)
                return Task.FromResult(folderBrowserDialog.SelectedPath);

            return Task.FromResult<string>(default);
        }


        public Task<string> SaveFileDialogAsync(string title, string initialFilename, string initialDirectory = default, string filter = default, string defualtExt = default)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = title,
                Filter = filter,
                DefaultExt = defualtExt,
                AddExtension = true,
                RestoreDirectory = string.IsNullOrEmpty(initialDirectory),
                InitialDirectory = initialDirectory,
                FileName = initialFilename
            };

            var dialogResult = saveFileDialog.ShowDialog(App.CurrentWindow);
            if (dialogResult == true)
                return Task.FromResult(saveFileDialog.FileName);

            return Task.FromResult<string>(default);
        }


        public Task<string> OpenFileDialogAsync(string title, string initialDirectory = default, string filter = default, string defualtExt = default)
        {
            if (Path.HasExtension(initialDirectory))
                initialDirectory = Path.GetDirectoryName(initialDirectory);

            var openFileDialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                CheckFileExists = true,
                InitialDirectory = initialDirectory,
                RestoreDirectory = string.IsNullOrEmpty(initialDirectory),
                DefaultExt = defualtExt,
                AddExtension = true,
                Multiselect = false
            };
            var dialogResult = openFileDialog.ShowDialog(App.CurrentWindow);
            if (dialogResult == true)
                return Task.FromResult(openFileDialog.FileName);

            return Task.FromResult<string>(default);
        }


        private string SanitizeOnnxRuntimeErrorMessage(string message)
        {
            if (message.Contains("tid("))
                return $"[OnnxRuntime Exception]: {message
                    .Split("tid(")
                    .Last()
                    .Split(")")
                    .Last()
                    .Trim()}";
            return message;
        }
    }


    public struct DialogResult
    {
        private readonly bool _result;
        private readonly bool _dontAskAgain;

        public DialogResult(bool result, bool dontAskAgain)
        {
            _result = result;
            _dontAskAgain = dontAskAgain;
        }

        public readonly bool Result => _result;
        public readonly bool DontAskAgain => _dontAskAgain;

        public static implicit operator bool(DialogResult dialogResult)
        {
            return dialogResult._result;
        }

        public static implicit operator DialogResult(bool result)
        {
            return new DialogResult(result, false);
        }

        public static bool operator true(DialogResult dialogResult)
        {
            return dialogResult._result;
        }

        public static bool operator false(DialogResult dialogResult)
        {
            return !dialogResult._result;
        }
    }

}
