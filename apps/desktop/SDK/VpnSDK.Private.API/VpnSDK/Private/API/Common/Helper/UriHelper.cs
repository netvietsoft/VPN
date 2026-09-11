using System;
using System.Linq;

namespace VpnSDK.Private.API.Common.Helper;

internal class UriHelper
{
	public static Uri TransformUri(Uri original, string newHost)
	{
		if (original == null)
		{
			throw new ArgumentNullException("original");
		}
		if (string.IsNullOrWhiteSpace(newHost))
		{
			throw new ArgumentException("New host cannot be null or whitespace.", "newHost");
		}
		string[] array = (from s in original.AbsolutePath.Split('/')
			where !string.IsNullOrEmpty(s)
			select s).ToArray();
		string text = ((array.Length != 0) ? array[^1] : "");
		return new UriBuilder(original)
		{
			Host = newHost,
			Path = (string.IsNullOrEmpty(text) ? "" : ("/" + text))
		}.Uri;
	}

	public static Uri ReplaceHost(Uri original, string newHost)
	{
		if (original == null)
		{
			throw new ArgumentNullException("original");
		}
		if (string.IsNullOrWhiteSpace(newHost))
		{
			throw new ArgumentException("New host cannot be null or whitespace.", "newHost");
		}
		return new UriBuilder(original)
		{
			Host = newHost
		}.Uri;
	}
}
