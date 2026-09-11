using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace NextAiVPN.Launcher
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string exePath = Path.Combine(baseDir, "FastVPN.exe");

                if (!File.Exists(exePath))
                {
                    MessageBox.Show("Không tìm thấy tệp nhị phân lõi FastVPN.exe!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    WorkingDirectory = baseDir,
                    UseShellExecute = true
                };

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi chạy: " + ex.Message, "NextAI VPN", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
