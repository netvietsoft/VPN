using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using NextAiVPN.Entities.Streaming;
using NextAiVPN.Enums.Streaming;

namespace NextAiVPN.Streaming.Entities;

public class StreamingLocation : IStreamingLocation
{
	public string Id { get; }

	public string CountryCode { get; }

	public string FullLocationName { get; }

	public string CityCode { get; }

	public string SearchName { get; }

	public ushort? PingMs { get; set; }

	public string Country { get; }

	public string City { get; }

	public IList<Protocol> AvailableProtocols { get; } = new List<Protocol>();

	public IList<IServer> Servers { get; } = new List<IServer>();

	public StreamingLocation(string countryCode, string country, string protocol)
	{
		CountryCode = countryCode;
		Country = country;
		SearchName = GetSearchName();
		Id = SearchName;
		City = GetCityName();
		FullLocationName = countryCode + " - " + country;
		if (protocol.ToLower().Equals(Protocol.IKEV2.ToString().ToLower()))
		{
			AvailableProtocols.Add(Protocol.IKEV2);
		}
	}

	private string GetSearchName()
	{
		return CountryCode + Country + City;
	}

	private string GetCityName()
	{
		return "Optimized for Streaming";
	}

	public Task<ushort?> Ping()
	{
		return Task.Run(delegate
		{
			Ping ping = new Ping();
			try
			{
				PingReply pingReply = ping.Send(Servers.First().Name);
				if (pingReply.Status == IPStatus.Success)
				{
					PingMs = (ushort)pingReply.RoundtripTime;
					return PingMs;
				}
				PingMs = GetRandomPing();
				return PingMs;
			}
			catch (Exception ex)
			{
				Logger.Log.Error(ex.Message, "Ping", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\Entities\\StreamingLocation.cs", 71);
				PingMs = GetRandomPing();
				return PingMs;
			}
			finally
			{
				((IDisposable)ping)?.Dispose();
			}
		});
	}

	private static ushort GetRandomPing()
	{
		Logger.Log.Error("Getting random ping", "GetRandomPing", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\Entities\\StreamingLocation.cs", 81);
		return (ushort)new Random().Next(168, 256);
	}
}
