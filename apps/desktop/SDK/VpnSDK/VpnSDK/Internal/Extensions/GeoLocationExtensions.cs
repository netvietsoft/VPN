using System;
using GeoCoordinatePortable;
using VpnSDK.DTO;
using VpnSDK.Private.API.DTO;

namespace VpnSDK.Internal.Extensions;

internal static class GeoLocationExtensions
{
	internal static readonly GeoCoordinate CenterOfAmerica = new GeoCoordinate(39.8283, -98.5795);

	public static GeoCoordinate ToGeocoordinate(this GeoLocation geoLocation)
	{
		if (geoLocation != null)
		{
			_ = geoLocation.Latitude;
			if (0 == 0 && IsWithinValidRange(geoLocation.Latitude, geoLocation.Longitude))
			{
				return new GeoCoordinate((double)geoLocation.Latitude, (double)geoLocation.Longitude);
			}
		}
		return CenterOfAmerica;
	}

	public static GeoCoordinate ToGeoCoordinate(this Tuple<double, double> geoLocation)
	{
		if (geoLocation == null || !IsWithinValidRange(geoLocation.Item1, geoLocation.Item2))
		{
			return CenterOfAmerica;
		}
		return new GeoCoordinate(geoLocation.Item1, geoLocation.Item2);
	}

	public static GeoCoordinate ToGeoCoordinate(this NetworkGeolocation geolocation)
	{
		if (!geolocation.IsValid())
		{
			return CenterOfAmerica;
		}
		return new GeoCoordinate((double)geolocation.Latitude, (double)geolocation.Longitude);
	}

	public static bool IsValid(this NetworkGeolocation geolocation)
	{
		if (geolocation == null || !IsWithinValidRange(geolocation.Latitude, geolocation.Longitude))
		{
			return false;
		}
		return true;
	}

	private static bool IsWithinValidRange(double latitude, double longitude)
	{
		if (latitude >= -90.0 && latitude <= 90.0 && longitude >= -180.0 && longitude <= 180.0 && !double.IsNaN(latitude) && !double.IsNaN(longitude) && latitude != 0.0)
		{
			return longitude != 0.0;
		}
		return false;
	}

	private static bool IsWithinValidRange(decimal latitude, decimal longitude)
	{
		return IsWithinValidRange((double)latitude, (double)longitude);
	}
}
