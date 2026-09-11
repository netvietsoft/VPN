using System;
using NextAiVPN.Enums;

namespace NextAiVPN.Entities;

public class LoginAttemptResult
{
	public LoginAttemptStatus Status { get; }

	public Exception Exception { get; }

	public string ErrorMessage => Exception?.Message;

	private LoginAttemptResult(LoginAttemptStatus status, Exception exception)
	{
		Status = status;
		Exception = exception;
	}

	public static LoginAttemptResult Success()
	{
		return new LoginAttemptResult(LoginAttemptStatus.Success, null);
	}

	public static LoginAttemptResult Skipped()
	{
		return new LoginAttemptResult(LoginAttemptStatus.Skipped, null);
	}

	public static LoginAttemptResult ModeUnavailable(Exception exception)
	{
		return new LoginAttemptResult(LoginAttemptStatus.ModeUnavailable, exception);
	}

	public static LoginAttemptResult NoNetwork(Exception exception)
	{
		return new LoginAttemptResult(LoginAttemptStatus.NoNetwork, exception);
	}

	public static LoginAttemptResult Failed(Exception exception)
	{
		return new LoginAttemptResult(LoginAttemptStatus.Failed, exception);
	}
}
