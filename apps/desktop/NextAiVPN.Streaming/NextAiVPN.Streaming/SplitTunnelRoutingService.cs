using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using NextAiVPN.Common;
using NextAiVPN.Streaming.Entities;

namespace NextAiVPN.Streaming;

public class SplitTunnelRoutingService : ISplitTunnelRoutingService
{
	private const int BypassMetric = 999;

	private readonly ISplitTunnelingRepository _splitTunnelingRepository;

	private readonly IAppSettingsHelper _appSettingsHelper;

	public SplitTunnelRoutingService(ISplitTunnelingRepository splitTunnelingRepository, IAppSettingsHelper appSettingsHelper)
	{
		_splitTunnelingRepository = splitTunnelingRepository;
		_appSettingsHelper = appSettingsHelper;
	}

	public async Task AddDomainsToBypassVpnAsync()
	{
		BestRoute best = GetBestLocalDefaultRoute(StreamingConstants.VpnName);
		if (best == null)
		{
			Logger.Log.Error("Failed to find a non‑VPN default route.", "AddDomainsToBypassVpnAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 41);
			return;
		}
		foreach (string domain in ExpandDomains(await _splitTunnelingRepository.GetItems()))
		{
			try
			{
				await AddBypassRoute(domain, best);
			}
			catch (Exception ex)
			{
				Logger.Log.Error("DNS error for " + domain + ": " + ex.Message, "AddDomainsToBypassVpnAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 54);
			}
		}
	}

	private static async Task AddBypassRoute(string domain, BestRoute best)
	{
		IPAddress[] array = (from ip in await Dns.GetHostAddressesAsync(domain)
			group ip by ip.ToString() into g
			select g.First()).ToArray();
		foreach (IPAddress iPAddress in array)
		{
			if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
			{
				AddBypassRoute(iPAddress, best.Gateway, best.IfIndex);
				Logger.Log.Information($"BYPASS {domain}:{iPAddress}", "AddBypassRoute", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 73);
			}
		}
	}

	public async Task AddDomainsToSplitTunnel()
	{
		int interfaceIndex = GetInterfaceIndexByName(StreamingConstants.VpnName);
		if (interfaceIndex == -1)
		{
			Logger.Log.Error("Could not find VPN interface index.", "AddDomainsToSplitTunnel", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 86);
			return;
		}
		foreach (string item in ExpandDomains(await _splitTunnelingRepository.GetItems()))
		{
			await AddDomainRoutesAsync(item, interfaceIndex);
		}
	}

	private static async Task AddDomainRoutesAsync(string domain, int interfaceIndex)
	{
		try
		{
			IPAddress[] array = await Dns.GetHostAddressesAsync(domain);
			foreach (IPAddress iPAddress in array)
			{
				if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
				{
					AddRouteViaIf(iPAddress, interfaceIndex);
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Log.Error("Error resolving " + domain + ": " + ex.Message, "AddDomainRoutesAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 114);
		}
	}

	public async Task RemoveAllBypassRoutesAsync(string vpnInterfaceAlias)
	{
		await Task.Run(delegate
		{
			try
			{
				using Process process = Process.Start(new ProcessStartInfo
				{
					FileName = "powershell.exe",
					Arguments = "-NoProfile -Command \"Get-NetRoute | Where-Object {($_.DestinationPrefix -like '*/32') -and ($_.InterfaceAlias -ne '" + vpnInterfaceAlias + "') -and $_.DestinationPrefix -ne '0.0.0.0/0'} | ForEach-Object { route delete $_.DestinationPrefix.Split('/')[0] }\"",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				});
				string text = process.StandardOutput.ReadToEnd();
				string value = process.StandardError.ReadToEnd();
				process.WaitForExit();
				if (process.ExitCode != 0)
				{
					Logger.Log.Error($"Failed to remove routes. ExitCode={process.ExitCode}, Error={value}", "RemoveAllBypassRoutesAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 152);
				}
				else
				{
					Logger.Log.Information("Removed bypass routes:\n" + text, "RemoveAllBypassRoutesAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 156);
				}
			}
			catch (Exception ex)
			{
				Logger.Log.Error("Failed to remove all bypass routes: " + ex.Message, "RemoveAllBypassRoutesAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 162);
			}
		});
	}

	public async Task RemoveAllBypassRoutesAsync()
	{
		await Task.Run(delegate
		{
			try
			{
				using Process process = Process.Start(new ProcessStartInfo
				{
					FileName = "powershell.exe",
					Arguments = "-NoProfile -Command " + $"$metric={999}; " + "$r1 = Get-NetRoute -AddressFamily IPv4  | Where-Object { $_.DestinationPrefix -like '*/32' -and $_.RouteMetric -eq $metric }  | Select-Object -Expand DestinationPrefix; $r2 = Get-NetRoute -AddressFamily IPv4 -PolicyStore PersistentStore -ErrorAction SilentlyContinue  | Where-Object { $_.DestinationPrefix -like '*/32' -and $_.RouteMetric -eq $metric }  | Select-Object -Expand DestinationPrefix; $targets = @($r1 + $r2) | Sort-Object -Unique; $targets | ForEach-Object { $_ }",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				});
				string text = process.StandardOutput.ReadToEnd();
				string text2 = process.StandardError.ReadToEnd();
				process.WaitForExit();
				if (!string.IsNullOrWhiteSpace(text2))
				{
					Logger.Log.Warning("[SplitTunneling] sweep stderr: " + text2, "RemoveAllBypassRoutesAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 206);
				}
				string[] array = text.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				string[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					if (IPAddress.TryParse(array2[i].Split('/')[0].Trim(), out IPAddress address))
					{
						RemoveRoute(address);
						Logger.Log.Information($"[SplitTunneling] BYPASS removed {address}", "RemoveAllBypassRoutesAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 215);
					}
				}
				Logger.Log.Information($"[SplitTunneling] Removed {array.Length} /32 routes with metric {999}.", "RemoveAllBypassRoutesAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 219);
			}
			catch (Exception ex)
			{
				Logger.Log.Error("[SplitTunneling] RemoveAllBypassRoutesAsync failed: " + ex.Message, "RemoveAllBypassRoutesAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 224);
			}
		});
	}

	public async Task RemoveBypassRoutesAsync()
	{
		foreach (string item in ExpandDomains(await _splitTunnelingRepository.GetItems()))
		{
			await RemoveBypassRoutesForDomainAsync(item);
		}
	}

	private static async Task RemoveBypassRoutesForDomainAsync(string domain)
	{
		try
		{
			IPAddress[] array = await Dns.GetHostAddressesAsync(domain);
			foreach (IPAddress iPAddress in array)
			{
				if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
				{
					RemoveRoute(iPAddress);
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Log.Error("Error removing route for " + domain + ": " + ex.Message, "RemoveBypassRoutesForDomainAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 258);
		}
	}

	public Task<bool> IsSplitTunnelingEnabledAsync()
	{
		return Task.FromResult(_appSettingsHelper.GetValue("IsStreamingSplitTunnelingEnabled").Equals("1"));
	}

	private static void AddBypassRoute(IPAddress ip, string gateway, int ifIndex)
	{
		RunRoute("add", ip.ToString(), "mask", "255.255.255.255", gateway, "IF", ifIndex.ToString(CultureInfo.InvariantCulture), "metric", 999.ToString(CultureInfo.InvariantCulture));
	}

	private static void AddRouteViaIf(IPAddress ip, int interfaceIndex)
	{
		RunRoute("add", ip.ToString(), "mask", "255.255.255.255", "0.0.0.0", "IF", interfaceIndex.ToString(CultureInfo.InvariantCulture));
	}

	private static void RemoveRoute(IPAddress ip)
	{
		RunRoute("delete", ip.ToString());
	}

	private static void RunRoute(params string[] args)
	{
		ProcessStartInfo processStartInfo = new ProcessStartInfo
		{
			FileName = Path.Combine(Environment.SystemDirectory, "route.exe"),
			UseShellExecute = false,
			CreateNoWindow = true,
			WindowStyle = ProcessWindowStyle.Hidden
		};
		foreach (string item in args)
		{
			processStartInfo.ArgumentList.Add(item);
		}
		using Process process = Process.Start(processStartInfo);
		process.WaitForExit();
		if (process.ExitCode != 0)
		{
			Logger.Log.Error($"route {string.Join(" ", args)} failed ({process.ExitCode})", "RunRoute", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 331);
		}
	}

	private static BestRoute GetBestLocalDefaultRoute(string vpnAlias)
	{
		try
		{
			BestRoute bestRoute = null;
			int num = int.MaxValue;
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface networkInterface in allNetworkInterfaces)
			{
				NetworkInterfaceType networkInterfaceType = networkInterface.NetworkInterfaceType;
				if (networkInterface.OperationalStatus != OperationalStatus.Up || networkInterfaceType == NetworkInterfaceType.Loopback || networkInterfaceType == NetworkInterfaceType.Tunnel || networkInterfaceType == NetworkInterfaceType.Ppp || (!string.IsNullOrEmpty(vpnAlias) && (networkInterface.Name.Equals(vpnAlias, StringComparison.OrdinalIgnoreCase) || networkInterface.Description.IndexOf(vpnAlias, StringComparison.OrdinalIgnoreCase) >= 0)) || networkInterface.Description.IndexOf("WAN Miniport", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					continue;
				}
				IPInterfaceProperties iPProperties = networkInterface.GetIPProperties();
				if (iPProperties == null)
				{
					continue;
				}
				IPv4InterfaceProperties iPv4Properties = iPProperties.GetIPv4Properties();
				if (iPv4Properties == null)
				{
					continue;
				}
				string text = null;
				foreach (GatewayIPAddressInformation gatewayAddress in iPProperties.GatewayAddresses)
				{
					if (gatewayAddress != null && gatewayAddress.Address?.AddressFamily == AddressFamily.InterNetwork && gatewayAddress.Address.ToString() != "0.0.0.0")
					{
						text = gatewayAddress.Address.ToString();
						break;
					}
				}
				if (text != null)
				{
					int num2 = networkInterfaceType switch
					{
						NetworkInterfaceType.Wireless80211 => 1, 
						NetworkInterfaceType.Ethernet => 0, 
						_ => 2, 
					};
					if (num2 < num)
					{
						bestRoute = new BestRoute
						{
							Gateway = text,
							IfIndex = iPv4Properties.Index
						};
						num = num2;
					}
				}
			}
			if (bestRoute != null)
			{
				Logger.Log.Information($"[SplitTunneling] .NET route selected: GW {bestRoute.Gateway}, IF {bestRoute.IfIndex}", "GetBestLocalDefaultRoute", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 416);
				return bestRoute;
			}
			using (Process process = Process.Start(new ProcessStartInfo
			{
				FileName = "powershell.exe",
				Arguments = "-NoProfile -Command $r = Get-NetRoute -AddressFamily IPv4 -DestinationPrefix 0.0.0.0/0 | " + (string.IsNullOrEmpty(vpnAlias) ? "" : ("Where-Object { $_.InterfaceAlias -ne '" + vpnAlias + "' } | ")) + "Where-Object { $_.InterfaceAlias -notmatch 'WAN Miniport|IKEv2|SSTP|L2TP|PPTP|TAP|TUN|WireGuard' } | Select-Object NextHop,ifIndex,InterfaceAlias,RouteMetric,InterfaceMetric,@{n='TotalMetric';e={$_.RouteMetric + $_.InterfaceMetric}} | Sort-Object TotalMetric | Select-Object -First 1; $gw = $null; if ($r -and ($null -eq $r.NextHop -or $r.NextHop -eq '0.0.0.0')) {   $cfg = Get-NetIPConfiguration -InterfaceIndex $r.ifIndex -ErrorAction SilentlyContinue;   if ($cfg -and $cfg.IPv4DefaultGateway) { $gw = $cfg.IPv4DefaultGateway.NextHop } } else { $gw = $r.NextHop } if ($r -and $gw) { Write-Output ($gw + ',' + $r.ifIndex) }",
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			}))
			{
				string text2 = process.StandardOutput.ReadToEnd().Trim();
				string text3 = process.StandardError.ReadToEnd().Trim();
				process.WaitForExit();
				if (!string.IsNullOrWhiteSpace(text3))
				{
					Logger.Log.Warning("[SplitTunneling] PS stderr: " + text3, "GetBestLocalDefaultRoute", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 451);
				}
				if (!string.IsNullOrWhiteSpace(text2))
				{
					string[] array = text2.Split(',');
					if (array.Length == 2 && IPAddress.TryParse(array[0], out IPAddress address) && int.TryParse(array[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
					{
						Logger.Log.Information($"[SplitTunneling] PS route selected: GW {address}, IF {result}", "GetBestLocalDefaultRoute", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 460);
						return new BestRoute
						{
							Gateway = address.ToString(),
							IfIndex = result
						};
					}
				}
			}
			DumpDiagnostics(vpnAlias);
			Logger.Log.Error("[SplitTunneling] No suitable non‑VPN default route found.", "GetBestLocalDefaultRoute", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 466);
		}
		catch (Exception ex)
		{
			Logger.Log.Error("[SplitTunneling] GetBestLocalDefaultRoute failed: " + ex.Message, "GetBestLocalDefaultRoute", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 470);
		}
		return null;
	}

	private static void DumpDiagnostics(string vpnAlias)
	{
		try
		{
			RunShellLog("powershell.exe", "Get-NetRoute -AddressFamily IPv4 -DestinationPrefix 0.0.0.0/0 | ft ifIndex,InterfaceAlias,NextHop,RouteMetric,InterfaceMetric -AutoSize");
			RunShellLog("powershell.exe", "Get-NetIPConfiguration | select InterfaceAlias,InterfaceIndex,IPv4DefaultGateway | fl");
			if (!string.IsNullOrEmpty(vpnAlias))
			{
				Logger.Log.Information("[SplitTunneling] Expected VPN alias: '" + vpnAlias + "'", "DumpDiagnostics", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 489);
			}
		}
		catch (Exception ex)
		{
			Logger.Log.Error("[SplitTunneling] DumpDiagnostics failed: " + ex.Message, "DumpDiagnostics", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 494);
		}
	}

	private static void RunShellLog(string file, string args)
	{
		using Process process = Process.Start(new ProcessStartInfo
		{
			FileName = file,
			Arguments = "-NoProfile -Command " + args,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true
		});
		string text = process.StandardOutput.ReadToEnd();
		string text2 = process.StandardError.ReadToEnd();
		process.WaitForExit();
		if (!string.IsNullOrWhiteSpace(text2))
		{
			Logger.Log.Warning(text2, "RunShellLog", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 516);
		}
		if (!string.IsNullOrWhiteSpace(text))
		{
			Logger.Log.Information(text, "RunShellLog", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 521);
		}
	}

	private static int GetInterfaceIndexByName(string interfaceName)
	{
		try
		{
			using Process process = Process.Start(new ProcessStartInfo
			{
				FileName = Path.Combine(Environment.SystemDirectory, "netsh.exe"),
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true,
				ArgumentList = { "interface", "ipv4", "show", "interfaces" }
			});
			string text = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			string[] array = text.Split('\n');
			foreach (string text2 in array)
			{
				if (text2.IndexOf(interfaceName, StringComparison.Ordinal) >= 0)
				{
					string[] array2 = text2.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					if (array2.Length != 0 && int.TryParse(array2[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
					{
						return result;
					}
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Log.Error("Failed to get interface index: " + ex.Message, "GetInterfaceIndexByName", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SplitTunnelRoutingService.cs", 572);
		}
		return -1;
	}

	private static IEnumerable<string> ExpandDomains(IEnumerable<string> domains)
	{
		foreach (string d in domains)
		{
			yield return d;
			if (!d.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
			{
				yield return "www." + d;
			}
		}
	}
}
