using VpnSDK.Interfaces;

namespace VpnSDK;

public delegate void SDKEventHandler(ISDK sender);
public delegate void SDKEventHandler<in T>(ISDK sender, T args);
