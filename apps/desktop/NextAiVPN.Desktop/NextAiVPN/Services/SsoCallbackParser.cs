using System;

namespace NextAiVPN.Services;

public static class SsoCallbackParser
{
	private const string CodeParameter = "code";

	public static string ExtractCode(string uri)
	{
		return ExtractQueryParameter(uri, "code");
	}

	public static string ExtractQueryParameter(string uri, string name)
	{
		if (string.IsNullOrEmpty(uri) || string.IsNullOrEmpty(name))
		{
			return null;
		}
		string text = name + "=";
		int startIndex = 0;
		int num;
		while (true)
		{
			num = uri.IndexOf(text, startIndex, StringComparison.Ordinal);
			if (num < 0)
			{
				return null;
			}
			if (IsParameterBoundary(uri, num))
			{
				break;
			}
			startIndex = num + text.Length;
		}
		int num2 = num + text.Length;
		int num3 = uri.IndexOf('&', num2);
		if (num3 >= 0)
		{
			return uri.Substring(num2, num3 - num2);
		}
		return uri.Substring(num2);
	}

	private static bool IsParameterBoundary(string uri, int tokenIndex)
	{
		if (tokenIndex == 0)
		{
			return true;
		}
		char c = uri[tokenIndex - 1];
		if (!char.IsLetterOrDigit(c))
		{
			return c != '_';
		}
		return false;
	}
}
