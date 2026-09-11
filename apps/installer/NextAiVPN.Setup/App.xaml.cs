using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace NextAiVPN.Setup
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Any(a => a.Equals("/uninstall", StringComparison.OrdinalIgnoreCase) || 
                                a.Equals("-uninstall", StringComparison.OrdinalIgnoreCase)))
            {
                bool isSilent = e.Args.Any(a => a.Equals("/silent", StringComparison.OrdinalIgnoreCase) || 
                                                a.Equals("-silent", StringComparison.OrdinalIgnoreCase));
                PerformUninstall(isSilent);
                Shutdown();
                return;
            }

            // Normal Setup Wizard GUI
            try
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi động trình cài đặt:\n" + ex.ToString(), "Lỗi NextAI VPN Setup", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        private void PerformUninstall(bool isSilent)
        {
            try
            {
                if (!isSilent)
                {
                    var result = MessageBox.Show(
                        "Bạn có chắc chắn muốn gỡ cài đặt NextAI VPN và toàn bộ thành phần liên quan khỏi máy tính?",
                        "NextAI VPN Uninstaller",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    );
                    if (result != MessageBoxResult.Yes) return;
                }

                // 1. Tắt tiến trình NextAiVPN nếu đang chạy
                foreach (var proc in Process.GetProcessesByName("NextAiVPN.Desktop"))
                {
                    try { proc.Kill(); proc.WaitForExit(3000); } catch { }
                }

                // 2. Xóa shortcuts
                string desktopShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "NextAI VPN.lnk");
                ShortcutHelper.RemoveShortcut(desktopShortcut);

                string userDesktopShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "NextAI VPN.lnk");
                ShortcutHelper.RemoveShortcut(userDesktopShortcut);

                string startMenuDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs", "NextAI Technology");
                if (Directory.Exists(startMenuDir))
                {
                    try { Directory.Delete(startMenuDir, true); } catch { }
                }

                // 3. Xóa Registry
                RegistryHelper.RemoveUninstallInfo();
                RegistryHelper.SetRunOnStartup(false, "");

                // 4. Lên lịch xóa thư mục cài đặt khi thoát
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c timeout /t 2 & rmdir /s /q \"{baseDir}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });

                if (!isSilent)
                {
                    MessageBox.Show(
                        "NextAI VPN đã được gỡ cài đặt thành công khỏi hệ thống.",
                        "Gỡ Cài Đặt Hoàn Tất",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            }
            catch (Exception ex)
            {
                if (!isSilent)
                {
                    MessageBox.Show("Lỗi khi gỡ cài đặt: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
