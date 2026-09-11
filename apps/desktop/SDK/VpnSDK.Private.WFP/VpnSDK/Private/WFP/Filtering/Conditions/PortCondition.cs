using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using VpnSDK.Private.WFP.Interop;

namespace VpnSDK.Private.WFP.Filtering.Conditions;

public class PortCondition : FilterCondition, IDisposable
{
	private IntPtr _portRange = IntPtr.Zero;

	private readonly ushort portStart;

	private readonly ushort portEnd;

	public IEnumerable<ushort> Ports => from x in Enumerable.Range(portStart, portEnd)
		select (ushort)x;

	public PortCondition(ushort portNumber)
	{
		Condition.fieldKey = WFPGuids.FWPM_CONDITION_IP_REMOTE_PORT;
		Condition.conditionValue = portNumber;
		portStart = portNumber;
		portEnd = portNumber;
	}

	public PortCondition(ushort portRangeStart, ushort portRangeEnd)
	{
		Condition.fieldKey = WFPGuids.FWPM_CONDITION_IP_REMOTE_PORT;
		portStart = portRangeStart;
		portEnd = portRangeEnd;
		if (portRangeStart > portRangeEnd)
		{
			throw new ArgumentException("Invalid port range.");
		}
		FWP_RANGE0 structure = new FWP_RANGE0
		{
			valueLow = portRangeStart,
			valueHigh = portRangeEnd
		};
		_portRange = Marshal.AllocHGlobal(Marshal.SizeOf<FWP_RANGE0>());
		Marshal.StructureToPtr(structure, _portRange, fDeleteOld: true);
		Condition.conditionValue.type = FWP_DATA_TYPE.FWP_RANGE_TYPE;
		Condition.conditionValue.Union.rangeValue = _portRange;
	}

	private void ReleaseUnmanagedResources()
	{
		if (_portRange != IntPtr.Zero)
		{
			Marshal.DestroyStructure<FWP_RANGE0>(_portRange);
			Marshal.FreeHGlobal(_portRange);
			_portRange = IntPtr.Zero;
		}
	}

	public void Dispose()
	{
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~PortCondition()
	{
		ReleaseUnmanagedResources();
	}
}
