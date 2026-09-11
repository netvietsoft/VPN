using System;
using System.Collections.Generic;
using VpnSDK.Private.API;

namespace VpnSDK.Internal.Helpers;

internal static class VpnApiMapper
{
	private static readonly Dictionary<ApiError, Func<VpnApiException, HTTPException>> ErrorMappings = new Dictionary<ApiError, Func<VpnApiException, HTTPException>>
	{
		{
			ApiError.InvalidApiKey,
			(VpnApiException e) => new APIException("Invalid API key.", e)
		},
		{
			ApiError.InvalidRequest,
			(VpnApiException e) => new AuthenticationException("Invalid API key or bearer token provided.", e)
		},
		{
			ApiError.InvalidCredentials,
			(VpnApiException e) => new AuthenticationException("Invalid username or password.", e)
		},
		{
			ApiError.InactiveAccount,
			(VpnApiException e) => new InvalidAccountException("Inactive account.", e)
		},
		{
			ApiError.TooManyInvalidRequests,
			(VpnApiException e) => new APIException("API requests are currently being rate limited due to too many invalid requests.", e)
		},
		{
			ApiError.AccessTokenExpired,
			(VpnApiException _) => new OAuthException("Access token expired.")
		},
		{
			ApiError.RefreshTokenExpired,
			(VpnApiException _) => new OAuthException("Refresh token expired.")
		},
		{
			ApiError.InternalServerError,
			(VpnApiException e) => new APIException("API server failure.", e)
		},
		{
			ApiError.ServerNotFound,
			(VpnApiException _) => new InvalidServerException("Server not found.")
		},
		{
			ApiError.ServerUnhealthy,
			(VpnApiException _) => new ServerUnhealthyException("Server unhealthy.")
		},
		{
			ApiError.InvalidEntryExitServers,
			(VpnApiException _) => new InvalidDoubleHopConfigurationException("Invalid configuration for entry or exit locations.", ApiError.InvalidEntryExitServers)
		},
		{
			ApiError.MissingEntryExitServers,
			(VpnApiException _) => new InvalidDoubleHopConfigurationException("Missing required field - entry or exit location.", ApiError.MissingEntryExitServers)
		},
		{
			ApiError.DoublehopNotAvailable,
			(VpnApiException _) => new DoubleHopNotAvailableException("DoubleHop is not available.", ApiError.DoublehopNotAvailable)
		},
		{
			ApiError.AllApiEndpointsUnreachable,
			(VpnApiException _) => new EndpointsUnreachableException("All API endpoints are unreachable. Possible network restrictions or regional blocking detected.")
		}
	};

	public static HTTPException ToApiException(this VpnApiException e)
	{
		if (ErrorMappings.TryGetValue(e.Error, out var value))
		{
			return value(e);
		}
		return new APIException("An unknown error occurred.", e);
	}
}
