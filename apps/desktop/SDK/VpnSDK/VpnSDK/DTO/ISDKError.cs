using System;

namespace VpnSDK.DTO;

public interface ISDKError
{
	Exception SystemException { get; }
}
