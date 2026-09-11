using System.Collections.Generic;
using System.Threading.Tasks;
using NextAiVPN.Enums.Streaming;
using NextAiVPN.Streaming.Entities;

namespace NextAiVPN.Entities.Streaming;

public interface IStreamingLocation
{
	string Id { get; }

	string CountryCode { get; }

	string FullLocationName { get; }

	string CityCode { get; }

	string SearchName { get; }

	ushort? PingMs { get; set; }

	string Country { get; }

	string City { get; }

	IList<Protocol> AvailableProtocols { get; }

	IList<IServer> Servers { get; }

	Task<ushort?> Ping();
}
