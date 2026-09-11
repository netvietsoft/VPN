using System.Threading;

namespace VpnSDK.Core.Interfaces;

internal interface IFeature
{
	string Name { get; }

	IConfig Config { get; set; }

	SynchronizationContext SynchronizationContext { get; set; }

	OperationResult Start();

	OperationResult Stop();
}
