using System;
using System.Security.Cryptography;
using System.Text;

namespace NextAiVPN.Common;

public static class Dpapi
{
	private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("NextAiVPN.NextAiVPN.dpapi.v1");

	public static bool IsEncrypted(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return false;
		}
		try
		{
			Unprotect(Convert.FromBase64String(value));
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static string EncryptToBase64(string plaintext)
	{
		if (plaintext == null)
		{
			return null;
		}
		return Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(plaintext), Entropy, DataProtectionScope.CurrentUser));
	}

	public static string DecryptFromBase64(string base64)
	{
		if (base64 == null)
		{
			return null;
		}
		byte[] bytes = Unprotect(Convert.FromBase64String(base64));
		return Encoding.UTF8.GetString(bytes);
	}

	private static byte[] Unprotect(byte[] protectedData)
	{
		try
		{
			return ProtectedData.Unprotect(protectedData, Entropy, DataProtectionScope.CurrentUser);
		}
		catch (CryptographicException)
		{
			return ProtectedData.Unprotect(protectedData, null, DataProtectionScope.CurrentUser);
		}
	}
}
