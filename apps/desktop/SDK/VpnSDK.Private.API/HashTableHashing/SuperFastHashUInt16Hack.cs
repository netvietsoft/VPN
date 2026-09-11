using System.Runtime.InteropServices;

namespace HashTableHashing;

internal class SuperFastHashUInt16Hack
{
	[StructLayout(LayoutKind.Explicit)]
	private struct BytetoUInt16Converter
	{
		[FieldOffset(0)]
		public byte[] Bytes;

		[FieldOffset(0)]
		public ushort[] UInts;
	}

	public static uint Hash(byte[] dataToHash)
	{
		int num = dataToHash.Length;
		if (num == 0)
		{
			return 0u;
		}
		uint num2 = (uint)num;
		int num3 = num & 3;
		int num4 = num >> 2;
		int num5 = 0;
		ushort[] uInts = new BytetoUInt16Converter
		{
			Bytes = dataToHash
		}.UInts;
		while (num4 > 0)
		{
			num2 += uInts[num5++];
			uint num6 = (uint)(uInts[num5++] << 11) ^ num2;
			num2 = (num2 << 16) ^ num6;
			num2 += num2 >> 11;
			num4--;
		}
		num5 *= 2;
		switch (num3)
		{
		case 3:
			num2 += (ushort)(dataToHash[num5++] | (dataToHash[num5++] << 8));
			num2 ^= num2 << 16;
			num2 ^= (uint)(dataToHash[num5] << 18);
			num2 += num2 >> 11;
			break;
		case 2:
			num2 += (ushort)(dataToHash[num5++] | (dataToHash[num5] << 8));
			num2 ^= num2 << 11;
			num2 += num2 >> 17;
			break;
		case 1:
			num2 += dataToHash[num5];
			num2 ^= num2 << 10;
			num2 += num2 >> 1;
			break;
		}
		num2 ^= num2 << 3;
		num2 += num2 >> 5;
		num2 ^= num2 << 4;
		num2 += num2 >> 17;
		num2 ^= num2 << 25;
		return num2 + (num2 >> 6);
	}
}
