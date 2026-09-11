using System.Collections.Generic;
using VpnSDK.Private.API.Common.Enums;

namespace VpnSDK.Private.API.DTO;

public class OpenVpnServerConfiguration
{
	public Dictionary<EncryptionLevel, List<ushort>> Ports { get; internal set; }

	public string ScramblePassphrase { get; internal set; }

	public OpenVpnServerConfiguration()
	{
		ScramblePassphrase = "Crash&Burn";
		Ports = new Dictionary<EncryptionLevel, List<ushort>>
		{
			{
				EncryptionLevel.None,
				new List<ushort> { 8080 }
			},
			{
				EncryptionLevel.Normal,
				new List<ushort> { 8443 }
			},
			{
				EncryptionLevel.High,
				new List<ushort> { 443, 1194 }
			},
			{
				EncryptionLevel.Scrambled,
				new List<ushort> { 3074 }
			}
		};
	}
}
