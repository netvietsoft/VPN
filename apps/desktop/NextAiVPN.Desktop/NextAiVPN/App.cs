using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NextAiVPN;

public partial class App : Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_trace.log");
		AppDomain.CurrentDomain.ProcessExit += (s, ev) =>
		{
			try
			{
				NextAiVPN.Services.NextAiLocationService.DisableSystemProxy();
				File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] ProcessExit triggered!\nStackTrace:\n{Environment.StackTrace}\n\n");
			}
			catch { }
		};
		AppDomain.CurrentDomain.UnhandledException += (s, ev) =>
		{
			try
			{
				NextAiVPN.Services.NextAiLocationService.DisableSystemProxy();
				File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] UnhandledException: {ev.ExceptionObject}\n\n");
			}
			catch { }
		};
		base.DispatcherUnhandledException += OnDispatcherUnhandledException;
		TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
		base.OnStartup(e);

		ShutdownMode = ShutdownMode.OnExplicitShutdown;
		try
		{
			File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] OnStartup entered\n");

			// [VI] Đảm bảo dọn dẹp sạch sẽ System Proxy khi khởi động
			// [EN] Ensure clean system proxy state on startup
			NextAiVPN.Services.NextAiLocationService.DisableSystemProxy();
			File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] DisableSystemProxy done\n");

			// [VI] Khởi chạy dịch vụ thu thập IP cư dân ngay khi khởi động
			// [EN] Start residential IP harvester service immediately on application startup
			NextAiVPN.Services.NextAiNodeCollectorService.Start();
			File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] NextAiNodeCollectorService started\n");

			File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] Creating MainWindow...\n");
			MainWindow mainWindow = new MainWindow();
			File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] MainWindow created\n");

			// [VI] Ẩn hoàn toàn MainWindow (cửa sổ đăng nhập cũ), không gọi Show() để tránh hiện cửa sổ đen
			// [EN] Completely hide MainWindow (legacy login window), do not call Show() to prevent black window overlay
			mainWindow.Hide();
			mainWindow.Visibility = Visibility.Collapsed;
			mainWindow.ShowInTaskbar = false;

			var expandedWindow = mainWindow.GetSdk()?.VpnExpandedWindow;
			File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] expandedWindow is {(expandedWindow == null ? "NULL" : "READY")}\n");
			if (expandedWindow != null)
			{
				Current.MainWindow = expandedWindow;
				expandedWindow.Show();
				File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] expandedWindow.Show() called\n");
				expandedWindow.ExpandedSideMenu?.SetMenuOption(NextAiVPN.Enums.SideMenuOption.Dashboard);
				File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] SetMenuOption(Dashboard) called\n");

				// [VI] Kích hoạt Agent kiểm thử tự động (AutoTestAgent) nếu có cờ --autotest hoặc biến môi trường
				// [EN] Activate AutoTestAgent for automated UI testing if --autotest flag or env var is set
				if (Array.Exists(Environment.GetCommandLineArgs(), a => a.Equals("--autotest", StringComparison.OrdinalIgnoreCase)) ||
				    Environment.GetEnvironmentVariable("NEXTAI_AUTOTEST") == "1")
				{
					NextAiVPN.Services.AutoTestAgent.Start(expandedWindow);
				}
			}

		}
		catch (Exception ex)
		{
			File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] EXCEPTION in OnStartup: {ex}\n");
			ReportException(ex);
		}
	}

	protected override void OnExit(ExitEventArgs e)
	{
		string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_trace.log");
		try
		{
			File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] OnExit called with code {e.ApplicationExitCode}\nStackTrace:\n{Environment.StackTrace}\n\n");
		}
		catch { }
		base.OnExit(e);
	}

	private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
	{
		ReportException(e.Exception);
		e.Handled = true;
	}

	private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
	{
		ReportException(e.Exception);
		e.SetObserved();
	}

	private static void ReportException(Exception exception)
	{
		if (exception != null)
		{
			try
			{
				string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_trace.log");
				File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] ReportException: {exception}\n");
				File.WriteAllText(@"e:\DECOMPILER\Soft\VPN\CONVERT\crash.log", exception.ToString() + "\n" + (exception.InnerException != null ? exception.InnerException.ToString() : ""));
			}
			catch {}
			Utils.Logger?.Error(exception, "ReportException", "App.xaml.cs", 81);
			Utils.BugsnagService.Notify(exception);
		}
	}
}
