using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NextAiVPN.Services.Persistence;

internal class SingleInstanceDetector : ISingleInstanceDetector
{
	public bool IsOneInstanceForEachUser()
	{
		Process[] processesByName = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
		if (processesByName.Length <= 1)
		{
			return true;
		}
		List<string> list = new List<string>();
		Process[] array = processesByName;
		foreach (Process process in array)
		{
			string processOwner = new ProcessOwnerService().GetProcessOwner(process.Id);
			if (!string.IsNullOrEmpty(processOwner))
			{
				list.Add(processOwner);
			}
		}
		foreach (IGrouping<string, string> item in from x in list
			group x by x)
		{
			if (item.Count() > 1)
			{
				return false;
			}
		}
		return true;
	}
}
