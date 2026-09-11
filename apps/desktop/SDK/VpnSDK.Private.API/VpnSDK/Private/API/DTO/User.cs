using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using VpnSDK.Private.API.Utilities;

namespace VpnSDK.Private.API.DTO;

[Serializable]
public class User : JsonResponseResult
{
	private const double TokenValidityThresholdPercentage = 20.0;

	private NetworkCredential _vpnCredential;

	private byte[] _accessTokenProtected;

	private byte[] _refreshTokenProtected;

	[JsonProperty("email")]
	public string Email { get; set; }

	[JsonProperty("account_type")]
	public short AccountType { get; set; }

	[JsonProperty("sub_end_epoch", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public DateTime? SubscriptionExpiry { get; set; }

	[JsonProperty("access_token")]
	public string AccessToken
	{
		get
		{
			string result = string.Empty;
			if (_accessTokenProtected != null)
			{
				byte[] bytes = ProtectedData.Unprotect(_accessTokenProtected, null, DataProtectionScope.CurrentUser);
				result = Encoding.UTF8.GetString(bytes);
			}
			return result;
		}
		set
		{
			byte[] bytes = Encoding.UTF8.GetBytes(value);
			_accessTokenProtected = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
		}
	}

	[JsonProperty("refresh_token")]
	public string RefreshToken
	{
		get
		{
			string result = string.Empty;
			if (_refreshTokenProtected != null)
			{
				byte[] bytes = ProtectedData.Unprotect(_refreshTokenProtected, null, DataProtectionScope.CurrentUser);
				result = Encoding.UTF8.GetString(bytes);
			}
			return result;
		}
		set
		{
			byte[] bytes = Encoding.UTF8.GetBytes(value);
			_refreshTokenProtected = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
		}
	}

	[JsonProperty("access_expire_epoch", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public DateTime? AccessTokenExpiry { get; set; }

	[JsonProperty("username")]
	public string Username { get; set; }

	[JsonProperty("password")]
	public string Password { get; set; }

	[JsonProperty("branding")]
	public BrandingInfo BrandingInfo { get; set; }

	[JsonProperty("auth", ObjectCreationHandling = ObjectCreationHandling.Replace)]
	[JsonConverter(typeof(NetworkCredentialConverter))]
	public NetworkCredential VpnCredential
	{
		get
		{
			return _vpnCredential ?? GenerateVpnCredential();
		}
		internal set
		{
			_vpnCredential = value;
		}
	}

	public string Brand { get; set; }

	public DateTime? TokenCreationTime { get; set; }

	public bool HasValidToken
	{
		get
		{
			if (!AccessTokenExpiry.HasValue || !TokenCreationTime.HasValue)
			{
				return false;
			}
			DateTime utcNow = DateTime.UtcNow;
			if (utcNow < TokenCreationTime.Value || utcNow >= AccessTokenExpiry.Value)
			{
				return false;
			}
			TimeSpan timeSpan = AccessTokenExpiry.Value - TokenCreationTime.Value;
			return (utcNow - TokenCreationTime.Value).TotalMilliseconds / timeSpan.TotalMilliseconds * 100.0 < 80.0;
		}
	}

	public static explicit operator NetworkCredential(User user)
	{
		return user.ToNetworkCredential();
	}

	public string GetUsernameWithBrand(string username, string brand)
	{
		return username + (string.IsNullOrEmpty(brand) ? "" : ("@" + brand));
	}

	private NetworkCredential GenerateVpnCredential()
	{
		return new NetworkCredential(GetUsernameWithBrand(Username, Brand), Password);
	}

	public NetworkCredential ToNetworkCredential()
	{
		return new NetworkCredential(Username, Password);
	}
}
