using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using NextAiVPN.Common;

namespace NextAiVPN.Services.Persistence;

internal class AdminRunner : IAdminRunner
{
	private readonly DispatcherTimer _timer;

	private readonly IAppSettingsHelper _appSettingsHelper;

	public AdminRunner(IAppSettingsHelper appSettingsHelper)
	{
		_appSettingsHelper = appSettingsHelper;
		_timer = new DispatcherTimer();
		_timer.Tick += TimerOnTick;
		_timer.Interval = TimeSpan.FromSeconds(3L);
	}

	public void RunAsAdmin()
	{
		try
		{
			_appSettingsHelper.SetValue("RunAsAdmin", "1");
			Process process = Process.Start(new ProcessStartInfo(VPNConstants.FilePath.NextAiVpnExeFilePath)
			{
				UseShellExecute = true,
				Verb = "runas"
			});
			if (process != null)
			{
				_timer.Start();
				Application.Current.Shutdown();
			}
		}
		catch (Exception)
		{
			// If elevation is cancelled or fails, cancel timer and do not shutdown
			_timer.Stop();
		}
	}

	private void TimerOnTick(object sender, EventArgs e)
	{
		_timer.Stop();
		Application.Current.Shutdown();
	}
}
