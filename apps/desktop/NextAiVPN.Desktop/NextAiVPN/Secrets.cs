using System;
using NextAiVPN.Common;

namespace NextAiVPN;

public static class Secrets
{
	private static readonly string _nextaivpnApiKey;

	private static readonly string _nextaivpnAuthToken;

	private static readonly string _mixpanelToken;

	public const string NextAiVpnApiKey = "933f67de383fb9987d8c11216bc94da1";

	public const string NextAiVpnAuthToken = "@nextaitechnology";

	public const string MixpanelToken = "006820e9b2f2792c5382cac96bf53bba";

	public const string BugsnagApiKey = "f09ed9125b51daee9c4f6acf778dffd7";

	static Secrets()
	{
		_nextaivpnApiKey = GetRequired("WIN_NextAiVPN_KEY");
		_nextaivpnAuthToken = GetRequired("WIN_NextAiVPN_AUTH_TOKEN");
		_mixpanelToken = GetRequired("WIN_MIXPANEL_TOKEN");
	}

	private static string GetRequired(string name)
	{
		return GetRequired(name, Environment.GetEnvironmentVariable, Utils.Logger);
	}

	internal static string GetRequired(string name, Func<string, string> getEnvVar, IAppLogger logger)
	{
		string text = getEnvVar(name);
		if (string.IsNullOrWhiteSpace(text))
		{
			logger?.Error("Environment variable '" + name + "' is required but not configured.", "GetRequired", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Secrets.cs", 38);
			return string.Empty;
		}
		return text;
	}
}
