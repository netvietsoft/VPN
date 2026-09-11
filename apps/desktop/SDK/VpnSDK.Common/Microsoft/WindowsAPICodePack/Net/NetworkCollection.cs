using System.Collections;
using System.Collections.Generic;

namespace Microsoft.WindowsAPICodePack.Net;

internal class NetworkCollection : IEnumerable<Network>, IEnumerable
{
	private IEnumerable networkEnumerable;

	public NetworkCollection(IEnumerable networkEnumerable)
	{
		this.networkEnumerable = networkEnumerable;
	}

	public IEnumerator<Network> GetEnumerator()
	{
		foreach (INetwork item in networkEnumerable)
		{
			yield return new Network(item);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		foreach (INetwork item in networkEnumerable)
		{
			yield return new Network(item);
		}
	}
}
