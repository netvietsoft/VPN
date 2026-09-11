using System;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace VpnSDK.Private.API.DTO;

[DebuggerDisplay("{DebugDisplay}")]
public class Location : Node
{
	public string Name { get; private set; }

	public override string Id { get; set; }

	public Tuple<double, double> GeoCoordinate { get; private set; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string DebugDisplay
	{
		get
		{
			string arg = "Cities";
			if (base.Children?.FirstOrDefault() is Server)
			{
				arg = "Servers";
			}
			return $"Node {ToString()}, {arg}: {base.Children?.Count}";
		}
	}

	[JsonConstructor]
	public Location()
	{
	}

	public Location(string locationId, string locationName)
	{
		Id = locationId;
		Name = locationName;
	}

	public Location(string locationId, string locationName, double latitude, double longitude)
	{
		Id = locationId;
		Name = locationName;
		GeoCoordinate = new Tuple<double, double>(latitude, longitude);
	}

	public bool ShouldSerializeGeoCoordinate()
	{
		return GeoCoordinate != null;
	}

	public override string ToString()
	{
		return Name + ", " + Id;
	}
}
