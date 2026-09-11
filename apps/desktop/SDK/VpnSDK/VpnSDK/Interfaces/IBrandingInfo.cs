using System;
using System.Collections.Generic;

namespace VpnSDK.Interfaces;

public interface IBrandingInfo
{
	string AppName { get; }

	DateTime? LastUpdatedAt { get; }

	Dictionary<string, string> Urls { get; }

	Dictionary<string, string> Colors { get; }
}
