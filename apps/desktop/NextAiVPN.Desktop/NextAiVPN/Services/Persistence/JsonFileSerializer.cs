using System;
using System.IO;
using System.Text.Json;
using NextAiVPN.Common;

namespace NextAiVPN.Services.Persistence;

internal class JsonFileSerializer<T> : ISerializer<T>
{
	private readonly IAppLogger _logger;

	public JsonFileSerializer(IAppLogger logger)
	{
		_logger = logger;
	}

	public void Serialize(T data, string filePath)
	{
		try
		{
			using FileStream utf8Json = new FileStream(filePath, FileMode.Create);
			JsonSerializer.Serialize(utf8Json, data);
		}
		catch (Exception ex)
		{
			_logger?.Error($"[SerializationError] - Serialize. Type: {typeof(T)} - {ex.Message}", "Serialize", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\JsonFileSerializer.cs", 46);
		}
	}

	public T Deserialize(string filePath)
	{
		T result = default(T);
		try
		{
			if (!File.Exists(filePath))
			{
				return result;
			}
			using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			if (fileStream.Length == 0L)
			{
				return result;
			}
			result = JsonSerializer.Deserialize<T>(fileStream);
			return result;
		}
		catch (Exception ex)
		{
			_logger?.Error($"[SerializationError] - Deserialize. Type: {typeof(T)} - {ex.Message}", "Deserialize", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\JsonFileSerializer.cs", 75);
		}
		return result;
	}
}
