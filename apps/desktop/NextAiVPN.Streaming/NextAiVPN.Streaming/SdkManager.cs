using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using NextAiVPN.Entities.Streaming;
using NextAiVPN.Enums.Streaming;
using NextAiVPN.Streaming.Entities;
using NextAiVPN.Streaming.Exceptions;

namespace NextAiVPN.Streaming;

public class SdkManager : IDisposable
{
	private readonly IVpnConnector _connector;

	private readonly IStreamingVpnService _streamingVpnService;

	private readonly ICertificatesHelper _certificatesHelper;

	private readonly NetworkInterface _networkInterface;

	private int _tryGetLocationsCounter;

	private bool _disposed;

	public ObservableCollection<StreamingLocation> Locations = new ObservableCollection<StreamingLocation>();

	public Action<object> VpnConnectionStatusChanged;

	public Action<object> Error;

	public bool IsConnected => VpnConnectionStatus == StreamingConnectionStatus.Connected;

	public bool IsActive => _connector.IsActive;

	public StreamingConnectionStatus VpnConnectionStatus { get; private set; }

	public IStreamingLocation LastConnectedLocation { get; set; }

	public SdkManager(IVpnConnector vpnConnector, IStreamingVpnService streamingVpnService, ICertificatesHelper certificatesHelper)
	{
		_connector = vpnConnector;
		_streamingVpnService = streamingVpnService;
		_certificatesHelper = certificatesHelper;
		_networkInterface = GetNetworkInterface();
	}

	public async Task<IEnumerable<StreamingLocation>> GetStreamingLocations()
	{
		try
		{
			Locations = new ObservableCollection<StreamingLocation>((List<StreamingLocation>)(await _streamingVpnService.GetServerListAsync()));
			_tryGetLocationsCounter = 0;
			return Locations;
		}
		catch (Exception)
		{
			_tryGetLocationsCounter++;
			if (_tryGetLocationsCounter > 2)
			{
				_tryGetLocationsCounter = 0;
				throw new LocationListException();
			}
			return await GetStreamingLocations();
		}
	}

	public async Task<(long uploaded, long downloaded)> GetStatisticDataBytes()
	{
		(long, long) returnValue = (0L, 0L);
		await Task.Run(delegate
		{
			if (!IsConnected)
			{
				return 0;
			}
			try
			{
				if (_networkInterface == null)
				{
					return 0;
				}
				IPv4InterfaceStatistics iPv4Statistics = _networkInterface.GetIPv4Statistics();
				returnValue = (iPv4Statistics.BytesSent, iPv4Statistics.BytesReceived);
			}
			catch (Exception)
			{
				return 0;
			}
			return 0;
		});
		return returnValue;
	}

	public static NetworkInterface GetNetworkInterface()
	{
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

	public Task Login(string userName, string password)
	{
		return Task.Run(async delegate
		{
			await GetStreamingLocations();
			_connector.SetCredentials(userName, password);
		});
	}

	public Task RemoveVpn()
	{
		return Task.Run(async () => await _connector.RemoveConnectionAsync());
	}

	public Task<bool> Connect(IStreamingLocation streamingLocation)
	{
		return Task.Run(async delegate
		{
			if (streamingLocation == null)
			{
				SetConnectionError("Streaming connection error. Method: Connect. Message: streamingLocation == null");
				return false;
			}
			VpnConnectionStatus = StreamingConnectionStatus.Connecting;
			VpnConnectionStatusChanged(StreamingConnectionStatus.Connecting);
			if (await _connector.TryConnect(streamingLocation))
			{
				VpnConnectionStatus = StreamingConnectionStatus.Connected;
				VpnConnectionStatusChanged(StreamingConnectionStatus.Connected);
				LastConnectedLocation = streamingLocation;
				Logger.Log.Information("[Streaming] Streaming connected", "Connect", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SdkManager.cs", 169);
				return true;
			}
			SetConnectionError("[Streaming] Streaming connection error. Can't connect to the location " + streamingLocation.FullLocationName);
			throw new StreamingConnectionException("Streaming connection error. Can't connect to the location " + streamingLocation.FullLocationName);
		});
	}

	private void SetConnectionError(string message)
	{
		Logger.Log.Error(message, "SetConnectionError", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SdkManager.cs", 180);
		VpnConnectionStatus = StreamingConnectionStatus.Disconnected;
		VpnConnectionStatusChanged(StreamingConnectionStatus.Disconnected);
		Error("Streaming connection error");
	}

	public Task<bool> Disconnect()
	{
		return Task.Run(async delegate
		{
			try
			{
				VpnConnectionStatus = StreamingConnectionStatus.Disconnecting;
				VpnConnectionStatusChanged(StreamingConnectionStatus.Disconnecting);
				bool num = await _connector.TryDisconnectAsync();
				if (num)
				{
					VpnConnectionStatus = StreamingConnectionStatus.Disconnected;
					VpnConnectionStatusChanged(StreamingConnectionStatus.Disconnected);
					Logger.Log.Information("[Streaming] Streaming Disconnected", "Disconnect", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\SdkManager.cs", 199);
				}
				else
				{
					Error("Streaming disconnect error");
				}
				return num;
			}
			catch (Exception ex)
			{
				Error("Streaming disconnect error: " + ex.Message);
				return false;
			}
		});
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed || !disposing)
		{
			return;
		}
		_disposed = true;
		try
		{
			_connector.TryDisconnect();
		}
		finally
		{
			_connector.RemoveConnection();
		}
	}
}
