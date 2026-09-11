using System;

namespace Microsoft.WindowsAPICodePack.Net;

[Flags]
public enum NetworkPropertyChange
{
	Connection = 1,
	Description = 2,
	Icon = 8,
	Name = 4,
	Category = 0x10
}
