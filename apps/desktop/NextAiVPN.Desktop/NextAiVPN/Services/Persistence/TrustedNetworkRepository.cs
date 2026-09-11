using System;
using System.Collections.Generic;
using System.IO;

namespace NextAiVPN.Services.Persistence;

internal class TrustedNetworkRepository : ITrustedNetworkRepository
{
	private static readonly string _filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SystemInfo.AppName, "TrustedNetworks.tnfvpn");

	private readonly ISerializer<IEnumerable<string>> _serializer;

	public TrustedNetworkRepository(ISerializer<IEnumerable<string>> serializer)
	{
		_serializer = serializer;
	}

	public IEnumerable<string> GetNetworks()
	{
		return _serializer.Deserialize(_filePath) ?? new List<string>();
	}

	public void SetNetworks(IEnumerable<string> networks)
	{
		_serializer.Serialize(networks, _filePath);
	}
}
