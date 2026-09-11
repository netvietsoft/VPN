using System;
using System.Collections.Generic;
using System.Linq;
using DotRas;

namespace VpnSDK.Private.Ras.Utilities;

public static class RasProtocolAvailability
{
	public static List<RasConnectionType> Check()
	{
		try
		{
			return (from x in RasDevice.GetDevices()
				where x.DeviceType == RasDeviceType.Vpn
				select x.Name.Split(new char[2] { '(', ')' }).ElementAtOrDefault(1) into x
				select Enum.TryParse<RasConnectionType>(x, ignoreCase: true, out var result) ? new RasConnectionType?(result) : ((RasConnectionType?)null) into x
				where x.HasValue
				select x.Value).Distinct().ToList();
		}
		catch
		{
		}
		return new List<RasConnectionType>();
	}
}
