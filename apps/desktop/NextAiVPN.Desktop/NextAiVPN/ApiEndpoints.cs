namespace NextAiVPN;

internal static class ApiEndpoints
{
	public static string LoginPage
	{
		get
		{
			if (!IsRelease())
			{
				return "https://www.sandbox.nextaitechnology.com/auth/sso?returnUrl=%2fconnect%2fauthorize%2fcallback%3fresponse_type%3dcode%26client_id%3dapplication_5758%26redirect_uri%3dhttp%253A%252F%252Flocalhost%252Fsso%26scope%3dopenid%2520profile%2520offline_access%2520email%26state%3dxyz";
			}
			return "https://www.nextaitechnology.com/auth/sso?returnUrl=%2fconnect%2fauthorize%2fcallback%3fresponse_type%3dcode%26client_id%3d332de52499a24a168311aadd8c363201%26redirect_uri%3dhttp%253A%252F%252Flocalhost%252Fsso%26scope%3dopenid%2520profile%2520offline_access%2520email%26state%3dxyz";
		}
	}

	public static string CreateAccountPage
	{
		get
		{
			if (!IsRelease())
			{
				return "https://www.sandbox.nextaitechnology.com/auth/sso/signup?returnUrl=%2fconnect%2fauthorize%2fcallback%3fresponse_type%3dcode%26client_id%3dapplication_5758%26redirect_uri%3dhttp%3a%2f%2flocalhost%2fsso%26scope%3dopenid+profile+offline_access+email%26state%3dxyz";
			}
			return "https://www.nextaitechnology.com/auth/sso/signup?returnUrl=%2fconnect%2fauthorize%2fcallback%3fresponse_type%3dcode%26client_id%3d332de52499a24a168311aadd8c363201%26redirect_uri%3dhttp%3a%2f%2flocalhost%2fsso%26scope%3dopenid+profile+offline_access+email%26state%3dxyz";
		}
	}

	public static string ExchangeEndPoint
	{
		get
		{
			if (!IsRelease())
			{
				return "https://sb.vpn.ncapi.io/api/v1/auth/exchange_code";
			}
			return "https://vpn.ncapi.io/api/v1/auth/exchange_code";
		}
	}

	public static string CheckSubscriptionEndPoint
	{
		get
		{
			if (!IsRelease())
			{
				return "https://sb.vpn.ncapi.io/api/v3/auth/check";
			}
			return "https://vpn.ncapi.io/api/v3/auth/check";
		}
	}

	public static string CheckVersionNumber
	{
		get
		{
			if (!IsRelease())
			{
				return "https://sb.vpn.ncapi.io/appversion";
			}
			return "https://vpn.ncapi.io/appversion";
		}
	}

	public static string FeedbackEndPoint
	{
		get
		{
			if (!IsRelease())
			{
				return "https://sb.vpn.ncapi.io/api/feedback";
			}
			return "https://vpn.ncapi.io/api/feedback";
		}
	}

	public static string UploadFeedbackLogsFilesEndPoint
	{
		get
		{
			if (!IsRelease())
			{
				return "https://sb.vpn.ncapi.io/api/v1/uploadlogs";
			}
			return "https://vpn.ncapi.io/api/v1/uploadlogs";
		}
	}

	public static string BaseEndPoint
	{
		get
		{
			if (!IsRelease())
			{
				return "https://sb.vpn.ncapi.io";
			}
			return "https://vpn.ncapi.io";
		}
	}

	public static string NextAiGlobalLoginEndPoint
	{
		get
		{
			if (!IsRelease())
			{
				return "https://sb.vpn.ncapi.io/api/v1/nextaiglobal/login";
			}
			return "https://vpn.ncapi.io/api/v1/nextaiglobal/login";
		}
	}

	private static bool IsRelease()
	{
		return true;
	}
}
