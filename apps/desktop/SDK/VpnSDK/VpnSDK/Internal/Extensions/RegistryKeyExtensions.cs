using Microsoft.Win32;

namespace VpnSDK.Internal.Extensions;

internal static class RegistryKeyExtensions
{
	public static bool TryOpenSubKey(this RegistryKey key, string subkey, out RegistryKey registryKey)
	{
		try
		{
			registryKey = key.OpenSubKey(subkey, writable: false);
			return true;
		}
		catch
		{
			registryKey = null;
			return false;
		}
	}

	public static T GetValue<T>(this RegistryKey key, string keyName)
	{
		try
		{
			return (T)key.GetValue(keyName, default(T));
		}
		catch
		{
			return default(T);
		}
	}
}
