namespace HashTableHashing;

internal static class SuperFastHashInlineBitConverter
{
	public static uint Hash(string dataToHash)
	{
		int length = dataToHash.Length;
		if (length == 0)
		{
			return 0u;
		}
		uint num = (uint)length;
		int num2 = length & 3;
		int num3 = length >> 2;
		int index = 0;
		while (num3 > 0)
		{
			num += (ushort)(dataToHash[index++] | ((uint)dataToHash[index++] << 8));
			uint num4 = ((dataToHash[index++] | ((uint)dataToHash[index++] << 8)) << 11) ^ num;
			num = (num << 16) ^ num4;
			num += num >> 11;
			num3--;
		}
		switch (num2)
		{
		case 3:
			num += (ushort)(dataToHash[index++] | ((uint)dataToHash[index++] << 8));
			num ^= num << 16;
			num ^= (uint)dataToHash[index] << 18;
			num += num >> 11;
			break;
		case 2:
			num += (ushort)(dataToHash[index++] | ((uint)dataToHash[index] << 8));
			num ^= num << 11;
			num += num >> 17;
			break;
		case 1:
			num += dataToHash[index];
			num ^= num << 10;
			num += num >> 1;
			break;
		}
		num ^= num << 3;
		num += num >> 5;
		num ^= num << 4;
		num += num >> 17;
		num ^= num << 25;
		return num + (num >> 6);
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
		while (num4 > 0)
		{
			num2 += (ushort)(dataToHash[num5++] | (dataToHash[num5++] << 8));
			uint num6 = (uint)((dataToHash[num5++] | (dataToHash[num5++] << 8)) << 11) ^ num2;
			num2 = (num2 << 16) ^ num6;
			num2 += num2 >> 11;
			num4--;
		}
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
