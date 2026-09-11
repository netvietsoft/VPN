using System.Text;

namespace VpnSDK.Private.API.Wireguard.DTO;

public class WireguardConfiguration
{
	public Interface Interface { get; set; }

	public Peer Peer { get; set; }

	public override string ToString()
	{
		if (Interface == null || Peer == null)
		{
			return base.ToString();
		}
		return ToWireGuardConfig();
	}

	public string ToWireGuardConfig()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(Interface.ToString());
		stringBuilder.AppendLine(Peer.ToString());
		return stringBuilder.ToString().Trim();
	}
}
