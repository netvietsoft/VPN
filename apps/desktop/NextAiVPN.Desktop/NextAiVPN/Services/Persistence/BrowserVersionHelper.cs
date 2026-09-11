using System;
using System.Globalization;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using NextAiVPN.Common;

namespace NextAiVPN.Services.Persistence;

public class BrowserVersionHelper : IBrowserVersionHelper
{
	private readonly IBugsnagService _bugsnagService;

	private readonly IAppLogger _logger;

	public BrowserVersionHelper(IBugsnagService bugsnagService, IAppLogger logger)
	{
		_bugsnagService = bugsnagService;
		_logger = logger;
	}

	public int GetEdgeVersion()
	{
		try
		{
			object value = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Edge\\BLBeacon").GetValue("version");
			if (value == null)
			{
				_logger?.Error("BrowserVersionHelper - Edge version not found", "GetEdgeVersion", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\BrowserVersionHelper.cs", 42);
				return 0;
			}
			int.TryParse(value.ToString().Substring(0, value.ToString().IndexOf(".")), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result);
			return result;
		}
		catch (Exception ex)
		{
			_logger?.Error("Unable to get Edge version: " + ex.Message, "GetEdgeVersion", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\BrowserVersionHelper.cs", 51);
			return 0;
		}
	}

	public int GetEdgeWebView2Version()
	{
		try
		{
			string availableBrowserVersionString = CoreWebView2Environment.GetAvailableBrowserVersionString();
			if (availableBrowserVersionString == null)
			{
				_logger?.Error("Edge WebView2 Runtime version not found", "GetEdgeWebView2Version", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\BrowserVersionHelper.cs", 68);
				return 0;
			}
			int.TryParse(availableBrowserVersionString.ToString().Substring(0, availableBrowserVersionString.ToString().IndexOf(".")), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result);
			_logger?.Information("Edge WebView2 Runtime version " + availableBrowserVersionString, "GetEdgeWebView2Version", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\BrowserVersionHelper.cs", 73);
			return result;
		}
		catch (WebView2RuntimeNotFoundException ex)
		{
			_logger?.Error("Unable to get Edge WebView2 Runtime version: " + ex.Message, "GetEdgeWebView2Version", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\BrowserVersionHelper.cs", 78);
			return 0;
		}
	}

	public int GetIeVersion()
	{
		string text = string.Empty;
		int num = 0;
		try
		{
			object value = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Internet Explorer").GetValue("svcUpdateVersion");
			if (value == null)
			{
				value = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Internet Explorer").GetValue("Version");
			}
			if (value != null)
			{
				text = value.ToString().Substring(0, value.ToString().IndexOf("."));
			}
			switch (text)
			{
			case "7":
				num = 7000;
				break;
			case "8":
				num = 8888;
				break;
			case "9":
				num = 9999;
				break;
			case "10":
				num = 10001;
				break;
			default:
				if (!string.IsNullOrEmpty(text) && Convert.ToInt32(text) >= 11)
				{
					num = 11001;
				}
				break;
			}
			if (num == 0)
			{
				_logger?.Error("IE version not found", "GetIeVersion", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\BrowserVersionHelper.cs", 120);
			}
			else
			{
				Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", SystemInfo.AppName + ".exe", num);
			}
		}
		catch (Exception ex)
		{
			_bugsnagService.Notify($"[setIEVersion] {ex.Message} {num}");
			_logger?.Error($"Unable to get IE version: {ex.Message} {num}", "GetIeVersion", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\BrowserVersionHelper.cs", 132);
		}
		_logger?.Information($"IE version {num}", "GetIeVersion", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\BrowserVersionHelper.cs", 135);
		return num;
	}
}
