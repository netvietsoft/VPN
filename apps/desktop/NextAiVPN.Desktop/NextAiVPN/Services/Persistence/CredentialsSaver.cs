using System;
using System.Globalization;
using System.IO;
using NextAiVPN.Common;

namespace NextAiVPN.Services.Persistence;

internal class CredentialsSaver : ICredentialsSaver
{
	private const string TrustedNetworkFileName = "\\TrustedNetworks.tnfvpn";

	private const string AppSettingsXmlFileName = "\\AppSettings.xml";

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IPreferencesRepository _preferencesRepository;

	private readonly string _userPreferencesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NextAiVPN\\Preferences");

	private readonly string _tempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NextAiVPN_temp\\Preferences");

	private static readonly string TempDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NextAiVPN_temp");

	private static readonly string NextAiVpnDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NextAiVPN");

	private readonly string _favoriteLocationPath = Path.Combine(TempDirPath, "favorite_loc.txt");

	public CredentialsSaver(IAppSettingsHelper appSettingsHelper, IPreferencesRepository preferencesRepository)
	{
		_appSettingsHelper = appSettingsHelper;
		_preferencesRepository = preferencesRepository;
	}

	public void Restore()
	{
		if (Directory.Exists(_tempPath))
		{
			RestoreAll();
		}
	}

	private void RestoreAll()
	{
		RestoreUserPreferences();
		RestoreTrustedNetwork();
		if (IsUsingNewCredentialMechanism())
		{
			RestoreAppSettingsXml();
			if (string.IsNullOrEmpty(_appSettingsHelper.GetValue("AccountType")))
			{
				_appSettingsHelper.SetValue("AccountType", "NextAiTechnology");
			}
		}
		else
		{
			if (File.Exists(_favoriteLocationPath))
			{
				string[] array = File.ReadAllLines(_favoriteLocationPath);
				_appSettingsHelper.SetValue("FavoritesList", array[0]);
				_appSettingsHelper.SetValue("LastConnectedId", array[1]);
			}
			else
			{
				_appSettingsHelper.SetValue("LastConnectedId", "bestavailable");
			}
			_appSettingsHelper.SetValue("IUnderstand", "1");
			_appSettingsHelper.SetValue("FirstRunDate", DateTime.Now.ToString("d", CultureInfo.InvariantCulture));
			_appSettingsHelper.SetValue("IsFirstRun", "0");
		}
		_preferencesRepository.SaveSinglePreference("iunderstand", "1");
		Directory.Delete(TempDirPath, recursive: true);
	}

	private static void RestoreUserPreferences()
	{
		string destinationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SystemInfo.AppName, "Preferences") + PreferencesRepository.GetDbPath();
		Copy(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SystemInfo.AppName + "_temp", "Preferences") + PreferencesRepository.GetDbPath(), destinationPath);
	}

	public void Save()
	{
		if (Directory.Exists(TempDirPath))
		{
			Directory.Delete(TempDirPath, recursive: true);
		}
		if (!Directory.Exists(_tempPath))
		{
			Directory.CreateDirectory(_tempPath);
			if (Directory.Exists(_tempPath))
			{
				SaveAll();
			}
		}
	}

	private void SaveAll()
	{
		SaveUserPreference();
		SaveTrustedNetwork();
		if (IsUsingNewCredentialMechanism())
		{
			SaveAppSettingsXml();
			return;
		}
		string value = _appSettingsHelper.GetValue("FavoritesList");
		string value2 = _appSettingsHelper.GetValue("LastConnectedId");
		using StreamWriter streamWriter = File.CreateText(_favoriteLocationPath);
		streamWriter.WriteLine(value);
		streamWriter.WriteLine(value2);
	}

	private void SaveAppSettingsXml()
	{
		string text = NextAiVpnDirPath + "\\AppSettings.xml";
		if (File.Exists(text))
		{
			string destinationPath = TempDirPath + "\\AppSettings.xml";
			Copy(text, destinationPath);
		}
	}

	private void RestoreAppSettingsXml()
	{
		string destinationPath = NextAiVpnDirPath + "\\AppSettings.xml";
		Copy(TempDirPath + "\\AppSettings.xml", destinationPath);
	}

	private static bool IsUsingNewCredentialMechanism()
	{
		return Version.Parse(SystemInfo.AppVersion) > Version.Parse("3.4.0.0");
	}

	private void SaveUserPreference()
	{
		string text = "\\UserPreferences.sqlite";
		string sourcePath = _userPreferencesPath + text;
		string destinationPath = _tempPath + text;
		Copy(sourcePath, destinationPath);
	}

	private void SaveTrustedNetwork()
	{
		string text = NextAiVpnDirPath + "\\TrustedNetworks.tnfvpn";
		if (File.Exists(text))
		{
			string destinationPath = TempDirPath + "\\TrustedNetworks.tnfvpn";
			Copy(text, destinationPath);
		}
	}

	private void RestoreTrustedNetwork()
	{
		string destinationPath = NextAiVpnDirPath + "\\TrustedNetworks.tnfvpn";
		Copy(TempDirPath + "\\TrustedNetworks.tnfvpn", destinationPath);
	}

	private static void Copy(string sourcePath, string destinationPath)
	{
		try
		{
			File.Copy(sourcePath, destinationPath, overwrite: true);
		}
		catch (Exception ex)
		{
			Console.Write(ex.Message);
		}
	}
}
