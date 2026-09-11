using System;

namespace VpnSDK.Private.API.Wireguard;

[Serializable]
internal struct KeyPair
{
	public string Public;

	public string Private;

	public byte[] GetPublicKey()
	{
		return Convert.FromBase64String(Public.ToString());
	}

	public byte[] GetPrivateKey()
	{
		return Convert.FromBase64String(Public.ToString());
	}
}
