using System;
using System.Threading.Tasks;
using NextAiVPN.Entities;

namespace NextAiVPN.Services;

public interface IAuthenticationFlow
{
	Task<bool> HasActiveSubscriptionAsync();

	VpnCredentials GetCredentials();

	Task<bool> EnsureKillSwitchPermissionAsync(Func<bool> isRunAsAdminAllowed);

	Task<LoginAttemptResult> LoginToNextAiVpnAsync(string userName, string password);

	Task<LoginAttemptResult> LoginToStreamingAsync(string userName, string password);
}
