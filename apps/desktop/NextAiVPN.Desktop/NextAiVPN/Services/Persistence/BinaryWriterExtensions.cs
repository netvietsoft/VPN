using System.IO;
using System.Threading.Tasks;

namespace NextAiVPN.Services.Persistence;

public static class BinaryWriterExtensions
{
	public static Task FlushAsync(this BinaryWriter writer)
	{
		if (writer.BaseStream is FileStream fileStream)
		{
			return fileStream.FlushAsync();
		}
		return Task.CompletedTask;
	}
}
