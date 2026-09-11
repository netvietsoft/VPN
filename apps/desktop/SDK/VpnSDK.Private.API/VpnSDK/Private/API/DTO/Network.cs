using System;

namespace VpnSDK.Private.API.DTO;

[Serializable]
public class Network : Node
{
	public Network(string id)
	{
		Id = id;
	}

	public Network()
	{
	}
}
