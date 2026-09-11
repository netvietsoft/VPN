namespace NextAiVPN.Services.Persistence;

internal interface ICustomUrlSchemeRegister
{
	void RegisterCustomUrlScheme(string schemeName, string applicationPath);
}
