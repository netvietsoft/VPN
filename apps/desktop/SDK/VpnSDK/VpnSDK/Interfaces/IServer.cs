using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

namespace VpnSDK.Interfaces;

public interface IServer : ILocation, INotifyPropertyChanged
{
	string Hostname { get; }

	IPAddress Ip { get; }

	short Load { get; }

	List<ushort> OpenVpnScramblePorts { get; }

	List<ushort> OpenVpnPorts { get; }
}
