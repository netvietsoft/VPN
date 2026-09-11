using System;
using Microsoft.WindowsAPICodePack.Net;

namespace MS.WindowsAPICodePack.Internal;

internal delegate void NetworkConnectivityChangedEvent(Guid networkId, ConnectivityStates newConnectivity);
