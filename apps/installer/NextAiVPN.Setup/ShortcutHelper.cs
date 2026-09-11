using System;
using System.IO;

namespace NextAiVPN.Setup
{
    /// <summary>
    /// Tiện ích tạo và quản lý Windows Shortcut (.lnk) thông qua COM WScript.Shell
    /// </summary>
    public static class ShortcutHelper
    {
        public static void CreateShortcut(string shortcutPath, string targetPath, string workingDir, string description, string iconPath)
        {
            try
            {
                string dir = Path.GetDirectoryName(shortcutPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return;

                dynamic shell = Activator.CreateInstance(shellType);
                dynamic shortcut = shell.CreateShortcut(shortcutPath);

                shortcut.TargetPath = targetPath;
                shortcut.WorkingDirectory = workingDir;
                shortcut.Description = description;
                if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
                {
                    shortcut.IconLocation = iconPath + ",0";
                }
                shortcut.Save();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[ShortcutHelper] Không thể tạo shortcut: " + ex.Message);
            }
        }

        public static void RemoveShortcut(string shortcutPath)
        {
            try
            {
                if (File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                }
            }
            catch { }
        }
    }
}
