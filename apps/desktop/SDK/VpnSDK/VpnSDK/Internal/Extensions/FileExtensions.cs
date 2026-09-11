using System.IO;
using System.IO.Compression;
using System.Text;

namespace VpnSDK.Internal.Extensions;

internal static class FileExtensions
{
	public static void WriteAllTextCompressed(string path, string contents)
	{
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		byte[] bytes = Encoding.Default.GetBytes(contents);
		using FileStream stream = File.OpenWrite(path);
		using GZipStream gZipStream = new GZipStream(stream, CompressionLevel.Fastest);
		gZipStream.Write(bytes, 0, bytes.Length);
	}

	public static string ReadAllTextCompressed(string path)
	{
		using FileStream stream = File.OpenRead(path);
		using GZipStream stream2 = new GZipStream(stream, CompressionMode.Decompress);
		using StreamReader streamReader = new StreamReader(stream2, Encoding.Default);
		return streamReader.ReadToEnd();
	}
}
