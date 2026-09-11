using VpnSDK.Interfaces;
using VpnSDK.Private.API;

namespace VpnSDK;

public class ConnectionConfigurationBuilderBase
{
	protected bool IsDoubleHopEnabled;

	protected ILocation EntryLocation;

	protected ILocation ExitLocation;

	protected virtual void Validate()
	{
		if (IsDoubleHopEnabled)
		{
			if (EntryLocation == null)
			{
				throw new InvalidDoubleHopConfigurationException("Missing entry location.", ApiError.MissingEntryExitServers);
			}
			if (ExitLocation == null)
			{
				throw new InvalidDoubleHopConfigurationException("Missing exit location.", ApiError.MissingEntryExitServers);
			}
			if (string.IsNullOrEmpty(EntryLocation.CountryCode) || string.IsNullOrEmpty(EntryLocation.City))
			{
				throw new InvalidDoubleHopConfigurationException("Invalid entry location.", ApiError.InvalidEntryExitServers);
			}
			if (string.IsNullOrEmpty(ExitLocation.CountryCode) || string.IsNullOrEmpty(ExitLocation.City))
			{
				throw new InvalidDoubleHopConfigurationException("Invalid exit location.", ApiError.InvalidEntryExitServers);
			}
			if (EntryLocation.CountryCode.Equals(ExitLocation.CountryCode) && EntryLocation.City.Equals(ExitLocation.City))
			{
				throw new InvalidDoubleHopConfigurationException("The entry and exit locations cannot be the same.", ApiError.InvalidEntryExitServers);
			}
		}
	}
}
