using System;
using Microsoft.Win32;
using NextAiVPN.Common;

namespace NextAiVPN.Services.Persistence;

internal class UninstallRegistryVersionUpdater : IUninstallRegistryVersionUpdater
{
	private const string UninstallKeyPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";

	private static readonly string AppDisplayName = SystemInfo.AppName;

	private const string DisplayNameValueName = "DisplayName";

	private const string DisplayVersionValueName = "DisplayVersion";

	private static readonly RegistryView[] Views = new RegistryView[2]
	{
		RegistryView.Registry64,
		RegistryView.Registry32
	};

	private readonly IAppLogger _logger;

	public UninstallRegistryVersionUpdater(IAppLogger logger)
	{
		_logger = logger;
	}

	public void UpdateDisplayVersion()
	{
		string appVersion = SystemInfo.AppVersion;
		RegistryView[] views = Views;
		foreach (RegistryView view in views)
		{
			try
			{
				UpdateInView(view, appVersion);
			}
			catch (Exception exception)
			{
				_logger?.Error(exception, "UpdateDisplayVersion", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\UninstallRegistryVersionUpdater.cs", 39);
			}
		}
	}

	private void UpdateInView(RegistryView view, string currentVersion)
	{
		using RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
		using RegistryKey registryKey2 = registryKey.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall");
		if (registryKey2 != null)
		{
			string[] subKeyNames = registryKey2.GetSubKeyNames();
			foreach (string subKeyName in subKeyNames)
			{
				UpdateEntry(registryKey2, subKeyName, currentVersion);
			}
		}
	}

	private void UpdateEntry(RegistryKey uninstallKey, string subKeyName, string currentVersion)
	{
		try
		{
			using (RegistryKey registryKey = uninstallKey.OpenSubKey(subKeyName))
			{
				string value = registryKey?.GetValue("DisplayName") as string;
				if (!AppDisplayName.Equals(value, StringComparison.Ordinal) || !IsRegistryVersionOutdated(registryKey.GetValue("DisplayVersion")?.ToString(), currentVersion))
				{
					return;
				}
			}
			using RegistryKey registryKey2 = uninstallKey.OpenSubKey(subKeyName, writable: true);
			registryKey2?.SetValue("DisplayVersion", currentVersion, RegistryValueKind.String);
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "UpdateEntry", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\UninstallRegistryVersionUpdater.cs", 87);
		}
	}

	internal static bool IsRegistryVersionOutdated(string registryVersion, string currentVersion)
	{
		if (Version.TryParse(registryVersion, out Version result) && Version.TryParse(currentVersion, out Version result2))
		{
			return result < result2;
		}
		return false;
	}
}
