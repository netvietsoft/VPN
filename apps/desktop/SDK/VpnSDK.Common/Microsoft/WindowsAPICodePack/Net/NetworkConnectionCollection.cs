using System.Collections;
using System.Collections.Generic;

namespace Microsoft.WindowsAPICodePack.Net;

internal class NetworkConnectionCollection : IEnumerable<NetworkConnection>, IEnumerable
{
	private IEnumerable networkConnectionEnumerable;

	public NetworkConnectionCollection(IEnumerable networkConnectionEnumerable)
	{
		this.networkConnectionEnumerable = networkConnectionEnumerable;
	}

	public IEnumerator<NetworkConnection> GetEnumerator()
	{
		foreach (INetworkConnection item in networkConnectionEnumerable)
		{
			yield return new NetworkConnection(item);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		foreach (INetworkConnection item in networkConnectionEnumerable)
		{
			yield return new NetworkConnection(item);
		}
	}
}
