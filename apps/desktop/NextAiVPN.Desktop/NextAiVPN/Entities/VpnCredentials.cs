namespace NextAiVPN.Entities;

public class VpnCredentials
{
	public string VpnUsername { get; }

	public string VpnPassword { get; }

	public VpnCredentials(string credUsername, string credPassword)
	{
		VpnUsername = credUsername;
		VpnPassword = credPassword;
	}
}
