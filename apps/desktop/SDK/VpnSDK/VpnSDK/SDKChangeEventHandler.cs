using VpnSDK.Interfaces;

namespace VpnSDK;

public delegate void SDKChangeEventHandler<in T>(ISDK sender, T previous, T current);
