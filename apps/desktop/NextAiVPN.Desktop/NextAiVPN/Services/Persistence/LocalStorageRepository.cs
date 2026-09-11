using System;
using System.Data.Common;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NextAiVPN.Common;

namespace NextAiVPN.Services.Persistence;

internal class LocalStorageRepository : IStorageRepository
{
	private static readonly string _filePath = VPNConstants.FilePath.PreferencesDatabaseFolderPath + "\\UserPreferences.sqlite";

	private readonly string _connectionString = "Data Source=" + _filePath + ";Version=3;";

	private readonly string _tableName = "Preferences";

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	public LocalStorageRepository(IAppSettingsHelper appSettingsHelper, IAppLogger logger)
	{
		_appSettingsHelper = appSettingsHelper ?? throw new ArgumentNullException("appSettingsHelper");
		_logger = logger;
	}

	public async Task SaveValue(string variableName, string value)
	{
		EnsureValidColumnName(variableName);
		string userName = _appSettingsHelper.GetValue("nickname");
		using SQLiteConnection dbConnection = new SQLiteConnection(_connectionString);
		_ = 3;
		try
		{
			await dbConnection.OpenAsync();
			AddColumnIfNotExists(dbConnection, _tableName, variableName, "TEXT");
			if (!(await IsUserExists(userName)))
			{
				await InsertNewUser(variableName, value, dbConnection, userName);
			}
			string commandText = $"UPDATE {_tableName} SET {variableName} = @value WHERE nickname = @userName";
			using SQLiteCommand updateCommand = new SQLiteCommand(commandText, dbConnection);
			updateCommand.Parameters.AddWithValue("@value", value);
			updateCommand.Parameters.AddWithValue("@userName", userName);
			await updateCommand.ExecuteNonQueryAsync();
		}
		catch (Exception ex)
		{
			_logger?.Error(ex.Message, "SaveValue", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\LocalStorageRepository.cs", 63);
		}
	}

	private async Task InsertNewUser(string variableName, string value, SQLiteConnection dbConnection, string userName)
	{
		string commandText = $"INSERT INTO {_tableName}(nickname, {variableName}) VALUES (@userName, @value)";
		using SQLiteCommand insertCommand = new SQLiteCommand(commandText, dbConnection);
		insertCommand.Parameters.AddWithValue("@userName", userName);
		insertCommand.Parameters.AddWithValue("@value", value);
		await insertCommand.ExecuteNonQueryAsync();
	}

	public async Task<string> GetValue(string variableName)
	{
		EnsureValidColumnName(variableName);
		string userName = _appSettingsHelper.GetValue("nickname");
		using SQLiteConnection dbConnection = new SQLiteConnection(_connectionString);
		_ = 2;
		try
		{
			await dbConnection.OpenAsync();
			AddColumnIfNotExists(dbConnection, _tableName, variableName, "TEXT");
			if (!(await IsUserExists(userName)))
			{
				_logger?.Error("User not found. Username: " + userName, "GetValue", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\LocalStorageRepository.cs", 98);
				return null;
			}
			string commandText = "SELECT " + variableName + " FROM Preferences WHERE nickname = @userName LIMIT 1";
			using SQLiteCommand command = new SQLiteCommand(commandText, dbConnection);
			command.Parameters.AddWithValue("@userName", userName);
			object obj = await command.ExecuteScalarAsync();
			return (obj != null && obj != DBNull.Value) ? obj.ToString() : string.Empty;
		}
		catch (Exception ex)
		{
			_logger?.Error(ex.Message, "GetValue", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\LocalStorageRepository.cs", 116);
			return string.Empty;
		}
	}

	public async Task<bool> IsUserExists(string userName)
	{
		_ = 2;
		try
		{
			using SQLiteConnection dbConnection = new SQLiteConnection(_connectionString);
			await dbConnection.OpenAsync();
			string commandText = "SELECT 1 FROM Preferences WHERE nickname = @userName LIMIT 1;";
			using SQLiteCommand command = new SQLiteCommand(commandText, dbConnection);
			command.Parameters.AddWithValue("@userName", userName);
			using DbDataReader reader = await command.ExecuteReaderAsync();
			return await reader.ReadAsync();
		}
		catch (Exception ex)
		{
			_logger?.Error(ex.Message, "IsUserExists", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\LocalStorageRepository.cs", 144);
			return false;
		}
	}

	private static void EnsureValidColumnName(string columnName)
	{
		if (string.IsNullOrEmpty(columnName) || !Regex.IsMatch(columnName, "^[A-Za-z0-9_]+$"))
		{
			throw new ArgumentException("Invalid column name: '" + columnName + "'", "columnName");
		}
	}

	private static void AddColumnIfNotExists(SQLiteConnection connection, string tableName, string columnName, string columnDefinition)
	{
		EnsureValidColumnName(columnName);
		SQLiteCommand sQLiteCommand = new SQLiteCommand("PRAGMA table_info(" + tableName + ");", connection);
		SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader();
		bool flag = false;
		while (sQLiteDataReader.Read())
		{
			if (string.Equals(sQLiteDataReader["name"].ToString(), columnName, StringComparison.OrdinalIgnoreCase))
			{
				flag = true;
				break;
			}
		}
		sQLiteDataReader.Close();
		sQLiteCommand.Dispose();
		if (!flag)
		{
			SQLiteCommand sQLiteCommand2 = new SQLiteCommand($"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition};", connection);
			sQLiteCommand2.ExecuteNonQuery();
			sQLiteCommand2.Dispose();
		}
	}
}
