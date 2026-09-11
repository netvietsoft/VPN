using System;
using System.Globalization;
using System.Net.NetworkInformation;
using Bugsnag;
using NextAiVPN.Common;

namespace NextAiVPN.Services;

internal static class BugsnagServiceFactory
{
	public static IBugsnagService Create(IAppSettingsHelper appSettingsHelper, IAppLogger logger)
	{
		try
		{
			string text = "f09ed9125b51daee9c4f6acf778dffd7";
			if (string.IsNullOrWhiteSpace(text))
			{
				logger?.Warning("Bugsnag API key is not configured; error reporting is disabled", "Create", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\BugsnagServiceFactory.cs", 30);
				return new NullBugsnagService();
			}
			Configuration configuration = new Configuration(text);
			configuration.AppVersion = SystemInfo.AppVersion;
			configuration.ReleaseStage = SystemInfo.ReleaseStage;
			configuration.AutoNotify = true;
			configuration.AutoCaptureSessions = true;
			configuration.ProjectNamespaces = new string[1] { "NextAiVPN" };
			return new BugsnagService(new Client(configuration), appSettingsHelper, NetworkInterface.GetIsNetworkAvailable);
		}
		catch (Exception ex)
		{
			string message = string.Format(CultureInfo.InvariantCulture, "[Error] - Path: {0}.{1}() - {2}", "BugsnagServiceFactory", "Create", ex.Message);
			logger?.Error(message, "Create", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\BugsnagServiceFactory.cs", 50);
			return new NullBugsnagService();
		}
	}
}
