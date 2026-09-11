using System;
using Microsoft.WindowsAPICodePack.Net;

namespace MS.WindowsAPICodePack.Internal;

internal delegate void NetworkConnectionConnectivityChangedEvent(Guid networkConnectionId, ConnectivityStates newConnectivity);
