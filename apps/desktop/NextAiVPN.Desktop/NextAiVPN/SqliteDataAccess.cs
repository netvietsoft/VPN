using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using Dapper;
using NextAiVPN.Entities;

namespace NextAiVPN;

public class SqliteDataAccess
{
	public static List<LogInformation> GetAll()
	{
		using IDbConnection cnn = new SQLiteConnection(GetConnectionString());
		return cnn.Query<LogInformation>("SELECT * FROM Logs", new DynamicParameters()).ToList();
	}

	public static void Insert(LogInformation logInfo)
	{
		using IDbConnection cnn = new SQLiteConnection(GetConnectionString());
		cnn.Execute("INSERT INTO Logs (LogDate, LogInfo) VALUES (@LogDate, @LogInfo)", logInfo);
	}

	public static void DeleteAll()
	{
		using IDbConnection cnn = new SQLiteConnection(GetConnectionString());
		cnn.Query<LogInformation>("DELETE FROM Logs", new DynamicParameters());
	}

	private static string GetConnectionString()
	{
		return "Data Source=" + VPNConstants.FilePath.AppLogsDbFilePath + ";Version=3;";
	}
}
