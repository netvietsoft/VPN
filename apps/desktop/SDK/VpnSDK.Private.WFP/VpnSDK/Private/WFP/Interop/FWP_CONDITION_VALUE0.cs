using System;
using System.Runtime.InteropServices;
using VpnSDK.Private.WFP.Filtering.Conditions;

namespace VpnSDK.Private.WFP.Interop;

internal struct FWP_CONDITION_VALUE0
{
	[StructLayout(LayoutKind.Explicit)]
	public struct FWP_CONDITION_VALUE0_Union
	{
		[FieldOffset(0)]
		public byte uint8;

		[FieldOffset(0)]
		public ushort uint16;

		[FieldOffset(0)]
		public uint uint32;

		[FieldOffset(0)]
		public IntPtr uint64;

		[FieldOffset(0)]
		public sbyte int8;

		[FieldOffset(0)]
		public short int16;

		[FieldOffset(0)]
		public int int32;

		[FieldOffset(0)]
		public IntPtr int64;

		[FieldOffset(0)]
		public float float32;

		[FieldOffset(0)]
		public IntPtr double64;

		[FieldOffset(0)]
		public IntPtr byteArray16;

		[FieldOffset(0)]
		public IntPtr byteBlob;

		[FieldOffset(0)]
		public IntPtr sid;

		[FieldOffset(0)]
		public IntPtr sd;

		[FieldOffset(0)]
		public IntPtr tokenInformation;

		[FieldOffset(0)]
		public IntPtr tokenAccessInformation;

		[FieldOffset(0)]
		public IntPtr unicodeString;

		[FieldOffset(0)]
		public IntPtr byteArray6;

		[FieldOffset(0)]
		public IntPtr v4AddrMask;

		[FieldOffset(0)]
		public IntPtr v6AddrMask;

		[FieldOffset(0)]
		public IntPtr rangeValue;
	}

	public FWP_DATA_TYPE type;

	public FWP_CONDITION_VALUE0_Union Union;

	public static implicit operator FWP_CONDITION_VALUE0(byte value)
	{
		return new FWP_CONDITION_VALUE0
		{
			type = FWP_DATA_TYPE.FWP_UINT8,
			Union = new FWP_CONDITION_VALUE0_Union
			{
				uint8 = value
			}
		};
	}

	public static implicit operator FWP_CONDITION_VALUE0(sbyte value)
	{
		return new FWP_CONDITION_VALUE0
		{
			type = FWP_DATA_TYPE.FWP_INT8,
			Union = new FWP_CONDITION_VALUE0_Union
			{
				int8 = value
			}
		};
	}

	public static implicit operator FWP_CONDITION_VALUE0(int value)
	{
		return new FWP_CONDITION_VALUE0
		{
			type = FWP_DATA_TYPE.FWP_INT32,
			Union = new FWP_CONDITION_VALUE0_Union
			{
				int32 = value
			}
		};
	}

	public static implicit operator FWP_CONDITION_VALUE0(uint value)
	{
		return new FWP_CONDITION_VALUE0
		{
			type = FWP_DATA_TYPE.FWP_UINT32,
			Union = new FWP_CONDITION_VALUE0_Union
			{
				uint32 = value
			}
		};
	}

	public static implicit operator FWP_CONDITION_VALUE0(ushort value)
	{
		return new FWP_CONDITION_VALUE0
		{
			type = FWP_DATA_TYPE.FWP_UINT16,
			Union = new FWP_CONDITION_VALUE0_Union
			{
				uint16 = value
			}
		};
	}

	public static implicit operator FWP_CONDITION_VALUE0(TrafficDirection value)
	{
		return new FWP_CONDITION_VALUE0
		{
			type = FWP_DATA_TYPE.FWP_UINT32,
			Union = new FWP_CONDITION_VALUE0_Union
			{
				uint32 = (uint)value
			}
		};
	}

	public static implicit operator FWP_CONDITION_VALUE0(IPProtocol value)
	{
		return new FWP_CONDITION_VALUE0
		{
			type = FWP_DATA_TYPE.FWP_UINT8,
			Union = new FWP_CONDITION_VALUE0_Union
			{
				uint8 = (byte)value
			}
		};
	}
}
