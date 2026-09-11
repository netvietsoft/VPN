using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VpnSDK.Enums;
using VpnSDK.Interfaces;
using VpnSDK.Internal.Configuration;
using VpnSDK.Internal.Helpers;
using VpnSDK.Private.API.DTO;

namespace VpnSDK;

internal class SDKCoreMockup : SDKCore
{
	internal SDKCoreMockup(ISDKConfiguration sdkConfiguration)
		: base(sdkConfiguration)
	{
	}

	public override async Task Connect(ILocation location, IConnectionConfiguration connectionConfiguration, CancellationToken cancellationToken = default(CancellationToken))
	{
		IList<Server> list = LocationsHelper.LocationToServers(base.Locations, new List<ILocation> { location }, _lastKnownUserLocation);
		if (list == null || list.Count <= 0)
		{
			throw new NullLocationException("Unable to get server object.");
		}
		if (base.CurrentConnectionStatus == ConnectionStatus.Connecting)
		{
			throw new InvalidOperationException("Connection already in progress.");
		}
		OnVpnConnectionStatusChanged(ConnectionStatus.Connecting);
		try
		{
			await Task.Delay(500, cancellationToken);
		}
		catch
		{
			OnVpnConnectionStatusChanged(ConnectionStatus.Disconnected);
			throw;
		}
		OnVpnConnectionStatusChanged(ConnectionStatus.Connected);
	}

	public override async Task Disconnect()
	{
		OnVpnConnectionStatusChanged(ConnectionStatus.Disconnecting);
		await Task.Delay(100);
		OnVpnConnectionStatusChanged(ConnectionStatus.Disconnected);
	}

	public override async Task<DriverInstallResult> InstallTapDriver()
	{
		OnTapDeviceInstallationStatusChanged(OperationStatus.InProgress);
		await Task.Delay(100);
		OnTapDeviceInstallationStatusChanged(OperationStatus.Completed);
		await Task.Delay(100);
		return DriverInstallResult.Success;
	}
}
