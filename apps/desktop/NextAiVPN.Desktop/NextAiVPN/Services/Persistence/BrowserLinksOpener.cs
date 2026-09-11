using System;
using System.Diagnostics;
using System.Globalization;
using NextAiVPN.Common;
using NextAiVPN.Enums;

namespace NextAiVPN.Services.Persistence;

internal class BrowserLinksOpener : IBrowserLinksOpener
{
	private const string SpSAiAssistantLink = "https://www.nextaiglobal.com/application/ai-assistant/";

	private const string SpSTos = "https://www.nextaiglobal.com/legal/nextaivpn-terms-of-service-nextaiglobal-legal/";

	private const string SpSPrivatePolicy = "https://www.nextaiglobal.com/legal/nextaivpn-privacy-policy-nextaiglobal-legal/";

	private const string SpSLicenses = "https://www.nextaiglobal.com/legal/nextaivpn-terms-of-service-nextaiglobal-legal/#licences";

	private const string SpSPurchaseSubscription = "https://www.nextaiglobal.com/vpn";

	private const string SpSContactUs = "https://www.nextaiglobal.com/about/contact-us/";

	private const string NcTos = "https://www.nextaitechnology.com/legal/apps/vpn-tos/";

	private const string NcPrivatePolicy = "https://www.nextaitechnology.com/legal/apps/vpn-privacy-policy";

	private const string NcCustomerSupport = "https://www.nextaitechnology.com/support/";

	private const string NcFaQ = "https://www.nextaitechnology.com/support/knowledgebase/category/2265/nextaivpn/";

	private const string NcStreamingRules = "https://www.nextaitechnology.com/support/knowledgebase/article.aspx/10668/2274/how-to-use-nextaivpn-streaming-mode/#stream_rules";

	private const string NcHelpCenter = "https://www.nextaitechnology.com/help-center/";

	private const string NcPurchaseSubscriptionSb = "https://www.sandbox.nextaitechnology.com/vpn/";

	private const string NcPurchaseSubscription = "https://www.nextaitechnology.com/vpn/";

	private readonly IAccountTypeHelper _accountTypeHelper;

	private readonly IBugsnagService _bugsnagService;

	private readonly IAppLogger _logger;

	private readonly IAnalyticsService _analyticsService;

	private const string NotificationMessage = "Open browser link - {0}";

	public BrowserLinksOpener(IAccountTypeHelper accountTypeHelper, IBugsnagService bugsnagService, IAppLogger logger, IAnalyticsService analyticsService)
	{
		_accountTypeHelper = accountTypeHelper;
		_bugsnagService = bugsnagService;
		_logger = logger;
		_analyticsService = analyticsService;
	}

	public void OpenBrowserLink(int link)
	{
		try
		{
			string link2 = GetLink(link);
			ProcessExecute(link2);
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "OpenBrowserLink", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\BrowserLinksOpener.cs", 68);
			_bugsnagService.Notify(exception);
		}
	}

	private void ProcessExecute(string linkStr)
	{
		LogInfo(linkStr);
		Process.Start(new ProcessStartInfo
		{
			FileName = linkStr,
			UseShellExecute = true
		});
	}

	private void LogInfo(string linkStr)
	{
		_logger?.Information(string.Format(CultureInfo.InvariantCulture, "Open browser link - {0}", linkStr), "LogInfo", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\BrowserLinksOpener.cs", 85);
		_analyticsService.SendNotification(string.Format(CultureInfo.InvariantCulture, "Open browser link - {0}", linkStr));
	}

	private string GetLink(int link)
	{
		return _accountTypeHelper.GetAccountType() switch
		{
			AccountType.NextAiTechnology => GetNextAiTechnologyLink(link), 
			AccountType.NextAiGlobal => GetNextAiGlobalLink(link), 
			_ => GetNextAiTechnologyLink(link), 
		};
	}

	private string GetNextAiGlobalLink(int link)
	{
		return link switch
		{
			1 => "https://www.nextaiglobal.com/legal/nextaivpn-terms-of-service-nextaiglobal-legal/", 
			2 => "https://www.nextaiglobal.com/legal/nextaivpn-privacy-policy-nextaiglobal-legal/", 
			3 => "https://www.nextaiglobal.com/application/ai-assistant/", 
			4 => "https://www.nextaiglobal.com/application/ai-assistant/", 
			5 => "https://www.nextaiglobal.com/legal/nextaivpn-terms-of-service-nextaiglobal-legal/#licences", 
			6 => "https://www.nextaiglobal.com/application/ai-assistant/", 
			7 => "https://www.nextaiglobal.com/vpn", 
			8 => "https://www.nextaiglobal.com/about/contact-us/", 
			_ => throw new ArgumentOutOfRangeException("BrowserLinksOpener"), 
		};
	}

	private static string GetNextAiTechnologyLink(int link)
	{
		return link switch
		{
			1 => "https://www.nextaitechnology.com/legal/apps/vpn-tos/", 
			2 => "https://www.nextaitechnology.com/legal/apps/vpn-privacy-policy", 
			3 => "https://www.nextaitechnology.com/support/", 
			4 => "https://www.nextaitechnology.com/support/knowledgebase/category/2265/nextaivpn/", 
			5 => "https://www.nextaitechnology.com/legal/apps/vpn-tos/", 
			6 => "https://www.nextaitechnology.com/support/knowledgebase/article.aspx/10668/2274/how-to-use-nextaivpn-streaming-mode/#stream_rules", 
			7 => "https://www.nextaitechnology.com/vpn/", 
			8 => "https://www.nextaitechnology.com/help-center/", 
			_ => throw new ArgumentOutOfRangeException("BrowserLinksOpener"), 
		};
	}

	public void OpenBrowserLink(string link)
	{
		try
		{
			ProcessExecute(link);
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "OpenBrowserLink", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\BrowserLinksOpener.cs", 168);
			_bugsnagService.Notify(exception);
		}
	}
}
