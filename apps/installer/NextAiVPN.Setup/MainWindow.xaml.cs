using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace NextAiVPN.Setup
{
    public partial class MainWindow : Window
    {
        private enum WizardStep
        {
            Welcome = 0,
            Options = 1,
            Installing = 2,
            Finished = 3
        }

        private WizardStep _currentStep = WizardStep.Welcome;
        private string _targetInstallDir = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            InitDefaults();
            UpdateStepUI();
        }

        private void InitDefaults()
        {
            try
            {
                bool isAdmin = false;
                try
                {
                    isAdmin = new System.Security.Principal.WindowsPrincipal(System.Security.Principal.WindowsIdentity.GetCurrent())
                        .IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
                }
                catch { }

                if (isAdmin)
                {
                    string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                    _targetInstallDir = Path.Combine(programFiles, "NextAiTechnology", "NextAiVPN");
                }
                else
                {
                    string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    _targetInstallDir = Path.Combine(localAppData, "Programs", "NextAiTechnology", "NextAiVPN");
                }

                TxtInstallPath.Text = _targetInstallDir;
                UpdateDiskSpaceInfo();
            }
            catch
            {
                TxtInstallPath.Text = @"C:\Program Files\NextAiTechnology\NextAiVPN";
            }
        }

        private void UpdateDiskSpaceInfo()
        {
            try
            {
                string root = Path.GetPathRoot(TxtInstallPath.Text) ?? "C:\\";
                var drive = new DriveInfo(root);
                if (drive.IsReady)
                {
                    double freeGb = drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
                    TxtDiskSpace.Text = $"Dung lượng yêu cầu: ~85 MB | Ổ đĩa ({drive.Name.TrimEnd('\\')}): còn trống {freeGb:F1} GB khả dụng";
                }
            }
            catch
            {
                TxtDiskSpace.Text = "Dung lượng yêu cầu: ~85 MB";
            }
        }

        private void UpdateStepUI()
        {
            PageWelcome.Visibility = _currentStep == WizardStep.Welcome ? Visibility.Visible : Visibility.Collapsed;
            PageOptions.Visibility = _currentStep == WizardStep.Options ? Visibility.Visible : Visibility.Collapsed;
            PageInstalling.Visibility = _currentStep == WizardStep.Installing ? Visibility.Visible : Visibility.Collapsed;
            PageFinished.Visibility = _currentStep == WizardStep.Finished ? Visibility.Visible : Visibility.Collapsed;

            switch (_currentStep)
            {
                case WizardStep.Welcome:
                    BtnBack.Visibility = Visibility.Collapsed;
                    BtnNext.Content = "Tiếp Tục >";
                    BtnNext.IsEnabled = true;
                    BtnCancel.Visibility = Visibility.Visible;
                    BtnCancel.IsEnabled = true;
                    break;

                case WizardStep.Options:
                    BtnBack.Visibility = Visibility.Visible;
                    BtnBack.IsEnabled = true;
                    BtnNext.Content = "Cài Đặt >";
                    BtnNext.IsEnabled = true;
                    BtnCancel.Visibility = Visibility.Visible;
                    BtnCancel.IsEnabled = true;
                    UpdateDiskSpaceInfo();
                    break;

                case WizardStep.Installing:
                    BtnBack.IsEnabled = false;
                    BtnNext.IsEnabled = false;
                    BtnCancel.IsEnabled = false;
                    break;

                case WizardStep.Finished:
                    BtnBack.Visibility = Visibility.Collapsed;
                    BtnCancel.Visibility = Visibility.Collapsed;
                    BtnNext.Content = "Hoàn Tất";
                    BtnNext.IsEnabled = true;
                    TxtFinishInstallPath.Text = $"Vị trí cài đặt: {_targetInstallDir}";
                    break;
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            switch (_currentStep)
            {
                case WizardStep.Welcome:
                    _currentStep = WizardStep.Options;
                    UpdateStepUI();
                    break;

                case WizardStep.Options:
                    _targetInstallDir = TxtInstallPath.Text.Trim();
                    if (string.IsNullOrWhiteSpace(_targetInstallDir))
                    {
                        MessageBox.Show("Vui lòng chọn đường dẫn thư mục cài đặt hợp lệ!", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    _currentStep = WizardStep.Installing;
                    UpdateStepUI();
                    _ = RunInstallationAsync();
                    break;

                case WizardStep.Finished:
                    if (ChkLaunchApp.IsChecked == true)
                    {
                        LaunchInstalledApp();
                    }
                    Close();
                    break;
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep == WizardStep.Options)
            {
                _currentStep = WizardStep.Welcome;
                UpdateStepUI();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn hủy bỏ quá trình cài đặt NextAI VPN không?",
                "NextAI VPN Setup",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );
            if (result == MessageBoxResult.Yes)
            {
                Close();
            }
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFolderDialog
                {
                    Title = "Chọn thư mục cài đặt NextAI VPN",
                    InitialDirectory = Directory.Exists(_targetInstallDir) ? _targetInstallDir : @"C:\Program Files"
                };

                if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.FolderName))
                {
                    _targetInstallDir = Path.Combine(dialog.FolderName, "NextAiVPN");
                    TxtInstallPath.Text = _targetInstallDir;
                    UpdateDiskSpaceInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở hộp thoại chọn thư mục: " + ex.Message, "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task RunInstallationAsync()
        {
            var progress = new Progress<InstallProgressReport>(report =>
            {
                InstallProgressBar.Value = report.Percentage;
                TxtProgressPercent.Text = $"{report.Percentage}%";
                TxtProgressStatus.Text = report.StatusMessage;
                TxtCurrentFile.Text = report.CurrentFile;
            });

            bool success = await Task.Run(() => DoInstallWork(progress));

            if (success)
            {
                _currentStep = WizardStep.Finished;
                UpdateStepUI();
            }
            else
            {
                MessageBox.Show(
                    "Đã xảy ra sự cố trong quá trình cài đặt. Vui lòng kiểm tra quyền Administrator hoặc đóng các ứng dụng đang chạy.",
                    "Lỗi Cài Đặt",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                _currentStep = WizardStep.Options;
                UpdateStepUI();
            }
        }

        private bool DoInstallWork(IProgress<InstallProgressReport> progress)
        {
            try
            {
                // 1. Dừng ứng dụng cũ nếu đang chạy
                progress.Report(new InstallProgressReport
                {
                    Percentage = 5,
                    StatusMessage = "Kiểm tra tiến trình đang chạy...",
                    CurrentFile = "Closing background processes"
                });

                foreach (var proc in Process.GetProcessesByName("NextAiVPN.Desktop"))
                {
                    try { proc.Kill(); proc.WaitForExit(3000); } catch { }
                }

                // 2. Tạo thư mục đích
                if (!Directory.Exists(_targetInstallDir))
                {
                    Directory.CreateDirectory(_targetInstallDir);
                }

                // 3. Đọc payload.zip nhúng trong assembly
                var assembly = Assembly.GetExecutingAssembly();
                using Stream? zipStream = assembly.GetManifestResourceStream("payload.zip");
                if (zipStream == null)
                {
                    throw new InvalidOperationException("Không tìm thấy tệp tài nguyên payload.zip bên trong trình cài đặt!");
                }

                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
                int totalEntries = archive.Entries.Count;
                int currentEntryIndex = 0;

                foreach (var entry in archive.Entries)
                {
                    currentEntryIndex++;
                    string destinationPath = Path.GetFullPath(Path.Combine(_targetInstallDir, entry.FullName));

                    // Bảo mật chống Zip Slip vulnerability
                    if (!destinationPath.StartsWith(_targetInstallDir, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        // Thư mục
                        Directory.CreateDirectory(destinationPath);
                    }
                    else
                    {
                        // Tệp tin
                        string? parent = Path.GetDirectoryName(destinationPath);
                        if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
                        {
                            Directory.CreateDirectory(parent);
                        }

                        entry.ExtractToFile(destinationPath, true);
                    }

                    int pct = 10 + (int)((currentEntryIndex / (double)totalEntries) * 75);
                    progress.Report(new InstallProgressReport
                    {
                        Percentage = Math.Min(85, pct),
                        StatusMessage = $"Đang giải nén tệp ({currentEntryIndex}/{totalEntries})...",
                        CurrentFile = entry.FullName
                    });
                }

                // 4. Sao chép chính trình cài đặt này vào thư mục làm Uninstaller
                progress.Report(new InstallProgressReport
                {
                    Percentage = 88,
                    StatusMessage = "Cấu hình trình gỡ cài đặt hệ thống...",
                    CurrentFile = "Configuring Uninstaller"
                });

                string currentSetupExe = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
                string installedSetupExe = Path.Combine(_targetInstallDir, "NextAiVPN_Setup.exe");
                if (File.Exists(currentSetupExe) && !currentSetupExe.Equals(installedSetupExe, StringComparison.OrdinalIgnoreCase))
                {
                    try { File.Copy(currentSetupExe, installedSetupExe, true); } catch { }
                }

                // 5. Tạo Shortcuts
                string mainExe = Path.Combine(_targetInstallDir, "NextAiVPN.Desktop.exe");

                bool createDesktop = false;
                bool createStartMenu = false;
                bool runStartup = false;

                Dispatcher.Invoke(() =>
                {
                    createDesktop = ChkDesktopShortcut.IsChecked == true;
                    createStartMenu = ChkStartMenuShortcut.IsChecked == true;
                    runStartup = ChkRunOnStartup.IsChecked == true;
                });

                if (createDesktop)
                {
                    progress.Report(new InstallProgressReport
                    {
                        Percentage = 92,
                        StatusMessage = "Đang tạo biểu tượng Desktop...",
                        CurrentFile = "Desktop Shortcut: NextAI VPN.lnk"
                    });

                    string commonDesktop = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
                    string userDesktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                    string shortcutPath = Path.Combine(Directory.Exists(commonDesktop) ? commonDesktop : userDesktop, "NextAI VPN.lnk");
                    ShortcutHelper.CreateShortcut(shortcutPath, mainExe, _targetInstallDir, "NextAI VPN Desktop & Residential Gateway", mainExe);
                }

                if (createStartMenu)
                {
                    progress.Report(new InstallProgressReport
                    {
                        Percentage = 95,
                        StatusMessage = "Đang tạo lối tắt trong Start Menu...",
                        CurrentFile = "Start Menu: NextAI Technology"
                    });

                    string startMenu = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
                    string nextAiStartMenu = Path.Combine(startMenu, "Programs", "NextAI Technology");
                    try 
                    { 
                        Directory.CreateDirectory(nextAiStartMenu); 
                    } 
                    catch 
                    {
                        string userPrograms = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                        nextAiStartMenu = Path.Combine(userPrograms, "NextAI Technology");
                        Directory.CreateDirectory(nextAiStartMenu);
                    }
                    string smShortcut = Path.Combine(nextAiStartMenu, "NextAI VPN.lnk");
                    ShortcutHelper.CreateShortcut(smShortcut, mainExe, _targetInstallDir, "NextAI VPN Desktop & Residential Gateway", mainExe);
                }

                // 6. Đăng ký Windows Registry
                progress.Report(new InstallProgressReport
                {
                    Percentage = 98,
                    StatusMessage = "Đăng ký vào hệ thống Windows...",
                    CurrentFile = "Registry: HKLM/Uninstall/NextAiVPN"
                });

                RegistryHelper.RegisterUninstallInfo(_targetInstallDir, installedSetupExe);
                if (runStartup)
                {
                    RegistryHelper.SetRunOnStartup(true, mainExe);
                }

                progress.Report(new InstallProgressReport
                {
                    Percentage = 100,
                    StatusMessage = "Cài đặt hoàn tất 100%!",
                    CurrentFile = "Done"
                });

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Setup] Error during install: " + ex);
                return false;
            }
        }

        private void LaunchInstalledApp()
        {
            try
            {
                string mainExe = Path.Combine(_targetInstallDir, "NextAiVPN.Desktop.exe");
                if (File.Exists(mainExe))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = mainExe,
                        WorkingDirectory = _targetInstallDir,
                        UseShellExecute = true
                    });
                }
            }
            catch { }
        }
    }
}
