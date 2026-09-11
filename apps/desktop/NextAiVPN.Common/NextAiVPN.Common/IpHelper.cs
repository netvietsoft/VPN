using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NextAiVPN.Common;

public class IpHelper : IIpHelper
{
	private static readonly HttpClient _httpClient = new HttpClient
	{
		Timeout = TimeSpan.FromSeconds(5L)
	};

	public async Task<string> GetLocalIpAsync()
	{
		return await Task.Run(() => Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault((IPAddress x) => x.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(x))?.ToString());
	}

	public async Task<string> GetPublicIpAsync(IEnumerable<string> servers)
	{
		if (servers == null || servers.Count() == 0)
		{
			throw new ArgumentException("No servers provided");
		}
		foreach (string item in servers.Where((string x) => !string.IsNullOrWhiteSpace(x)))
		{
			try
			{
				if (Uri.TryCreate(item, UriKind.Absolute, out Uri result))
				{
					string text = (await _httpClient.GetStringAsync(result))?.Trim();
					if (IPAddress.TryParse(text, out IPAddress _))
					{
						return text;
					}
					continue;
				}
				IPAddress iPAddress = (await Dns.GetHostAddressesAsync(item)).FirstOrDefault((IPAddress x) => x.AddressFamily == AddressFamily.InterNetwork);
				if (iPAddress != null)
				{
					return iPAddress.ToString();
				}
			}
			catch
			{
			}
		}
		return null;
	}
}
