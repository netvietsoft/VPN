namespace VpnSDK.Common.Settings;

public class DoubleHopSettings
{
	public bool IsDoubleHopEnabled { get; set; }

	public string EntryCountryCode { get; set; }

	public string EntryCity { get; set; }

	public string ExitCountryCode { get; set; }

	public string ExitCity { get; set; }

	public string Protocol { get; set; }

	public DoubleHopSettings(bool isDoubleHopEnabled, string entryCountryCode, string entryCity, string exitCountryCode, string exitCity, string protocol)
	{
		IsDoubleHopEnabled = isDoubleHopEnabled;
		EntryCountryCode = entryCountryCode;
		EntryCity = entryCity;
		ExitCountryCode = exitCountryCode;
		ExitCity = exitCity;
		Protocol = protocol;
	}
}
