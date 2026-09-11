using System;

namespace NextAiVPN.Services;

public interface ISignInService
{
	void BrowserNavigating(object sender, EventArgs e);

	void SignInValidations();

	void CheckPrelogged();
}
