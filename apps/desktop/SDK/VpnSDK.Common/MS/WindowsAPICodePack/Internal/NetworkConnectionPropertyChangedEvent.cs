using System;
using Microsoft.WindowsAPICodePack.Net;

namespace MS.WindowsAPICodePack.Internal;

internal delegate void NetworkConnectionPropertyChangedEvent(Guid networkConnectionId, NetworkConnectionPropertyChange flags);
