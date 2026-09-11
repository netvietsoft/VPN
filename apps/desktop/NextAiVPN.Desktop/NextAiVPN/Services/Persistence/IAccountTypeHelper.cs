using NextAiVPN.Enums;

namespace NextAiVPN.Services.Persistence;

public interface IAccountTypeHelper
{
	AccountType GetAccountType();

	void SetAccountType(AccountType type);
}
