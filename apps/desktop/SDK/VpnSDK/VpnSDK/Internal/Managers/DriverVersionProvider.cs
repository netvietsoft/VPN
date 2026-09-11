using System;
using VpnSDK.Enums;
using VpnSDK.NetFilter.CalloutDriver;

namespace VpnSDK.Internal.Managers;

internal class DriverVersionProvider : IDriverVersionProvider
{
	private readonly ICalloutDriverService _calloutDriverService;

	private readonly OpenVpnManager _openVpnManager;

	public DriverVersionProvider(ICalloutDriverService calloutDriverService, OpenVpnManager openVpnManager)
	{
		_calloutDriverService = calloutDriverService;
		_openVpnManager = openVpnManager;
	}

	public Version GetVersion(Driver driver)
	{
		return driver switch
		{
			Driver.OpenVPN => _openVpnManager?.DriverVersion, 
			Driver.Callout => _calloutDriverService?.DriverVersion, 
			_ => null, 
		};
	}
}
