using System;

namespace HashTableHashing;

internal static class SuperFastHashSimple
{
	public static uint Hash(byte[] dataToHash)
	{
		int num = dataToHash.Length;
		if (num == 0)
		{
			return 0u;
		}
		uint num2 = Convert.ToUInt32(num);
		int num3 = num & 3;
		int num4 = num >> 2;
		int num5 = 0;
		while (num4 > 0)
		{
			num2 += BitConverter.ToUInt16(dataToHash, num5);
			uint num6 = (uint)(BitConverter.ToUInt16(dataToHash, num5 + 2) << 11) ^ num2;
			num2 = (num2 << 16) ^ num6;
			num2 += num2 >> 11;
			num5 += 4;
			num4--;
		}
		switch (num3)
		{
		case 3:
			num2 += BitConverter.ToUInt16(dataToHash, num5);
			num2 ^= num2 << 16;
			num2 ^= (uint)(dataToHash[num5 + 2] << 18);
			num2 += num2 >> 11;
			break;
		case 2:
			num2 += BitConverter.ToUInt16(dataToHash, num5);
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
