using System.Net;
using System.Net.Sockets;
using VpnSDK.Private.API.DTO;

namespace VpnSDK.DTO;

public class NetworkGeolocation
{
	public decimal Latitude { get; internal set; }

	public decimal Longitude { get; internal set; }

	public string Country { get; internal set; }

	public string City { get; internal set; }

	public string CountryCode { get; internal set; }

	public IPAddress IPAddress { get; internal set; }

	public static implicit operator NetworkGeolocation(GeoIP geoIp)
	{
		if (geoIp?.IP == null)
		{
			return null;
		}
		return new NetworkGeolocation
		{
			City = geoIp.Location?.CityName,
			Country = geoIp.Location?.CountryName,
			CountryCode = geoIp.Location?.CountryCode,
			Latitude = (geoIp.Location?.Latitude ?? 0m),
			Longitude = (geoIp.Location?.Longitude ?? 0m),
			IPAddress = geoIp.IP
		};
	}

	public override string ToString()
	{
		return string.Format("({0}) {1} ({2}, {3})", new object[4]
		{
			(IPAddress.AddressFamily == AddressFamily.InterNetworkV6) ? "IPv6" : "IPv4",
			IPAddress,
			Country,
			City
		});
	}
}
