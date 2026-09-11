using System;
using System.IO;
using Microsoft.Win32;

namespace NextAiVPN.Setup
{
    /// <summary>
    /// Quản lý đăng ký thông tin cài đặt vào Windows Registry (Add/Remove Programs)
    /// </summary>
    public static class RegistryHelper
    {
        private const string UninstallKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\NextAiVPN";
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "NextAiVPN";

        public static void RegisterUninstallInfo(string installDir, string setupExePath)
        {
            try
            {
                // Thử ghi vào HKLM trước (nếu có quyền Admin), nếu không thì ghi vào HKCU (chuẩn User Installer)
                RegistryKey? key = null;
                try
                {
                    var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    key = baseKey.CreateSubKey(UninstallKeyPath);
                }
                catch
                {
                    var userKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                    key = userKey.CreateSubKey(UninstallKeyPath);
                }

                if (key != null)
                {
                    using (key)
                    {
                        string mainExe = Path.Combine(installDir, "NextAiVPN.Desktop.exe");
                        key.SetValue("DisplayName", "NextAI VPN Desktop & Residential Gateway", RegistryValueKind.String);
                        key.SetValue("DisplayVersion", "8.0.0.0", RegistryValueKind.String);
                        key.SetValue("Publisher", "nextaitechnology", RegistryValueKind.String);
                        key.SetValue("DisplayIcon", $"{mainExe},0", RegistryValueKind.String);
                        key.SetValue("InstallLocation", installDir, RegistryValueKind.String);
                        key.SetValue("UninstallString", $"\"{setupExePath}\" /uninstall", RegistryValueKind.String);
                        key.SetValue("QuietUninstallString", $"\"{setupExePath}\" /uninstall /silent", RegistryValueKind.String);
                        key.SetValue("EstimatedSize", 85000, RegistryValueKind.DWord); // ~85 MB
                        key.SetValue("NoModify", 1, RegistryValueKind.DWord);
                        key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                        key.SetValue("URLInfoAbout", "https://nextaitechnology.com", RegistryValueKind.String);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[RegistryHelper] Không thể ghi registry: " + ex.Message);
            }
        }

        public static void RemoveUninstallInfo()
        {
            try
            {
                using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                hklm.DeleteSubKeyTree(UninstallKeyPath, false);
            }
            catch { }

            try
            {
                using var hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                hkcu.DeleteSubKeyTree(UninstallKeyPath, false);
            }
            catch { }
        }

        public static void SetRunOnStartup(bool enable, string exePath)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
                if (key != null)
                {
                    if (enable)
                    {
                        key.SetValue(AppName, $"\"{exePath}\" --minimized");
                    }
                    else
                    {
                        key.DeleteValue(AppName, false);
                    }
                }
            }
            catch { }
        }
    }
}
