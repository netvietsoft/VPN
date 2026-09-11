namespace VpnSDK.DTO;

public class WireguardConfigurationStandalone : WireguardConfiguration
{
	public string ApiKey { get; set; }

	public string[] ApiBaseUrls { get; set; }
}
