using System;
using System.Management;

namespace NextAiVPN.Services;

internal class OSVersionService : IOSVersionService
{
	private readonly IBugsnagService _bugsnagService;

	public OSVersionService(IBugsnagService bugsnagService)
	{
		_bugsnagService = bugsnagService;
	}

	public string GetOSVersion()
	{
		try
		{
			using ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
			using ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();
			foreach (ManagementObject item in managementObjectCollection)
			{
				using (item)
				{
					object propertyValue = item.GetPropertyValue("Caption");
					if (propertyValue != null && !string.IsNullOrEmpty(propertyValue.ToString()))
					{
						return propertyValue.ToString();
					}
				}
			}
		}
		catch (Exception ex)
		{
			_bugsnagService.Notify("GetOSVersion error - " + ex.Message);
		}
		switch (Environment.OSVersion.Platform)
		{
		case PlatformID.Win32S:
			return "Win 3.1";
		case PlatformID.Win32Windows:
			switch (Environment.OSVersion.Version.Minor)
			{
			case 0:
				return "Win95";
			case 10:
				return "Win98";
			case 90:
				return "WinME";
			}
			break;
		case PlatformID.Win32NT:
			switch (Environment.OSVersion.Version.Major)
			{
			case 3:
				return "NT 3.51";
			case 4:
				return "NT 4.0";
			case 5:
				switch (Environment.OSVersion.Version.Minor)
				{
				case 0:
					return "Win2000";
				case 1:
					return "WinXP";
				case 2:
					return "Win2003";
				}
				break;
			case 6:
				switch (Environment.OSVersion.Version.Minor)
				{
				case 0:
					return "Vista/Win2008Server";
				case 1:
					return "Win7/Win2008Server R2";
				case 2:
					return "Win8/Win2012Server";
				case 3:
					return "Win8.1/Win2012Server R2";
				}
				break;
			case 10:
				return "Windows 10";
			default:
				return $"Unknown {Environment.OSVersion.Version.Major}";
			}
			break;
		case PlatformID.WinCE:
			return "Win CE";
		}
		return "Unknown";
	}
}
