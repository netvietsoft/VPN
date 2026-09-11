using System;
using System.Diagnostics;
using System.Linq;

namespace VpnSDK.Common.Utilities;

public class DnsConfigurationUtility
{
	private const string NETSH_FILE_NAME = "netsh";

	private const string NETSH_SET_DNS_ARGUMENTS_FORMAT = "interface ipv4 set dns \"{0}\" static {1} validate=no";

	private const string NETSH_ADD_DNS_ARGUMENTS_FORMAT = "interface ipv4 add dns \"{0}\" {1} validate=no";

	public static void SetDnsServer(string adapterName, string[] dnsServers)
	{
		if (string.IsNullOrWhiteSpace(adapterName))
		{
			throw new ArgumentException("Adapter name is required");
		}
		if (dnsServers == null || dnsServers.Length == 0)
		{
			throw new ArgumentException("DNS servers array must not be null or empty.", "dnsServers");
		}
		SetPrimaryDnsServer(adapterName, dnsServers[0]);
		AddAdditionalDnsServers(adapterName, dnsServers.Skip(1).ToArray());
	}

	public static void AddAdditionalDnsServers(string adapterName, string[] dnsServers)
	{
		if (dnsServers != null && dnsServers.Any())
		{
			foreach (string arg in dnsServers)
			{
				RunNetshCommand($"interface ipv4 add dns \"{adapterName}\" {arg} validate=no");
			}
		}
	}

	private static void SetPrimaryDnsServer(string adapterName, string dnsServer)
	{
		RunNetshCommand($"interface ipv4 set dns \"{adapterName}\" static {dnsServer} validate=no");
	}

	private static void RunNetshCommand(string arguments)
	{
		using Process process = new Process();
		process.StartInfo.FileName = "netsh";
		process.StartInfo.Arguments = arguments;
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.RedirectStandardInput = true;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.UseShellExecute = false;
		process.Start();
		process.WaitForExit();
		if (process.ExitCode != 0)
		{
			throw new InvalidOperationException($"Failed to run netsh command with arguments: {arguments}. Exit code: {process.ExitCode}");
		}
	}
}
