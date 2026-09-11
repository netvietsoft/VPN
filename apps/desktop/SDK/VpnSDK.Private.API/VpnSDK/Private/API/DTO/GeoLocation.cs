using Newtonsoft.Json;

namespace VpnSDK.Private.API.DTO;

public class GeoLocation
{
	[JsonProperty("country_name")]
	public string CountryName { get; set; }

	[JsonProperty("country_code")]
	public string CountryCode { get; set; }

	[JsonProperty("city_name")]
	public string CityName { get; set; }

	[JsonProperty("continent_name")]
	private string ContinentName { get; set; }

	[JsonProperty("latitude")]
	public decimal Latitude { get; set; }

	[JsonProperty("longitude")]
	public decimal Longitude { get; set; }

	[JsonProperty("region")]
	public string RegionName { get; set; }

	[JsonProperty("region_code")]
	public string RegionCode { get; set; }

	[JsonProperty("city")]
	internal string GeoCity
	{
		set
		{
			CityName = value;
		}
	}

	[JsonProperty("country")]
	internal string GeoCountry
	{
		set
		{
			CountryName = value;
		}
	}
}
