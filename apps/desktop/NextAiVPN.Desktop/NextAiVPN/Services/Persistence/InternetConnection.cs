using System;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NextAiVPN.Services.Persistence;

internal static class InternetConnection
{
	private static bool IsKillSwitch => Utils.AppSettingsHelper.GetValue("KillSwitch").Equals("1");

	[DllImport("wininet.dll")]
	private static extern bool InternetGetConnectedState(out int connectionDescription, int reservedValue);

	public static bool IsAvailable()
	{
		return GetConnectionStatus();
	}

	public static async Task<bool> IsAvailableAsync()
	{
		return GetConnectionStatus();
	}

	private static bool GetConnectionStatus()
	{
		bool flag = InternetGetConnectedState(out var connectionDescription, 0);
		if (IsKillSwitch)
		{
			return flag;
		}
		if (flag)
		{
			return true;
		}
		flag = InternetGetConnectedState(out connectionDescription, 0);
		bool flag2 = IsPing();
		return flag & flag2;
	}

	private static bool IsPing()
	{
		try
		{
			Ping ping = new Ping();
			string hostNameOrAddress = "google.com";
			byte[] buffer = new byte[32];
			int timeout = 1000;
			PingOptions options = new PingOptions();
			return ping.Send(hostNameOrAddress, timeout, buffer, options).Status == IPStatus.Success;
		}
		catch (Exception ex)
		{
			Utils.Logger.Error("InternetConnection error - method - InternetConnection.IsPing() - " + ex.Message, "IsPing", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\InternetConnection.cs", 75);
			return false;
		}
	}
}
