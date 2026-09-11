using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using VpnSDK.Private.WFP.Interop;

namespace VpnSDK.Private.WFP.Filtering.Conditions;

public class InterfaceCondition : FilterCondition, IDisposable
{
	private IntPtr _luidPtr;

	public NetworkInterface NetworkAdapter { get; private set; }

	public InterfaceType? InterfaceType
	{
		get
		{
			if (!(Condition.fieldKey != WFPGuids.FWPM_CONDITION_INTERFACE_TYPE))
			{
				return (InterfaceType)Condition.conditionValue.Union.uint32;
			}
			return null;
		}
	}

	public InterfaceCondition(NetworkInterface networkInterface, bool preferAdapterIndex = false)
	{
		if (networkInterface == null)
		{
			throw new ArgumentNullException("networkInterface");
		}
		if (!networkInterface.Supports(NetworkInterfaceComponent.IPv4) && !networkInterface.Supports(NetworkInterfaceComponent.IPv6))
		{
			throw new ArgumentException("Adapter provided does not support IPv4 nor IPv6.");
		}
		NetworkAdapter = networkInterface;
		if (preferAdapterIndex)
		{
			Condition.fieldKey = WFPGuids.FWPM_CONDITION_INTERFACE_INDEX;
			Condition.conditionValue.type = FWP_DATA_TYPE.FWP_UINT32;
			Condition.conditionValue.Union.uint32 = GetAdapterIndex(networkInterface);
			return;
		}
		Condition.fieldKey = WFPGuids.FWPM_CONDITION_IP_LOCAL_INTERFACE;
		List<int> list = new List<int>();
		IPInterfaceProperties iPProperties = networkInterface.GetIPProperties();
		if (networkInterface.Supports(NetworkInterfaceComponent.IPv6))
		{
			try
			{
				IPv6InterfaceProperties iPv6Properties = iPProperties.GetIPv6Properties();
				if (iPv6Properties != null)
				{
					list.Add(iPv6Properties.Index);
				}
			}
			catch (NetworkInformationException)
			{
			}
		}
		if (networkInterface.Supports(NetworkInterfaceComponent.IPv4))
		{
			try
			{
				IPv4InterfaceProperties iPv4Properties = iPProperties.GetIPv4Properties();
				if (iPv4Properties != null)
				{
					list.Add(iPv4Properties.Index);
				}
			}
			catch (NetworkInformationException)
			{
			}
		}
		if (list.Count == 0)
		{
			throw new InvalidOperationException("Network adapter was unable to provide IPv4 nor IPv6 information.");
		}
		IPHelperInterop.NET_LUID InterfaceLuid = default(IPHelperInterop.NET_LUID);
		using (List<int>.Enumerator enumerator = list.GetEnumerator())
		{
			while (enumerator.MoveNext() && IPHelperInterop.NetNativeMethods.ConvertInterfaceIndexToLuid((uint)enumerator.Current, ref InterfaceLuid) != 0)
			{
			}
		}
		Condition.conditionValue.type = FWP_DATA_TYPE.FWP_UINT64;
		_luidPtr = Marshal.AllocHGlobal(Marshal.SizeOf<ulong>());
		Marshal.StructureToPtr(InterfaceLuid.Value, _luidPtr, fDeleteOld: true);
		Condition.conditionValue.Union.uint64 = _luidPtr;
	}

	public InterfaceCondition(int adapterIndex)
	{
		Condition.fieldKey = WFPGuids.FWPM_CONDITION_INTERFACE_INDEX;
		Condition.conditionValue.type = FWP_DATA_TYPE.FWP_UINT32;
		Condition.conditionValue.Union.uint32 = (uint)adapterIndex;
	}

	public InterfaceCondition(InterfaceType interfaceType)
	{
		Condition.fieldKey = WFPGuids.FWPM_CONDITION_INTERFACE_TYPE;
		Condition.conditionValue.type = FWP_DATA_TYPE.FWP_UINT32;
		Condition.conditionValue.Union.uint32 = (uint)interfaceType;
	}

	private void ReleaseUnmanagedResources()
	{
		if (_luidPtr != IntPtr.Zero)
		{
			Marshal.DestroyStructure<ulong>(_luidPtr);
			Marshal.FreeHGlobal(_luidPtr);
			_luidPtr = IntPtr.Zero;
		}
	}

	public void Dispose()
	{
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~InterfaceCondition()
	{
		ReleaseUnmanagedResources();
	}

	private static uint GetAdapterIndex(NetworkInterface networkInterface)
	{
		if (networkInterface.Supports(NetworkInterfaceComponent.IPv6))
		{
			int? num = networkInterface.GetIPProperties()?.GetIPv6Properties()?.Index;
			if (num.HasValue)
			{
				return (uint)num.Value;
			}
		}
		if (networkInterface.Supports(NetworkInterfaceComponent.IPv4))
		{
			int? num2 = networkInterface.GetIPProperties()?.GetIPv4Properties()?.Index;
			if (num2.HasValue)
			{
				return (uint)num2.Value;
			}
		}
		throw new ArgumentException("Network interface has no index.");
	}
}
