using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Microsoft.Win32;
using VpnSDK.Common.Helpers;
using VpnSDK.Internal.Extensions;

namespace VpnSDK.Internal.Helpers;

internal class NetworkComponent : IEquatable<NetworkComponent>
{
	public string Name { get; private set; }

	public string Provider { get; private set; }

	public string ComponentId { get; private set; }

	public bool IsValid
	{
		get
		{
			if (!string.IsNullOrEmpty(Name))
			{
				return !string.IsNullOrEmpty(ComponentId);
			}
			return false;
		}
	}

	public NetworkInterface AssociatedInterface => AssociatedInterfaces.First();

	public IEnumerable<NetworkInterface> AssociatedInterfaces => from x in NetworkInterfaceHelper.GetAllNetworkInterfacesSafely()
		where x.Description.Equals(Name, StringComparison.OrdinalIgnoreCase)
		select x;

	public NetworkComponent(RegistryKey registryKey)
	{
		Name = registryKey.GetValue<string>("ProductName") ?? registryKey.GetValue<string>("DriverDesc");
		Provider = registryKey.GetValue<string>("ProviderName");
		ComponentId = registryKey.GetValue<string>("ComponentId") ?? registryKey.GetValue<string>("MatchingDeviceId");
	}

	public static bool operator ==(NetworkComponent component1, NetworkComponent component2)
	{
		return component1?.Equals(component2) ?? ((object)component2 == null);
	}

	public static bool operator !=(NetworkComponent component1, NetworkComponent component2)
	{
		return !(component1 == component2);
	}

	public bool Equals(NetworkComponent other)
	{
		if (!IsValid || (object)other == null || !other.IsValid)
		{
			return false;
		}
		return Name.Equals(other.Name);
	}

	public override int GetHashCode()
	{
		return ((((unchecked(522698701 * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Name)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Provider)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ComponentId)) * -1521134295 + EqualityComparer<NetworkInterface>.Default.GetHashCode(AssociatedInterface)) * -1521134295 + EqualityComparer<IEnumerable<NetworkInterface>>.Default.GetHashCode(AssociatedInterfaces);
	}
}
