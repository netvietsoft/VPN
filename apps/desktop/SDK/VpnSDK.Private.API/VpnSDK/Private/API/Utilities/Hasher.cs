using System;
using System.Security.Cryptography;
using System.Text;

namespace VpnSDK.Private.API.Utilities;

internal static class Hasher
{
	internal static string SHA256HashString(string input, bool trimHash = false)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array;
		using (SHA256CryptoServiceProvider sHA256CryptoServiceProvider = new SHA256CryptoServiceProvider())
		{
			array = sHA256CryptoServiceProvider.ComputeHash(new UTF8Encoding().GetBytes(input));
		}
		for (int i = 0; i < (trimHash ? Math.Min(array.Length, 3) : array.Length); i++)
		{
			stringBuilder.Append(array[i].ToString("x2"));
		}
		return stringBuilder.ToString();
	}
}
