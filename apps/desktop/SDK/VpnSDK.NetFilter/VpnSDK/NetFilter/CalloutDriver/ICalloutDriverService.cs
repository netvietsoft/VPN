using System;

namespace VpnSDK.NetFilter.CalloutDriver;

public interface ICalloutDriverService
{
	Version DriverVersion { get; }

	bool Start(bool waitForStart = true);

	void Remove(bool waitForStop);

	void Stop(bool waitForStop);

	bool IsServiceRunning();
}
