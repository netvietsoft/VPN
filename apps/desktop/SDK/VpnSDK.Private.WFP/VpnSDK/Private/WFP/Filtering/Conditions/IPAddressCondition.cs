using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using VpnSDK.Private.WFP.Interop;
using VpnSDK.Private.WFP.Utilities;

namespace VpnSDK.Private.WFP.Filtering.Conditions;

public class IPAddressCondition : FilterCondition, IDisposable
{
	private IntPtr _address = IntPtr.Zero;

	public IPAddress Address { get; private set; }

	public IPAddress SubnetMask { get; private set; }

	public byte Netmask { get; private set; }

	public IPAddressCondition(IPAddress address, IPAddress subnetMask)
	{
		Address = address;
		SubnetMask = subnetMask;
		Condition.fieldKey = WFPGuids.FWPM_CONDITION_IP_REMOTE_ADDRESS;
		if (address.AddressFamily != AddressFamily.InterNetwork)
		{
			throw new ArgumentException("IP address is not IPv4.");
		}
		FWP_V4_ADDR_AND_MASK structure = new FWP_V4_ADDR_AND_MASK
		{
			addr = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(address.GetAddressBytes(), 0)),
			mask = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(subnetMask.GetAddressBytes(), 0))
		};
		_address = Marshal.AllocHGlobal(Marshal.SizeOf<FWP_V4_ADDR_AND_MASK>());
		Marshal.StructureToPtr(structure, _address, fDeleteOld: true);
		Condition.conditionValue.type = FWP_DATA_TYPE.FWP_V4_ADDR_MASK;
		Condition.conditionValue.Union.v4AddrMask = _address;
	}

	public IPAddressCondition(IPAddress address, byte netmask = byte.MaxValue)
	{
		Address = address;
		Netmask = netmask;
		Condition.fieldKey = WFPGuids.FWPM_CONDITION_IP_REMOTE_ADDRESS;
		if (address.AddressFamily == AddressFamily.InterNetwork)
		{
			if (netmask > 32)
			{
				netmask = 32;
			}
			SubnetMask = NetmaskTools.CreateByNetBitLength(netmask);
			FWP_V4_ADDR_AND_MASK structure = new FWP_V4_ADDR_AND_MASK
			{
				addr = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(address.GetAddressBytes(), 0)),
				mask = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(SubnetMask.GetAddressBytes(), 0))
			};
			_address = Marshal.AllocHGlobal(Marshal.SizeOf<FWP_V4_ADDR_AND_MASK>());
			Marshal.StructureToPtr(structure, _address, fDeleteOld: true);
			Condition.conditionValue.type = FWP_DATA_TYPE.FWP_V4_ADDR_MASK;
			Condition.conditionValue.Union.v4AddrMask = _address;
			return;
		}
		if (address.AddressFamily == AddressFamily.InterNetworkV6)
		{
			if (netmask > 128)
			{
				netmask = 128;
			}
			FWP_V6_ADDR_AND_MASK structure2 = new FWP_V6_ADDR_AND_MASK
			{
				prefixLength = netmask,
				addr = address.GetAddressBytes()
			};
			_address = Marshal.AllocHGlobal(Marshal.SizeOf<FWP_V6_ADDR_AND_MASK>());
			Marshal.StructureToPtr(structure2, _address, fDeleteOld: true);
			Condition.conditionValue.type = FWP_DATA_TYPE.FWP_V6_ADDR_MASK;
			Condition.conditionValue.Union.v6AddrMask = _address;
			return;
		}
		throw new ArgumentException("IP address provided was not either IPv4 or IPv6.");
	}

	private void ReleaseUnmanagedResources()
	{
		if (_address != IntPtr.Zero)
		{
			switch (Condition.conditionValue.type)
			{
			case FWP_DATA_TYPE.FWP_V4_ADDR_MASK:
				Marshal.DestroyStructure<FWP_V4_ADDR_AND_MASK>(_address);
				break;
			case FWP_DATA_TYPE.FWP_V6_ADDR_MASK:
				Marshal.DestroyStructure<FWP_V6_ADDR_AND_MASK>(_address);
				break;
			}
			Marshal.FreeHGlobal(_address);
			_address = IntPtr.Zero;
		}
	}

	public void Dispose()
	{
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~IPAddressCondition()
	{
		ReleaseUnmanagedResources();
	}
}
