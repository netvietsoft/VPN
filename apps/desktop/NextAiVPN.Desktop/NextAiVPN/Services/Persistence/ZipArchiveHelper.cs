using System.IO;
using System.IO.Compression;

namespace NextAiVPN.Services.Persistence;

internal static class ZipArchiveHelper
{
	public static void CreateZipArchive(string filePath, string archivePath, string archiveFileName)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (ZipArchive destination = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
		{
			destination.CreateEntryFromFile(filePath, archiveFileName);
		}
		using FileStream destination2 = new FileStream(archivePath, FileMode.Create);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		memoryStream.CopyTo(destination2);
	}
}
