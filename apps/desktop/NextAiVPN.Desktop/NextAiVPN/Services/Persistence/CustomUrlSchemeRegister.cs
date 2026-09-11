using System;
using Microsoft.Win32;
using NextAiVPN.Common;

namespace NextAiVPN.Services.Persistence;

internal class CustomUrlSchemeRegister : ICustomUrlSchemeRegister
{
	private readonly IAppLogger _logger;

	public CustomUrlSchemeRegister(IAppLogger logger)
	{
		_logger = logger;
	}

	public void RegisterCustomUrlScheme(string schemeName, string applicationPath)
	{
		try
		{
			using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\" + schemeName))
			{
				registryKey.SetValue("", "URL:NextAiVPN Protocol");
				registryKey.SetValue("URL Protocol", "");
				using RegistryKey registryKey2 = registryKey.CreateSubKey("shell");
				using RegistryKey registryKey3 = registryKey2.CreateSubKey("open");
				using RegistryKey registryKey4 = registryKey3.CreateSubKey("command");
				registryKey4.SetValue("", "\"" + applicationPath + "\" \"%1\"");
			}
			_logger?.Information(schemeName + " URL scheme registered successfully.", "RegisterCustomUrlScheme", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\CustomUrlSchemeRegister.cs", 46);
		}
		catch (Exception ex)
		{
			_logger?.Error(ex.Message, "RegisterCustomUrlScheme", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\CustomUrlSchemeRegister.cs", 51);
		}
	}
}
