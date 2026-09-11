using System;
using NextAiVPN.Common;
using NextAiVPN.Enums;

namespace NextAiVPN.Services.Persistence;

internal class AccountTypeHelper : IAccountTypeHelper
{
	private readonly IAppSettingsHelper _appSettingsHelper;

	public AccountTypeHelper(IAppSettingsHelper appSettingsHelper)
	{
		_appSettingsHelper = appSettingsHelper;
	}

	public AccountType GetAccountType()
	{
		string value = _appSettingsHelper.GetValue("AccountType");
		if (string.IsNullOrEmpty(value) || !value.Equals("0"))
		{
			if (!Enum.TryParse<AccountType>(value, ignoreCase: true, out var result))
			{
				return AccountType.None;
			}
			return result;
		}
		return AccountType.None;
	}

	public void SetAccountType(AccountType type)
	{
		_appSettingsHelper.SetValue("AccountType", type.ToString());
	}
}
