using System;
using NextAiVPN.Common;

namespace NextAiVPN.Services.Persistence;

internal class DpapiSecretProtector : ISecretProtector
{
	private const string ProtectedValuePrefix = "dpapi:";

	public string Protect(string plainText)
	{
		if (string.IsNullOrEmpty(plainText))
		{
			return plainText;
		}
		return "dpapi:" + Dpapi.EncryptToBase64(plainText);
	}

	public string Unprotect(string storedValue)
	{
		if (string.IsNullOrEmpty(storedValue) || !storedValue.StartsWith("dpapi:", StringComparison.Ordinal))
		{
			return storedValue;
		}
		try
		{
			return Dpapi.DecryptFromBase64(storedValue.Substring("dpapi:".Length));
		}
		catch (Exception)
		{
			return string.Empty;
		}
	}
}
