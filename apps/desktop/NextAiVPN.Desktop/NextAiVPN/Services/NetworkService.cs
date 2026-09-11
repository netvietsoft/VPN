using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.Services;

internal class NetworkService : INetworkService
{
	private readonly IBugsnagService _bugsnagService;

	public NetworkService(IBugsnagService bugsnagService)
	{
		_bugsnagService = bugsnagService;
	}

	public static NetworkInterface GetActiveNetworkInterface()
	{
		if (!InternetConnection.IsAvailable())
		{
			return null;
		}
		NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
		foreach (NetworkInterface networkInterface in allNetworkInterfaces)
		{
			if (networkInterface.OperationalStatus == OperationalStatus.Up && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Tunnel && networkInterface.Description.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) < 0 && networkInterface.Name.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) < 0 && !networkInterface.Description.Equals("Microsoft Loopback Adapter", StringComparison.OrdinalIgnoreCase))
			{
				return networkInterface;
			}
		}
		return null;
	}

	public string GetConnectedNetworkName()
	{
		string text = string.Empty;
		try
		{
			NetworkInterface activeNetworkInterface = GetActiveNetworkInterface();
			if (activeNetworkInterface == null)
			{
				for (int i = 0; i < 10; i++)
				{
					Thread.Sleep(500);
					activeNetworkInterface = GetActiveNetworkInterface();
					if (activeNetworkInterface != null)
					{
						break;
					}
				}
			}
			if (activeNetworkInterface != null)
			{
				if (activeNetworkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
				{
					return "nextaivpn-b2c92a50-ee99-41ba-8e1a-7285170d1190";
				}
				if (activeNetworkInterface.Description.ToLower().Contains("NextAiVPN Windows Tap Adapter".ToLower()))
				{
					return "nextaivpn-b2c92a50-ee99-41ba-8e1a-7285170d1190";
				}
				text = GetConnectedNetworkName(activeNetworkInterface);
				if (string.IsNullOrEmpty(text) || text.ToLower().Equals("Identifying...".ToLower()))
				{
					for (int j = 0; j < 10; j++)
					{
						Thread.Sleep(500);
						text = GetConnectedNetworkName();
						if (!string.IsNullOrEmpty(text) && !text.ToLower().Equals("Identifying...".ToLower()))
						{
							return text;
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			_bugsnagService.Notify("[GetConnectedNetworkName] - " + ex.Message);
		}
		return text;
	}

	public string GetConnectedNetworkName(NetworkInterface info)
	{
		IReadOnlyList<string> connectedNetworkNames = NetworkListManagerInterop.GetConnectedNetworkNames();
		switch (connectedNetworkNames.Count)
		{
		case 0:
			return string.Empty;
		case 1:
		{
			string text = connectedNetworkNames[0];
			if (text.Equals("Identifying...", StringComparison.InvariantCultureIgnoreCase) || string.IsNullOrEmpty(text))
			{
				return info.Description;
			}
			return text;
		}
		default:
			return info.Description;
		}
	}
}
