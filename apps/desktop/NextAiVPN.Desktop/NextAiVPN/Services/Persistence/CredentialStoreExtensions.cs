using NextAiVPN.Common;
using NextAiVPN.Entities;
using NextAiVPN.Streaming.Exceptions;

namespace NextAiVPN.Services.Persistence;

public static class CredentialStoreExtensions
{
	public static VpnCredentials GetVpnCredentials(this ICredentialStore credentialStore)
	{
		string value = Utils.AppSettingsHelper.GetValue(AppSettingsKeys.VpnUsername);
		string value2 = Utils.AppSettingsHelper.GetValue(AppSettingsKeys.VpnPassword);
		if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(value2))
		{
			throw new CredentialStoreException();
		}
		VpnCredentials vpnCredentials = new VpnCredentials(value, value2);
		credentialStore.SaveCredentials(vpnCredentials);
		Utils.AppSettingsHelper.SetValue(AppSettingsKeys.VpnUsername, string.Empty);
		Utils.AppSettingsHelper.SetValue(AppSettingsKeys.VpnPassword, string.Empty);
		return vpnCredentials;
	}

	public static VpnCredentials GetVpnCredentialsFromCredManager(this ICredentialStore credentialStore)
	{
		CredentialStore credentialStore2 = new CredentialStore(Utils.Logger);
		VpnCredentials vpnCredentials = credentialStore2.GetCredentials();
		if (vpnCredentials == null || string.IsNullOrEmpty(vpnCredentials.VpnUsername) || string.IsNullOrEmpty(vpnCredentials.VpnPassword))
		{
			string value = Utils.AppSettingsHelper.GetValue(AppSettingsKeys.VpnUsername);
			string value2 = Utils.AppSettingsHelper.GetValue(AppSettingsKeys.VpnPassword);
			if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(value2))
			{
				throw new CredentialStoreException();
			}
			Utils.Logger.Information("LOADED OLD CRED FROM APP SETTINGS", "GetVpnCredentialsFromCredManager", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\CredentialStoreExtensions.cs", 59);
			vpnCredentials = new VpnCredentials(value, value2);
		}
		credentialStore.SaveCredentials(vpnCredentials);
		credentialStore2.DeleteCredentials();
		return vpnCredentials;
	}
}
