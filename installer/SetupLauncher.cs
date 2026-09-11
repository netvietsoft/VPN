using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace NextAiVPN.Installer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string msiPath = Path.Combine(baseDir, "NextAiVPN_Installer.msi");

                if (!File.Exists(msiPath))
                {
                    MessageBox.Show(
                        "Không tìm thấy tệp bộ cài đặt NextAiVPN_Installer.msi!\n\nVui lòng đảm bảo tệp NextAiVPN_Installer.msi nằm cùng thư mục.",
                        "NextAI VPN - Thông Báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "msiexec.exe",
                    Arguments = string.Format("/i \"{0}\"", msiPath),
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process proc = Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể khởi động trình cài đặt:\n" + ex.Message,
                    "NextAI VPN Setup",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }
    }
}
