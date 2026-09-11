using System;
using System.ComponentModel;
using System.Net;
using VpnSDK.Enums;

namespace VpnSDK.Interfaces;

public interface IUser : INotifyPropertyChanged
{
	string EmailAddress { get; }

	AccountStatus Status { get; }

	NetworkCredential VpnCredential { get; }

	DateTime? SubscriptionExpiry { get; }

	bool IsValid { get; }

	string AccessToken { get; }

	string RefreshToken { get; }

	DateTime? AccessTokenExpiry { get; }
}
