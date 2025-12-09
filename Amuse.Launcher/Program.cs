using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace Amuse.Launcher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string amusePath = GetAmuseExePath();
            if (amusePath == null || !File.Exists(amusePath))
                return;

            var processStartInfo = new ProcessStartInfo
            {
                FileName = amusePath,
                WorkingDirectory = Path.GetDirectoryName(amusePath),
                UseShellExecute = false
            };

            processStartInfo.Environment["DD_ENV_INSTR_STACK_SIZE_MB"] = "30";
            Process.Start(processStartInfo);
        }


        static string GetAmuseExePath()
        {
            const string regKey = @"SOFTWARE\Amuse";
            const string valueName = "Install_Dir";

            // First check 64-bit view
            string path = ReadInstallDir(RegistryView.Registry64, regKey, valueName);
            if (!string.IsNullOrWhiteSpace(path))
                return Path.Combine(path, "Amuse.exe");

            // Then check 32-bit view (WOW64)
            path = ReadInstallDir(RegistryView.Registry32, regKey, valueName);
            if (!string.IsNullOrWhiteSpace(path))
                return Path.Combine(path, "Amuse.exe");

            // Fallback to Program Files
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Amuse", "Amuse.exe");
        }


        static string ReadInstallDir(RegistryView view, string subKey, string valueName)
        {
            using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view).OpenSubKey(subKey))
            {
                if (key != null)
                {
                    var installDir = key.GetValue(valueName) as string;
                    if (!string.IsNullOrWhiteSpace(installDir))
                        return installDir;
                }
            }
            return null;
        }
    }
}
