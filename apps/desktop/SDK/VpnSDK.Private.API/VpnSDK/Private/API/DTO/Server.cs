using System.Diagnostics;
using System.Net;

namespace VpnSDK.Private.API.DTO;

[DebuggerDisplay("{Hostname}")]
public class Server : Node
{
	private short _isLoad;

	public override string Id
	{
		get
		{
			string hostname = Hostname;
			if (hostname == null)
			{
				return null;
			}
			return hostname.Split('.')[0];
		}
	}

	public string Hostname { get; private set; }

	public IPAddress IP { get; private set; }

	public bool InMaintenance { get; private set; }

	public ScheduledMaintenance ScheduledMaintenance { get; private set; }

	public short Load
	{
		get
		{
			return _isLoad;
		}
		set
		{
			if (value <= 0 || value >= 100)
			{
				InMaintenance = true;
			}
			_isLoad = value;
		}
	}

	public VpnConfiguration Configuration { get; private set; }

	public static Server Create(string hostname, IPAddress ip, bool inMaintenance, ScheduledMaintenance scheduledMaintenance, short serverLoad, VpnConfiguration configuration)
	{
		Server server = new Server
		{
			Hostname = hostname,
			IP = ip,
			InMaintenance = inMaintenance,
			ScheduledMaintenance = scheduledMaintenance,
			Load = serverLoad,
			Configuration = configuration
		};
		if (serverLoad <= 0 || serverLoad >= 100)
		{
			server.Load = 100;
			server.InMaintenance = true;
		}
		return server;
	}

	public bool ShouldSerializeScheduledMaintenance()
	{
		return ScheduledMaintenance != null;
	}
}
