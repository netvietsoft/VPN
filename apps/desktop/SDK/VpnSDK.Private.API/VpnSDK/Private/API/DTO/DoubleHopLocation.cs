using Newtonsoft.Json;

namespace VpnSDK.Private.API.DTO;

public class DoubleHopLocation
{
	[JsonProperty("country")]
	public string Country { get; set; }

	[JsonProperty("city")]
	public string City { get; set; }

	public DoubleHopLocation(string country, string city)
	{
		Country = country;
		City = city;
	}

	public static DoubleHopLocation Create(string countryCode, string city)
	{
		return new DoubleHopLocation(countryCode, city);
	}
}
