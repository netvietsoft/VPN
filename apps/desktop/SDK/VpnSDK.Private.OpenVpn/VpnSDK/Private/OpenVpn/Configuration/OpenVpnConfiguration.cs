using System.Collections.Generic;
using System.Text;

namespace VpnSDK.Private.OpenVpn.Configuration;

public class OpenVpnConfiguration : Dictionary<string, string[]>
{
	public OpenVpnConfiguration()
	{
	}

	public OpenVpnConfiguration(Dictionary<string, string[]> dict)
		: base((IDictionary<string, string[]>)dict)
	{
	}

	public string ToArguments()
	{
		StringBuilder stringBuilder = new StringBuilder();
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, string[]> current = enumerator.Current;
				stringBuilder.Append("--" + current.Key + " ");
				if (current.Value != null)
				{
					stringBuilder.Append(string.Join(" ", current.Value).Replace("\\", "\\\\") + " ");
				}
			}
		}
		return stringBuilder.ToString();
	}
}
