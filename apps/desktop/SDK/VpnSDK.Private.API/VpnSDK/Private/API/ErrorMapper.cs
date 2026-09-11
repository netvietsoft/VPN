using System.Collections.Generic;

namespace VpnSDK.Private.API;

internal static class ErrorMapper
{
	private static readonly Dictionary<int, ApiError> StrongVpnErrorMappings = new Dictionary<int, ApiError>
	{
		{
			1000,
			ApiError.InvalidRequest
		},
		{
			1001,
			ApiError.InvalidApiKey
		},
		{
			1099,
			ApiError.TooManyInvalidRequests
		},
		{
			1100,
			ApiError.InvalidCredentials
		},
		{
			1105,
			ApiError.AccessTokenExpired
		},
		{
			1009,
			ApiError.InternalServerError
		}
	};

	private static readonly Dictionary<int, ApiError> DefaultErrorMappings = new Dictionary<int, ApiError>
	{
		{
			1000,
			ApiError.InvalidApiKey
		},
		{
			1001,
			ApiError.InvalidRequest
		},
		{
			1002,
			ApiError.InvalidEntryExitServers
		},
		{
			1004,
			ApiError.InvalidEntryExitServers
		},
		{
			1003,
			ApiError.MissingEntryExitServers
		},
		{
			1099,
			ApiError.TooManyInvalidRequests
		},
		{
			1100,
			ApiError.InvalidCredentials
		},
		{
			1101,
			ApiError.RefreshTokenExpired
		},
		{
			1103,
			ApiError.InvalidMetadata
		},
		{
			1102,
			ApiError.AccessTokenExpired
		},
		{
			1105,
			ApiError.AccessTokenExpired
		},
		{
			1201,
			ApiError.InactiveAccount
		},
		{
			1202,
			ApiError.InactiveAccount
		},
		{
			2001,
			ApiError.ServerUnhealthy
		},
		{
			2002,
			ApiError.ServerNotFound
		},
		{
			2003,
			ApiError.FeatureNotAvailable
		},
		{
			2004,
			ApiError.DoublehopNotAvailable
		}
	};

	internal static ApiError Map(int errorCode, ApiType apiType)
	{
		if (apiType == ApiType.StrongVPN && StrongVpnErrorMappings.TryGetValue(errorCode, out var value))
		{
			return value;
		}
		if (DefaultErrorMappings.TryGetValue(errorCode, out var value2))
		{
			return value2;
		}
		return ApiError.Unknown;
	}
}
