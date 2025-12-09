using System.Threading.Tasks;
using System.Windows;
using static Amuse.UI.Dialogs.MessageDialog;

namespace Amuse.UI.Services
{
    public interface IDialogService
    {
        T GetDialog<T>() where T : Window;
        T GetDialog<T>(Window owner) where T : Window;

        Task<string> OpenFolderDialogAsync(string title, string initialDirectory = default);
        Task<string> OpenFileDialogAsync(string title, string initialDirectory = default, string filter = default, string defualtExt = default);
        Task<string> SaveFileDialogAsync(string title, string initialFilename, string initialDirectory = default, string filter = default, string defualtExt = default);

        Task<bool> ShowErrorMessageAsync(string title, string message);
        Task<bool> ShowMessageDialogAsync(string title, string message, MessageDialogType dialogType = MessageDialogType.Ok, MessageBoxIconType messageBoxIcon = MessageBoxIconType.None, MessageBoxStyleType messageBoxStyle = MessageBoxStyleType.None);
        Task<DialogResult> ShowMessageDialogAsync(string title, string message, MessageDialogType dialogType = MessageDialogType.Ok, MessageBoxIconType messageBoxIcon = MessageBoxIconType.None, MessageBoxStyleType messageBoxStyle = MessageBoxStyleType.None, bool dontAskAgain = false);
    }

}
