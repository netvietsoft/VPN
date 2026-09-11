using NextAiVPN.Common;

namespace NextAiVPN.Streaming;

public class Logger
{
	public static IAppLogger Log { get; private set; }

	public static void GetLoggerInstance(IAppLogger logger)
	{
		Log = logger;
	}
}
