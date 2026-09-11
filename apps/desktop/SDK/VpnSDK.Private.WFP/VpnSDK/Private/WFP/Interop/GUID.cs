using System;
using System.Runtime.InteropServices;

namespace VpnSDK.Private.WFP.Interop;

internal struct GUID : IEquatable<GUID>
{
	public uint Data1;

	public ushort Data2;

	public ushort Data3;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
	public byte[] Data4;

	public static implicit operator GUID(Guid value)
	{
		GUID result = default(GUID);
		byte[] array = value.ToByteArray();
		result.Data1 = BitConverter.ToUInt32(array, 0);
		result.Data2 = BitConverter.ToUInt16(array, 4);
		result.Data3 = BitConverter.ToUInt16(array, 6);
		result.Data4 = new byte[8];
		Array.Copy(array, 8, result.Data4, 0, 8);
		return result;
	}

	public bool Equals(GUID other)
	{
		if (Data1 == other.Data1 && Data2 == other.Data2 && Data3 == other.Data3)
		{
			return object.Equals(Data4, other.Data4);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is GUID)
		{
			return Equals((GUID)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		return string.Format("{0:X}-{1:X}-{2:X}-{3}", new object[4]
		{
			Data1,
			Data2,
			Data3,
			BitConverter.ToString(Data4).Replace("-", string.Empty)
		}).ToLowerInvariant();
	}

	public static bool operator ==(GUID left, GUID right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(GUID left, GUID right)
	{
		return !left.Equals(right);
	}
}
