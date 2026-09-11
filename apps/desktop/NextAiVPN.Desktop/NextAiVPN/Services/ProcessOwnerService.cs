using System;
using System.Management;

namespace NextAiVPN.Services;

internal class ProcessOwnerService : IProcessOwnerService
{
	public string GetProcessOwner(int processId)
	{
		using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("Select * From Win32_Process Where ProcessID = " + processId))
		{
			using ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();
			foreach (ManagementObject item in managementObjectCollection)
			{
				using (item)
				{
					string[] array = new string[2]
					{
						string.Empty,
						string.Empty
					};
					object[] args = array;
					if (Convert.ToInt32(item.InvokeMethod("GetOwner", args)) == 0)
					{
						return array[1] + "\\" + array[0];
					}
				}
			}
		}
		return null;
	}

	public string GetProcessOwner(string processName)
	{
		using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("Select * from Win32_Process Where Name = \"" + processName + "\""))
		{
			using ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();
			foreach (ManagementObject item in managementObjectCollection)
			{
				using (item)
				{
					string[] array = new string[2]
					{
						string.Empty,
						string.Empty
					};
					object[] args = array;
					if (Convert.ToInt32(item.InvokeMethod("GetOwner", args)) == 0)
					{
						return array[1] + "\\" + array[0];
					}
				}
			}
		}
		return null;
	}
}
