using System;
using System.Linq;
using System.Security;

namespace VpnSDK.Private.API.Wireguard;

internal static class SecureStringExtensions
{
	internal static void AppendString(this SecureString secureStr, string s)
	{
		Array.ForEach(s.ToArray(), secureStr.AppendChar);
	}

	internal static SecureString ToSecureString(this string s)
	{
		SecureString secureString = new SecureString();
		secureString.AppendString(s);
		return secureString;
	}
}
