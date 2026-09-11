using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using NextAiVPN.Common;

namespace NextAiVPN.Services.Persistence;

internal class AppSettingsXmlFileGenerator : IAppSettingsXmlFileGenerator
{
	private readonly string _filePath = VPNConstants.FilePath.AppSettingsFilePath;

	private readonly IAppSettingsHelper _appSettingsHelper;

	public AppSettingsXmlFileGenerator(IAppSettingsHelper appSettingsHelper)
	{
		_appSettingsHelper = appSettingsHelper;
	}

	public void EnsureIfFileIsExist()
	{
		if (!IsExist())
		{
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SystemInfo.AppName);
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			GenerateFile();
		}
		else
		{
			if (!CheckSize())
			{
				GenerateFile();
			}
			SetVariablesValues();
		}
	}

	private void SetVariablesValues()
	{
		if (!XDocument.Load(_filePath).Descendants("AccountType").Any() && !string.IsNullOrEmpty(_appSettingsHelper.GetValue("id_token")))
		{
			_appSettingsHelper.SetValue("AccountType", "NextAiTechnology");
		}
		if (string.IsNullOrEmpty(_appSettingsHelper.GetValue("StreamingProtocol")))
		{
			_appSettingsHelper.SetValue("StreamingProtocol", "IKEv2");
		}
		if (string.IsNullOrEmpty(_appSettingsHelper.GetValue("IsStreamingSplitTunnelingEnabled")))
		{
			_appSettingsHelper.SetValue("IsStreamingSplitTunnelingEnabled", "0");
		}
	}

	private bool IsExist()
	{
		return File.Exists(_filePath);
	}

	private bool CheckSize()
	{
		return new FileInfo(_filePath).Length > 5;
	}

	private void GenerateFile()
	{
		using XmlWriter xmlWriter = XmlWriter.Create(_filePath);
		xmlWriter.WriteStartElement("Settings");
		xmlWriter.WriteElementString(AppSettingsKeys.ReleaseStage, SystemInfo.ReleaseStage);
		xmlWriter.WriteElementString("nickname", string.Empty);
		xmlWriter.WriteElementString("access_token", string.Empty);
		xmlWriter.WriteElementString("id_token", string.Empty);
		xmlWriter.WriteElementString("expires_in", string.Empty);
		xmlWriter.WriteElementString("token_type", string.Empty);
		xmlWriter.WriteElementString("refresh_token", string.Empty);
		xmlWriter.WriteElementString("expires_at", string.Empty);
		xmlWriter.WriteElementString("nextaivpn_id", string.Empty);
		xmlWriter.WriteElementString("subscription_type", string.Empty);
		xmlWriter.WriteElementString("protocol", "WireGuard");
		xmlWriter.WriteElementString("protocolType", string.Empty);
		xmlWriter.WriteElementString("scramble", "0");
		xmlWriter.WriteElementString("ConnectedTo", string.Empty);
		xmlWriter.WriteElementString("Startup", "1");
		xmlWriter.WriteElementString("AutoConnect", "0");
		xmlWriter.WriteElementString("KillSwitch", "0");
		xmlWriter.WriteElementString("BlockLAN", "0");
		xmlWriter.WriteElementString("Favorites", string.Empty);
		xmlWriter.WriteElementString("Preferences", "protocol,protocolType,scramble,protocolPanel,ConnectedTo,Startup,AutoConnect,KillSwitch,BlockLAN");
		xmlWriter.WriteElementString("LastVersionCheck", string.Empty);
		xmlWriter.WriteElementString("LastFeedbackSent", string.Empty);
		xmlWriter.WriteElementString("LastFeedbackClosed", string.Empty);
		xmlWriter.WriteElementString("MaxDays", "45");
		xmlWriter.WriteElementString("MinDays", "14");
		xmlWriter.WriteElementString("IUnderstand", "1");
		xmlWriter.WriteElementString("FavoritesList", string.Empty);
		xmlWriter.WriteElementString("LastConnected", "bestavailable");
		xmlWriter.WriteElementString("IsBestAvailable", "0");
		xmlWriter.WriteElementString("IsPurchaseStarted", "0");
		xmlWriter.WriteElementString("LocalEnabled", "0");
		xmlWriter.WriteElementString("IsFirstRun", "0");
		xmlWriter.WriteElementString("ConnectionCounter", "0");
		xmlWriter.WriteElementString("ConnectionsToShowFeedbackCounter", "0");
		xmlWriter.WriteElementString("ConnectionsAfterSkipCounter", "0");
		xmlWriter.WriteElementString("SendFeedbackCounter", "0");
		xmlWriter.WriteElementString("FirstRunDate", "0");
		xmlWriter.WriteElementString("IsFeedbackSendExample1", "0");
		xmlWriter.WriteElementString("IsFeedbackSendExample2", "0");
		xmlWriter.WriteElementString("IsFeedbackSendExample3", "0");
		xmlWriter.WriteElementString("IsFeedbackDontSendAndClosedManually", "0");
		xmlWriter.WriteElementString("Email", string.Empty);
		xmlWriter.WriteElementString("ThemeAppearance", string.Empty);
		xmlWriter.WriteElementString("RunAsAdmin", "0");
		xmlWriter.WriteElementString("OpenTab", "1");
		xmlWriter.WriteElementString("LastNotificationId", "0");
		xmlWriter.WriteElementString("LastLoadedLocations", string.Empty);
		xmlWriter.WriteElementString("sort", string.Empty);
		xmlWriter.WriteElementString("LastConnectedId", string.Empty);
		xmlWriter.WriteElementString("IsNeedToShowFeedbackPopUp", "bestavailable");
		xmlWriter.WriteElementString("IsNeedSendAutoProtectNotification", string.Empty);
		xmlWriter.WriteElementString("NotificationsFetched", string.Empty);
		xmlWriter.WriteElementString("NewNotificationsCount", string.Empty);
		xmlWriter.WriteElementString(AppSettingsKeys.SplitTunnelingAppList, string.Empty);
		xmlWriter.WriteElementString(AppSettingsKeys.SplitTunnelingHostnameIpsList, string.Empty);
		xmlWriter.WriteElementString("IsSplitTunnelingEnabled", "0");
		xmlWriter.WriteElementString("VpnType", "nextaivpn");
		xmlWriter.WriteElementString("StreamingProtocol", "IKEv2");
		xmlWriter.WriteElementString("CodeVerifier", string.Empty);
		xmlWriter.WriteElementString("IsLoggedIn", string.Empty);
		xmlWriter.WriteElementString("NextAiGlobalUserId", string.Empty);
		xmlWriter.WriteElementString("LastRefreshTokenDate", string.Empty);
		xmlWriter.WriteElementString("IsAdBlocker", "0");
		xmlWriter.WriteElementString("AccountType", "None");
		xmlWriter.WriteElementString("SubscriptionId", string.Empty);
		xmlWriter.WriteElementString(AppSettingsKeys.IsLanAllowed, "0");
		xmlWriter.WriteElementString("IsIPv6LeakProtection", "0");
		xmlWriter.WriteElementString("IsDnsLeakProtection", "0");
		xmlWriter.WriteElementString("IsDnsMonitoring", "0");
		xmlWriter.WriteElementString("IsTrafficOptimizer", "0");
		xmlWriter.WriteElementString("TrafficOptimizerAppList", string.Empty);
		xmlWriter.WriteElementString("IsStreamingDeviceLimitReached", "0");
		xmlWriter.WriteElementString("StreamingSplitTunnelingDomainList", string.Empty);
		xmlWriter.WriteElementString("IsStreamingSplitTunnelingEnabled", "0");
		xmlWriter.WriteElementString("IsTrialFirstWindowShowed", "0");
		xmlWriter.WriteElementString(AppSettingsKeys.VpnUsername, string.Empty);
		xmlWriter.WriteElementString(AppSettingsKeys.VpnPassword, string.Empty);
		xmlWriter.WriteEndElement();
		xmlWriter.Flush();
	}
}
