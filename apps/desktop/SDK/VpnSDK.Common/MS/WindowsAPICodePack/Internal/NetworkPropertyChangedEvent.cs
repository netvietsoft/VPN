using System;
using Microsoft.WindowsAPICodePack.Net;

namespace MS.WindowsAPICodePack.Internal;

internal delegate void NetworkPropertyChangedEvent(Guid networkId, NetworkPropertyChange flags);
