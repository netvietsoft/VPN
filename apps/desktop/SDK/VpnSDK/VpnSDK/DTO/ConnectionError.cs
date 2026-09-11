using System;
using VpnSDK.Interfaces;

namespace VpnSDK.DTO;

public class ConnectionError : ISDKError
{
	public string Hostname { get; internal set; }

	public string Ip { get; internal set; }

	public string CityCode { get; internal set; }

	public string CountryCode { get; internal set; }

	public IConnectionConfiguration ConnectionConfiguration { get; internal set; }

	public Exception SystemException { get; internal set; }

	public ConnectionError(string hostname, string ip, string cityCode, string countryCode, IConnectionConfiguration connectionConfiguration, Exception systemException)
	{
		Hostname = hostname;
		Ip = ip;
		CityCode = cityCode;
		CountryCode = countryCode;
		ConnectionConfiguration = connectionConfiguration;
		SystemException = systemException;
	}
}
