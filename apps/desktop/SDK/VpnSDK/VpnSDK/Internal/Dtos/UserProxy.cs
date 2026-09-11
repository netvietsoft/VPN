using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using VpnSDK.Enums;
using VpnSDK.Interfaces;
using VpnSDK.Internal.Helpers;
using VpnSDK.Private.API.DTO;

namespace VpnSDK.Internal.Dtos;

internal class UserProxy : BindableBase, IUser, INotifyPropertyChanged
{
	public string EmailAddress => User?.Email;

	public AccountStatus Status
	{
		get
		{
			if (User == null)
			{
				return AccountStatus.Unknown;
			}
			if (!string.IsNullOrEmpty(User.Brand))
			{
				if (!Enum.IsDefined(typeof(AccountStatus), User.AccountType))
				{
					return AccountStatus.Unknown;
				}
				return (AccountStatus)User.AccountType;
			}
			switch (User.AccountType)
			{
			case 2:
				return AccountStatus.Paid;
			case 3:
				return AccountStatus.Expired;
			case 1:
			case 4:
				return AccountStatus.Trial;
			case 0:
				return AccountStatus.Free;
			default:
				return AccountStatus.Unknown;
			}
		}
	}

	public NetworkCredential VpnCredential => User?.VpnCredential;

	public DateTime? SubscriptionExpiry => User?.SubscriptionExpiry;

	public bool IsValid => User != null;

	public string RefreshToken => User?.RefreshToken;

	public string AccessToken => User?.AccessToken;

	public DateTime? AccessTokenExpiry => User?.AccessTokenExpiry;

	internal User User { get; private set; }

	public UserProxy(User user)
	{
		User = user;
	}

	internal void UpdateProxiedUser(User user)
	{
		List<string> list = new List<string>();
		if (User != null && user == null)
		{
			list.Add("Status");
			list.Add("IsValid");
		}
		if (User == null && user != null)
		{
			list.AddRange(new string[5] { "EmailAddress", "Status", "VpnCredential", "SubscriptionExpiry", "IsValid" });
		}
		if (User != null && user != null)
		{
			if (User.VpnCredential.UserName != user.VpnCredential.UserName || User.VpnCredential.Password != user.VpnCredential.Password)
			{
				list.Add("VpnCredential");
			}
			if (User.AccountType != user.AccountType)
			{
				list.Add("Status");
			}
			if (User.Email != user.Email)
			{
				list.Add("EmailAddress");
			}
			if (User.SubscriptionExpiry != user.SubscriptionExpiry)
			{
				list.Add("SubscriptionExpiry");
			}
			if (User.AccessToken != user.AccessToken)
			{
				list.Add("AccessToken");
			}
			if (User.RefreshToken != user.RefreshToken)
			{
				list.Add("RefreshToken");
			}
			if (User.AccessTokenExpiry != user.AccessTokenExpiry)
			{
				list.Add("AccessTokenExpiry");
			}
		}
		User = user;
		foreach (string item in list.Distinct())
		{
			OnPropertyChanged(item);
		}
	}
}
