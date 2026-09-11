namespace NextAiVPN.Services.Persistence;

public interface ISecretProtector
{
	string Protect(string plainText);

	string Unprotect(string storedValue);
}
