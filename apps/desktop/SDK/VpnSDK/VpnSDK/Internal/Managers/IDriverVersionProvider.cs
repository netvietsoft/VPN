using System;
using VpnSDK.Enums;

namespace VpnSDK.Internal.Managers;

internal interface IDriverVersionProvider
{
	Version GetVersion(Driver driver);
}
