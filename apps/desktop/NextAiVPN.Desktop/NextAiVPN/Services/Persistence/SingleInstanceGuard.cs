using System.Threading;
using System.Windows;

namespace NextAiVPN.Services.Persistence;

internal class SingleInstanceGuard : ISingleInstance
{
	private const string EventName = "1533535b-481e-45e3-a316-ed95e89f1fa4";

	private readonly SDKMonitor _sdk;

	private readonly ISingleInstanceDetector _singleInstanceDetector;

	private readonly EventWaitHandle _eventWaitHandle;

	public SingleInstanceGuard(SDKMonitor sdk, ISingleInstanceDetector singleInstanceDetector)
	{
		_sdk = sdk;
		_singleInstanceDetector = singleInstanceDetector;
		_eventWaitHandle = new EventWaitHandle(initialState: false, EventResetMode.AutoReset, "1533535b-481e-45e3-a316-ed95e89f1fa4");
	}

	/// <summary>
	/// [VI] Bảo vệ đa tiến trình - cho phép chạy trực tiếp mà không kill ứng dụng.
	/// [EN] Multi-instance guard - allows direct running without killing the application.
	/// </summary>
	public void EnsureSingleInstance()
	{
		try
		{
			Thread thread = new Thread((ThreadStart)delegate
			{
				while (true)
				{
					_eventWaitHandle.WaitOne();
					Application.Current?.Dispatcher?.Invoke(delegate
					{
						_sdk?.TaskBarService?.DisplayWindowsOnSystemTrayIconClick();
					});
				}
			});
			thread.IsBackground = true;
			thread.Start();
		}
		catch
		{
		}
	}
}
