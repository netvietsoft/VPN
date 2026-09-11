using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using VpnSDK.Internal.Extensions;

namespace VpnSDK.Internal.Helpers;

internal static class NetworkComponentFinder
{
	private static readonly List<NetworkComponent> ComponentRegistry = new List<NetworkComponent>();

	public static NetworkComponent GetComponentById(string id)
	{
		return GetComponentsById(id).FirstOrDefault();
	}

	public static bool GetComponentsById(string[] ids, out NetworkComponent[] foundComponents)
	{
		ids = ids.Where((string x) => !string.IsNullOrEmpty(x)).ToArray();
		foundComponents = (from x in ComponentRegistry
			where x.IsValid
			where ids.Any((string y) => y.StartsWith(x.ComponentId, StringComparison.OrdinalIgnoreCase))
			select x).ToArray();
		return foundComponents.Length != 0;
	}

	public static NetworkComponent[] GetComponentsById(params string[] ids)
	{
		GetComponentsById(ids, out var foundComponents);
		return foundComponents;
	}

	public static void Update()
	{
		try
		{
			RegistryKey classRoot = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e972-e325-11ce-bfc1-08002be10318}\\", writable: false);
			if (classRoot == null)
			{
				return;
			}
			lock (ComponentRegistry)
			{
				ComponentRegistry.Clear();
				ComponentRegistry.AddRange(from x in (from x in classRoot.GetSubKeyNames()
						where char.IsDigit(x[0])
						select x).Select(delegate(string x)
					{
						classRoot.TryOpenSubKey(x, out var registryKey);
						return registryKey;
					})
					where x != null
					select new NetworkComponent(x) into x
					where x.IsValid
					select x);
			}
		}
		catch
		{
		}
	}
}
