using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using NextAiVPN.Entities.Streaming;
using NextAiVPN.Streaming.Entities;
using Newtonsoft.Json;
using RestSharp;

namespace NextAiVPN.Streaming;

public class StreamingVpnService : IStreamingVpnService
{
	private static readonly RestClient RestClient = new RestClient(new RestClientOptions(StreamingConstants.BaseEndPoint));

	public async Task<IEnumerable<IStreamingLocation>> GetServerListAsync()
	{
		try
		{
			RestRequest request = new RestRequest(StreamingConstants.ServerListEndpoint);
			RestResponse restResponse = await RestClient.ExecuteAsync(request);
			if (restResponse.IsSuccessful)
			{
				return ConvertToLocationObjects(restResponse.Content);
			}
			Logger.Log.Error($"[Streaming] Cant get streaming server list. StatusCode: {restResponse.StatusCode}", "GetServerListAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\StreamingVpnService.cs", 36);
			return new List<IStreamingLocation>();
		}
		catch (Exception ex)
		{
			Logger.Log.Error(ex.Message, "GetServerListAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\StreamingVpnService.cs", 44);
			return new List<IStreamingLocation>();
		}
	}

	public IEnumerable<StreamingLocation> GetServerList()
	{
		try
		{
			RestRequest request = new RestRequest(StreamingConstants.ServerListEndpoint);
			RestResponse restResponse = RestClient.Execute(request);
			if (restResponse.IsSuccessful)
			{
				return ConvertToLocationObjects(restResponse.Content);
			}
			Logger.Log.Error($"[Streaming] Cant get streaming server list. StatusCode: {restResponse.StatusCode}", "GetServerList", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\StreamingVpnService.cs", 70);
			return new List<StreamingLocation>();
		}
		catch (Exception ex)
		{
			Logger.Log.Error(ex.Message, "GetServerList", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\StreamingVpnService.cs", 77);
			throw;
		}
	}

	private static IEnumerable<StreamingLocation> ConvertToLocationObjects(string response)
	{
		StreamingServerListResponse streamingServerListResponse = JsonConvert.DeserializeObject<StreamingServerListResponse>(response);
		List<StreamingLocation> list = new List<StreamingLocation>();
		if (streamingServerListResponse.Success == 0)
		{
			return list;
		}
		foreach (CountryServerList server in streamingServerListResponse.ServerList)
		{
			try
			{
				StreamingLocation streamingLocation = new StreamingLocation(server.CountryCode, server.Country, server.Protocol);
				foreach (StreamingServer server2 in server.Servers)
				{
					int.TryParse(server2.Utilization, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result);
					streamingLocation.Servers.Add(new Server(server2.Name, server2.Status, result));
				}
				streamingLocation.Ping();
				list.Add(streamingLocation);
			}
			catch (Exception ex)
			{
				Logger.Log.Error($"[Streaming] Can't convert to location object {ex.Data}", "ConvertToLocationObjects", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\StreamingVpnService.cs", 109);
			}
		}
		return list;
	}

	public string GetServerListString()
	{
		try
		{
			RestRequest request = new RestRequest(StreamingConstants.ServerListEndpoint);
			RestResponse restResponse = RestClient.Execute(request);
			if (restResponse.IsSuccessful)
			{
				return restResponse.Content;
			}
			return "Cant get locations";
		}
		catch (Exception)
		{
			return "Cant get locations";
		}
	}
}
