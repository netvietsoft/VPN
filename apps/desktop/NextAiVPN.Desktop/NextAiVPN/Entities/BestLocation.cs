using VpnSDK.Interfaces;

namespace NextAiVPN.Entities;

internal class BestLocation
{
	public string ConnectionString { get; set; }

	public string Country { get; set; }

	public string City { get; set; }

	public ILocation Location { get; set; }
}
