using VpnSDK.Enums;
using VpnSDK.Interfaces;

namespace VpnSDK;

public delegate void SDKOperationEventHandler<in T>(ISDK sender, OperationStatus status, T args);
public delegate void SDKOperationEventHandler(ISDK sender, OperationStatus status);
