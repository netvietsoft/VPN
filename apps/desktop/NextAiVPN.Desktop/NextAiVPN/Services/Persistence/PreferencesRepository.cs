using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using NextAiVPN.Common;

namespace NextAiVPN.Services.Persistence;

internal class PreferencesRepository : IPreferencesRepository
{
	private static readonly HashSet<string> PreferenceColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		"nickname",
		"accesstoken",
		"idtoken",
		"expiresin",
		"tokentype",
		"refreshtoken",
		"expiresat",
		"nextaivpnid",
		"vpnusername",
		"vpnpassword",
		"subscriptiontype",
		"protocol",
		"protocoltype",
		"scramble",
		"connectedto",
		"lastconnected",
		"isbestavailable",
		"startup",
		"autoconnect",
		"killswitch",
		"blocklan",
		"lastfeedbacksent",
		"lastversioncheck",
		"lastfeedbackclosed",
		"iunderstand",
		"favoriteslist",
		"localenabled",
		"isactive",
		AppSettingsKeys.SplitTunnelingAppList,
		AppSettingsKeys.SplitTunnelingHostnameIpsList
	};

	private static readonly HashSet<string> SensitiveColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "accesstoken", "idtoken", "refreshtoken" };

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	private readonly ISecretProtector _secretProtector;

	private readonly string _dbFolderPath;

	private string ConnectionString => "Data Source=" + _dbFolderPath + GetDbPath() + ";Version=3;";

	public PreferencesRepository(IAppSettingsHelper appSettingsHelper, IAppLogger logger, ISecretProtector secretProtector)
		: this(appSettingsHelper, logger, secretProtector, GetDbFolderPath())
	{
	}

	internal PreferencesRepository(IAppSettingsHelper appSettingsHelper, IAppLogger logger, ISecretProtector secretProtector, string dbFolderPath)
	{
		_appSettingsHelper = appSettingsHelper ?? throw new ArgumentNullException("appSettingsHelper");
		_logger = logger;
		_secretProtector = secretProtector ?? throw new ArgumentNullException("secretProtector");
		_dbFolderPath = dbFolderPath;
	}

	public static string GetDbFolderPath()
	{
		return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SystemInfo.AppName, "Preferences");
	}

	public static string GetDbPath()
	{
		return "\\UserPreferences.sqlite";
	}

	private string GetConfigNickname()
	{
		return _appSettingsHelper.GetValue("nickname");
	}

	public async Task InitializePreferencesDbIfNeeded()
	{
		_ = 4;
		try
		{
			if (!Directory.Exists(_dbFolderPath))
			{
				await CreatePreferencesFolder(_dbFolderPath);
			}
			await RestoreCredentials();
			string path = _dbFolderPath + GetDbPath();
			if (!File.Exists(path))
			{
				await CreateDbFile(path);
			}
			await EnsurePreferencesTableExistsAsync();
			await EnsureLogDbIsExist();
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "InitializePreferencesDbIfNeeded", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\PreferencesRepository.cs", 118);
		}
	}

	public Task EnsurePreferencesTableExistsAsync()
	{
		return Task.Run(delegate
		{
			using SQLiteConnection sQLiteConnection = new SQLiteConnection(ConnectionString);
			sQLiteConnection.Open();
			using SQLiteCommand sQLiteCommand = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='Preferences';", sQLiteConnection);
			if (sQLiteCommand.ExecuteScalar() == null)
			{
				using (SQLiteCommand sQLiteCommand2 = new SQLiteCommand($"\r\nCREATE TABLE Preferences (\r\n nickname VARCHAR(50),\r\n accesstoken TEXT,\r\n idtoken TEXT,\r\n expiresin VARCHAR(25),\r\n tokentype VARCHAR(25),\r\n refreshtoken TEXT,\r\n expiresat VARCHAR(50),\r\n nextaivpnid VARCHAR(25),\r\n vpnusername TEXT,\r\n vpnpassword TEXT,\r\n subscriptiontype VARCHAR(10),\r\n protocol VARCHAR(15),\r\n protocoltype VARCHAR(15),\r\n scramble VARCHAR(5),\r\n connectedto VARCHAR(50),\r\n lastconnected VARCHAR(50),\r\n isbestavailable VARCHAR(5),\r\n startup VARCHAR(5),\r\n autoconnect VARCHAR(5),\r\n killswitch VARCHAR(5),\r\n blocklan VARCHAR(5),\r\n lastfeedbacksent VARCHAR(50),\r\n lastversioncheck VARCHAR(50),\r\n lastfeedbackclosed VARCHAR(50),\r\n iunderstand VARCHAR(5),\r\n favoriteslist TEXT,\r\n localenabled VARCHAR(5),\r\n isactive VARCHAR(5),\r\n{AppSettingsKeys.SplitTunnelingAppList} TEXT,\r\n{AppSettingsKeys.SplitTunnelingHostnameIpsList} TEXT\r\n);", sQLiteConnection))
				{
					sQLiteCommand2.ExecuteNonQuery();
				}
				_logger?.Information("Table 'Preferences' created successfully.", "EnsurePreferencesTableExistsAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\PreferencesRepository.cs", 179);
			}
		});
	}

	public async Task EnsureLogDbIsExist()
	{
		try
		{
			await Task.Run(delegate
			{
				if (!File.Exists(VPNConstants.FilePath.AppLogsDbFilePath))
				{
					File.Copy("AppLogsDB.db", VPNConstants.FilePath.AppLogsDbFilePath);
				}
			});
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "EnsureLogDbIsExist", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\PreferencesRepository.cs", 201);
		}
	}

	public async Task RestoreCredentials()
	{
		await Task.Run(delegate
		{
			if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NextAiVPN_temp", "Preferences") + GetDbPath()))
			{
				new CredentialsSaver(_appSettingsHelper, this).Restore();
			}
		});
	}

	private static async Task CreatePreferencesFolder(string path)
	{
		await Task.Run(delegate
		{
			Directory.CreateDirectory(path);
		});
	}

	private static async Task CreateDbFile(string path)
	{
		await Task.Run(delegate
		{
			SQLiteConnection.CreateFile(path);
		});
	}

	public async Task<bool> UserExists()
	{
		bool exists = false;
		try
		{
			await Task.Run(delegate
			{
				using SQLiteConnection sQLiteConnection = new SQLiteConnection(ConnectionString);
				sQLiteConnection.Open();
				using (SQLiteCommand sQLiteCommand = new SQLiteCommand("SELECT nickname FROM Preferences WHERE nickname = @nickname;", sQLiteConnection))
				{
					sQLiteCommand.Parameters.AddWithValue("@nickname", GetConfigNickname());
					using SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader();
					if (sQLiteDataReader.Read())
					{
						exists = true;
					}
					sQLiteDataReader.Close();
				}
				sQLiteConnection.Close();
			});
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "UserExists", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\PreferencesRepository.cs", 267);
		}
		return exists;
	}

	public async Task<string> HasActiveUser()
	{
		string user = "";
		try
		{
			await Task.Run(delegate
			{
				using SQLiteConnection sQLiteConnection = new SQLiteConnection(ConnectionString);
				sQLiteConnection.Open();
				using (SQLiteCommand sQLiteCommand = new SQLiteCommand("SELECT nickname FROM Preferences WHERE isactive = '1';", sQLiteConnection))
				{
					using SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader();
					if (sQLiteDataReader.Read())
					{
						user = sQLiteDataReader["nickname"].ToString();
					}
					sQLiteDataReader.Close();
				}
				sQLiteConnection.Close();
			});
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "HasActiveUser", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\PreferencesRepository.cs", 303);
			return "";
		}
		return user;
	}

	public async Task LoadUserPreferencesToConfig(string nickname, bool isSignIn)
	{
		try
		{
			await EnsurePreferencesTableExistsAsync();
			await Task.Run(delegate
			{
				using SQLiteConnection sQLiteConnection = new SQLiteConnection(ConnectionString);
				sQLiteConnection.Open();
				using (SQLiteCommand sQLiteCommand = new SQLiteCommand("SELECT * FROM Preferences WHERE nickname = @nickname;", sQLiteConnection))
				{
					sQLiteCommand.Parameters.AddWithValue("@nickname", nickname);
					using SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader();
					if (sQLiteDataReader.Read())
					{
						LoadPreference(sQLiteDataReader, "nickname", "nickname");
						LoadPreference(sQLiteDataReader, "accesstoken", "access_token");
						LoadPreference(sQLiteDataReader, "idtoken", "id_token");
						LoadPreference(sQLiteDataReader, "expiresin", "expires_in");
						LoadPreference(sQLiteDataReader, "tokentype", "token_type");
						LoadPreference(sQLiteDataReader, "refreshtoken", "refresh_token");
						LoadPreference(sQLiteDataReader, "expiresat", "expires_at");
						LoadPreference(sQLiteDataReader, "nextaivpnid", "nextaivpn_id");
						LoadPreference(sQLiteDataReader, "subscriptiontype", "subscription_type");
						LoadPreference(sQLiteDataReader, "protocol", "protocol");
						LoadPreference(sQLiteDataReader, "protocolType", "protocolType");
						LoadPreference(sQLiteDataReader, "scramble", "scramble");
						LoadPreference(sQLiteDataReader, "connectedto", "ConnectedTo");
						LoadPreference(sQLiteDataReader, "startup", "Startup");
						LoadPreference(sQLiteDataReader, "autoconnect", "AutoConnect");
						LoadPreference(sQLiteDataReader, "killswitch", "KillSwitch");
						LoadPreference(sQLiteDataReader, "blocklan", "BlockLAN");
						LoadPreference(sQLiteDataReader, "lastfeedbacksent", "LastFeedbackSent");
						LoadPreference(sQLiteDataReader, "lastversioncheck", "LastVersionCheck");
						LoadPreference(sQLiteDataReader, "lastfeedbackclosed", "LastFeedbackClosed");
						LoadPreference(sQLiteDataReader, "iunderstand", "IUnderstand");
						LoadPreference(sQLiteDataReader, "favoriteslist", "FavoritesList");
						LoadPreference(sQLiteDataReader, "lastconnected", "LastConnected");
						LoadPreference(sQLiteDataReader, "isbestavailable", "IsBestAvailable");
						LoadPreference(sQLiteDataReader, "localenabled", "LocalEnabled");
						LoadPreference(sQLiteDataReader, AppSettingsKeys.SplitTunnelingAppList, AppSettingsKeys.SplitTunnelingAppList);
						LoadPreference(sQLiteDataReader, AppSettingsKeys.SplitTunnelingHostnameIpsList, AppSettingsKeys.SplitTunnelingHostnameIpsList);
					}
					sQLiteDataReader.Close();
				}
				sQLiteConnection.Close();
			});
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "LoadUserPreferencesToConfig", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\PreferencesRepository.cs", 367);
		}
	}

	private void LoadPreference(SQLiteDataReader reader, string columnName, string settingsKey)
	{
		string text = reader[columnName].ToString();
		if (SensitiveColumns.Contains(columnName))
		{
			text = _secretProtector.Unprotect(text);
		}
		_appSettingsHelper.SetValue(settingsKey, (!string.IsNullOrEmpty(text)) ? text : _appSettingsHelper.GetValue(settingsKey));
	}

	public Task SaveSignInPrimaryInfo()
	{
		return Task.Run(delegate
		{
			using SQLiteConnection sQLiteConnection = new SQLiteConnection(ConnectionString);
			try
			{
				sQLiteConnection.Open();
				using SQLiteCommand sQLiteCommand = new SQLiteCommand("INSERT INTO Preferences (nickname,accesstoken,idtoken,expiresin,tokentype,refreshtoken,expiresat,nextaivpnid,vpnusername,vpnpassword,subscriptiontype,protocol,protocoltype,scramble,connectedto,lastconnected,isbestavailable,startup,autoconnect,killswitch,blocklan,lastfeedbacksent,lastversioncheck,lastfeedbackclosed,iunderstand,favoriteslist,localenabled,isactive) VALUES (@nickname,@accesstoken,@idtoken,@expiresin,@tokentype,@refreshtoken,@expiresat,@nextaivpnid,@vpnusername,@vpnpassword,@subscriptiontype,@protocol,@protocoltype,@scramble,@connectedto,@lastconnected,@isbestavailable,@startup,@autoconnect,@killswitch,@blocklan,@lastfeedbacksent,@lastversioncheck,@lastfeedbackclosed,@iunderstand,@favoriteslist,@localenabled,@isactive)", sQLiteConnection);
				AddSettingParameter(sQLiteCommand, "@nickname", "nickname");
				AddProtectedSettingParameter(sQLiteCommand, "@accesstoken", "access_token");
				AddProtectedSettingParameter(sQLiteCommand, "@idtoken", "id_token");
				AddSettingParameter(sQLiteCommand, "@expiresin", "expires_in");
				AddSettingParameter(sQLiteCommand, "@tokentype", "token_type");
				AddProtectedSettingParameter(sQLiteCommand, "@refreshtoken", "refresh_token");
				AddSettingParameter(sQLiteCommand, "@expiresat", "expires_at");
				AddSettingParameter(sQLiteCommand, "@nextaivpnid", "nextaivpn_id");
				sQLiteCommand.Parameters.AddWithValue("@vpnusername", string.Empty);
				sQLiteCommand.Parameters.AddWithValue("@vpnpassword", string.Empty);
				AddSettingParameter(sQLiteCommand, "@subscriptiontype", "subscription_type");
				AddSettingParameter(sQLiteCommand, "@protocol", "protocol");
				AddSettingParameter(sQLiteCommand, "@protocoltype", "protocolType");
				AddSettingParameter(sQLiteCommand, "@scramble", "scramble");
				AddSettingParameter(sQLiteCommand, "@connectedto", "ConnectedTo");
				AddSettingParameter(sQLiteCommand, "@lastconnected", "LastConnected");
				AddSettingParameter(sQLiteCommand, "@isbestavailable", "IsBestAvailable");
				AddSettingParameter(sQLiteCommand, "@startup", "Startup");
				AddSettingParameter(sQLiteCommand, "@autoconnect", "AutoConnect");
				AddSettingParameter(sQLiteCommand, "@killswitch", "KillSwitch");
				AddSettingParameter(sQLiteCommand, "@blocklan", "BlockLAN");
				AddSettingParameter(sQLiteCommand, "@lastfeedbacksent", "LastFeedbackSent");
				AddSettingParameter(sQLiteCommand, "@lastversioncheck", "LastVersionCheck");
				AddSettingParameter(sQLiteCommand, "@lastfeedbackclosed", "LastFeedbackClosed");
				AddSettingParameter(sQLiteCommand, "@iunderstand", "IUnderstand");
				AddSettingParameter(sQLiteCommand, "@favoriteslist", "FavoritesList");
				AddSettingParameter(sQLiteCommand, "@localenabled", "LocalEnabled");
				sQLiteCommand.Parameters.AddWithValue("@isactive", "1");
				sQLiteCommand.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				_logger?.Error($"{"PreferencesRepository"}.{"SaveSignInPrimaryInfo"} failed: {ex.Message}", "SaveSignInPrimaryInfo", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\PreferencesRepository.cs", 437);
			}
			finally
			{
				sQLiteConnection.Close();
			}
		});
	}

	public Task UpdateSignInPrimaryInfo()
	{
		return Task.Run(delegate
		{
			using SQLiteConnection sQLiteConnection = new SQLiteConnection(ConnectionString);
			try
			{
				sQLiteConnection.Open();
				using SQLiteCommand sQLiteCommand = new SQLiteCommand("UPDATE Preferences SET accesstoken = @accesstoken, idtoken = @idtoken, expiresin = @expiresin, tokentype = @tokentype, refreshtoken = @refreshtoken, expiresat = @expiresat, nextaivpnid = @nextaivpnid, subscriptiontype = @subscriptiontype, isactive = '1' WHERE nickname = @nickname", sQLiteConnection);
				AddProtectedSettingParameter(sQLiteCommand, "@accesstoken", "access_token");
				AddProtectedSettingParameter(sQLiteCommand, "@idtoken", "id_token");
				AddSettingParameter(sQLiteCommand, "@expiresin", "expires_in");
				AddSettingParameter(sQLiteCommand, "@tokentype", "token_type");
				AddProtectedSettingParameter(sQLiteCommand, "@refreshtoken", "refresh_token");
				AddSettingParameter(sQLiteCommand, "@expiresat", "expires_at");
				AddSettingParameter(sQLiteCommand, "@nextaivpnid", "nextaivpn_id");
				AddSettingParameter(sQLiteCommand, "@subscriptiontype", "subscription_type");
				sQLiteCommand.Parameters.AddWithValue("@nickname", GetConfigNickname());
				sQLiteCommand.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				_logger?.Error($"{"PreferencesRepository"}.{"UpdateSignInPrimaryInfo"} failed: {ex.Message}", "UpdateSignInPrimaryInfo", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\PreferencesRepository.cs", 477);
			}
			finally
			{
				sQLiteConnection.Close();
			}
		});
	}

	private void AddSettingParameter(SQLiteCommand command, string parameterName, string settingsKey)
	{
		command.Parameters.AddWithValue(parameterName, _appSettingsHelper.GetValue(settingsKey));
	}

	private void AddProtectedSettingParameter(SQLiteCommand command, string parameterName, string settingsKey)
	{
		command.Parameters.AddWithValue(parameterName, _secretProtector.Protect(_appSettingsHelper.GetValue(settingsKey)));
	}

	public void SaveSinglePreference(string preference, string value)
	{
		if (!PreferenceColumns.Contains(preference))
		{
			_logger?.Error("SaveSinglePreference rejected unknown preference column '" + preference + "'", "SaveSinglePreference", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\PreferencesRepository.cs", 502);
			return;
		}
		if (SensitiveColumns.Contains(preference))
		{
			value = _secretProtector.Protect(value);
		}
		using SQLiteConnection sQLiteConnection = new SQLiteConnection(ConnectionString);
		try
		{
			sQLiteConnection.Open();
			using SQLiteCommand sQLiteCommand = new SQLiteCommand("UPDATE Preferences SET " + preference + " = @value WHERE nickname = @nickname", sQLiteConnection);
			sQLiteCommand.Parameters.AddWithValue("@value", value);
			sQLiteCommand.Parameters.AddWithValue("@nickname", GetConfigNickname());
			sQLiteCommand.ExecuteNonQuery();
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "SaveSinglePreference", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\PreferencesRepository.cs", 527);
		}
		finally
		{
			sQLiteConnection.Close();
		}
	}

	public async Task SetUserActiveStatus(int status)
	{
		await Task.Run(delegate
		{
			using SQLiteConnection sQLiteConnection = new SQLiteConnection(ConnectionString);
			try
			{
				sQLiteConnection.Open();
				using SQLiteCommand sQLiteCommand = new SQLiteCommand("UPDATE Preferences SET isactive = @status WHERE nickname = @nickname;", sQLiteConnection);
				sQLiteCommand.Parameters.AddWithValue("@status", status.ToString());
				sQLiteCommand.Parameters.AddWithValue("@nickname", GetConfigNickname());
				sQLiteCommand.ExecuteNonQuery();
			}
			catch (Exception exception)
			{
				_logger?.Error(exception, "SetUserActiveStatus", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\PreferencesRepository.cs", 556);
			}
			finally
			{
				sQLiteConnection.Close();
			}
		});
	}

	public void SaveUserPreferences(string username)
	{
		if (!File.Exists(VPNConstants.FilePath.UserPreferencesFilePath))
		{
			new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("Preferences", new XElement("User", new XAttribute("name", username), GetPreferenceElements()))).Save(VPNConstants.FilePath.UserPreferencesFilePath);
			return;
		}
		XDocument xDocument = XDocument.Load(VPNConstants.FilePath.UserPreferencesFilePath);
		string value = _appSettingsHelper.GetValue("Preferences");
		if (string.IsNullOrEmpty(value))
		{
			return;
		}
		string[] array = value.Split(',');
		XElement xElement = xDocument.Element("Preferences");
		if (xElement == null)
		{
			_logger?.Error("UserPreferences file has an unexpected root element", "SaveUserPreferences", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\PreferencesRepository.cs", 593);
			return;
		}
		if (SearchUserPreferences(username))
		{
			XElement xElement2 = xElement.Elements("User").FirstOrDefault((XElement x) => x.Attribute("name")?.Value == username);
			if (xElement2 != null)
			{
				string[] array2 = array;
				foreach (string text in array2)
				{
					xElement2.SetElementValue(text, _appSettingsHelper.GetValue(text));
				}
			}
		}
		else
		{
			xElement.Add(new XElement("User", new XAttribute("name", username), GetPreferenceElements()));
		}
		xDocument.Save(VPNConstants.FilePath.UserPreferencesFilePath);
	}

	private XElement[] GetPreferenceElements()
	{
		string[] array = _appSettingsHelper.GetValue("Preferences").Split(',');
		List<XElement> list = new List<XElement>();
		string[] array2 = array;
		foreach (string text in array2)
		{
			list.Add(new XElement(text, _appSettingsHelper.GetValue(text)));
		}
		return list.ToArray();
	}

	public bool SearchUserPreferences(string username)
	{
		bool result = false;
		XmlDocument xmlDocument = new XmlDocument();
		if (File.Exists(VPNConstants.FilePath.UserPreferencesFilePath))
		{
			xmlDocument.Load(VPNConstants.FilePath.UserPreferencesFilePath);
			foreach (XmlNode item in xmlDocument.DocumentElement)
			{
				if (item.Attributes != null && item.Attributes.Count != 0 && item.Attributes[0].InnerText == username)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}
}
