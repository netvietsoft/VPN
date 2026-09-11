using System;
using System.Collections.Generic;
using System.Formats.Nrbf;
using System.IO;
using System.Linq;
using NextAiVPN.Common;

namespace NextAiVPN.Services.Persistence;

internal class LegacyBinaryFallbackSerializer : ISerializer<IEnumerable<string>>
{
	private enum LegacyReadResult
	{
		NotLegacy,
		Success,
		Corrupt
	}

	private readonly ISerializer<IEnumerable<string>> _inner;

	private readonly IAppLogger _logger;

	public LegacyBinaryFallbackSerializer(ISerializer<IEnumerable<string>> inner, IAppLogger logger)
	{
		_inner = inner;
		_logger = logger;
	}

	public void Serialize(IEnumerable<string> data, string filePath)
	{
		_inner.Serialize(data, filePath);
	}

	public IEnumerable<string> Deserialize(string filePath)
	{
		List<string> items;
		switch (TryReadLegacyBinaryFile(filePath, out items))
		{
		case LegacyReadResult.Success:
			if (TryRewriteAsJson(filePath, items))
			{
				_logger?.Information($"[LegacyMigration] Migrated legacy binary file to JSON: {filePath} ({items.Count} items)", "Deserialize", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\LegacyBinaryFallbackSerializer.cs", 63);
			}
			else
			{
				_logger?.Error("[LegacyMigration] Rewrite to JSON failed; keeping legacy file for retry on next start: " + filePath, "Deserialize", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\LegacyBinaryFallbackSerializer.cs", 68);
			}
			return items;
		case LegacyReadResult.Corrupt:
			return null;
		default:
			return _inner.Deserialize(filePath);
		}
	}

	private LegacyReadResult TryReadLegacyBinaryFile(string filePath, out List<string> items)
	{
		items = null;
		try
		{
			if (!File.Exists(filePath))
			{
				return LegacyReadResult.NotLegacy;
			}
			using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			if (fileStream.Length == 0L || !NrbfDecoder.StartsWithPayloadHeader(fileStream))
			{
				return LegacyReadResult.NotLegacy;
			}
			fileStream.Position = 0L;
			SerializationRecord serializationRecord = NrbfDecoder.Decode(fileStream);
			if (serializationRecord is SZArrayRecord<string> sZArrayRecord)
			{
				items = sZArrayRecord.GetArray().ToList();
				return LegacyReadResult.Success;
			}
			if (serializationRecord is ClassRecord classRecord && classRecord.TypeNameMatches(typeof(List<string>)))
			{
				int @int = classRecord.GetInt32("_size");
				string[] array = ((SZArrayRecord<string>)classRecord.GetArrayRecord("_items")).GetArray();
				if (@int < 0 || @int > array.Length)
				{
					_logger?.Error($"[LegacyMigration] Invalid List size {@int} for backing array of {array.Length} in {filePath}", "TryReadLegacyBinaryFile", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\LegacyBinaryFallbackSerializer.cs", 126);
					items = null;
					return LegacyReadResult.Corrupt;
				}
				items = array.Take(@int).ToList();
				return LegacyReadResult.Success;
			}
			_logger?.Error("[LegacyMigration] Unrecognized legacy payload root '" + serializationRecord.TypeName.FullName + "' in " + filePath, "TryReadLegacyBinaryFile", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\LegacyBinaryFallbackSerializer.cs", 136);
			return LegacyReadResult.Corrupt;
		}
		catch (Exception ex)
		{
			_logger?.Error("[LegacyMigration] Failed to read legacy binary file '" + filePath + "' - " + ex.Message, "TryReadLegacyBinaryFile", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\LegacyBinaryFallbackSerializer.cs", 143);
			items = null;
			return LegacyReadResult.Corrupt;
		}
	}

	private bool TryRewriteAsJson(string filePath, List<string> items)
	{
		string text = filePath + ".migrating";
		try
		{
			_inner.Serialize(items, text);
			IEnumerable<string> enumerable = _inner.Deserialize(text);
			if (enumerable == null || enumerable.Count() != items.Count)
			{
				return false;
			}
			File.Copy(filePath, filePath + ".bak", overwrite: true);
			File.Move(text, filePath, overwrite: true);
			return true;
		}
		catch (Exception ex)
		{
			_logger?.Error("[LegacyMigration] Failed to rewrite '" + filePath + "' as JSON - " + ex.Message, "TryRewriteAsJson", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\LegacyBinaryFallbackSerializer.cs", 175);
			return false;
		}
		finally
		{
			try
			{
				if (File.Exists(text))
				{
					File.Delete(text);
				}
			}
			catch
			{
			}
		}
	}
}
