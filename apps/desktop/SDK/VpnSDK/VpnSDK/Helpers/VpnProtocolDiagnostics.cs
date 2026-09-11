using System.Collections.Generic;
using System.IO;
using System.Linq;
using UACHelper;
using VpnSDK.DTO;
using VpnSDK.Enums;

namespace VpnSDK.Helpers;

public class VpnProtocolDiagnostics
{
	public static void DiagnoseProtocolsAvailability(OpenVpnConfiguration openVpnConfiguration, RasConfiguration rasConfiguration, WireguardConfiguration wireguardConfiguration, Dictionary<NetworkConnectionType, bool> availableProtocols)
	{
		if (openVpnConfiguration != null && (!File.Exists(Path.Combine(openVpnConfiguration.OpenVpnDirectory, openVpnConfiguration.OpenVpnExecutableFileName)) || !global::UACHelper.UACHelper.IsElevated) && availableProtocols.ContainsKey(NetworkConnectionType.OpenVPN))
		{
			availableProtocols[NetworkConnectionType.OpenVPN] = false;
		}
		if (wireguardConfiguration != null)
		{
			if (((wireguardConfiguration is WireguardConfigurationStandalone && string.IsNullOrEmpty(((WireguardConfigurationStandalone)wireguardConfiguration).ApiKey)) || string.IsNullOrEmpty(wireguardConfiguration.ConfigDirectory) || string.IsNullOrEmpty(wireguardConfiguration.ConnectionName) || !global::UACHelper.UACHelper.IsElevated) && availableProtocols.ContainsKey(NetworkConnectionType.WireGuard))
			{
				availableProtocols[NetworkConnectionType.WireGuard] = false;
			}
		}
		else if (availableProtocols.ContainsKey(NetworkConnectionType.OpenVPN))
		{
			availableProtocols[NetworkConnectionType.OpenVPN] = false;
		}
		if (rasConfiguration == null && availableProtocols.ContainsKey(NetworkConnectionType.IKEv2))
		{
			availableProtocols[NetworkConnectionType.IKEv2] = false;
		}
		if (wireguardConfiguration == null && availableProtocols.ContainsKey(NetworkConnectionType.WireGuard))
		{
			availableProtocols[NetworkConnectionType.WireGuard] = false;
		}
		if (availableProtocols.Count((KeyValuePair<NetworkConnectionType, bool> x) => x.Value) == 0)
		{
			throw new EmptyAvailableVpnProtocolsException("There are no VPN protocols available.");
		}
	}
}
