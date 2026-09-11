using System;
using System.IO;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using Serilog;
using Serilog.Events;

namespace NextAiVPN.Common;

public class AppLogger : IAppLogger
{
	public ReplaySubject<LogEvent> LogSubject { get; set; } = new ReplaySubject<LogEvent>();

	public AppLogger(string filePath)
	{
		Initialize(filePath);
	}

	public void Error(string message, [CallerMemberName] string callingMethod = null, [CallerFilePath] string callingFilePath = null, [CallerLineNumber] int callingFileLineNumber = 0)
	{
		Log.Error($"Error in method '{callingMethod}' at {Path.GetFileNameWithoutExtension(callingFilePath)}:{callingFileLineNumber}: '{message}'");
	}

	public void Error(Exception exception, [CallerMemberName] string callingMethod = null, [CallerFilePath] string callingFilePath = null, [CallerLineNumber] int callingFileLineNumber = 0)
	{
		Log.Error($"Exception: '{exception.GetType()}'");
		Log.Error($"Error in method '{callingMethod}' at {Path.GetFileNameWithoutExtension(callingFilePath)}:{callingFileLineNumber}: '{exception.Message}'");
		Log.Error("StackTrace: '" + exception.StackTrace + "'");
	}

	public void Information(string message, [CallerMemberName] string callingMethod = null, [CallerFilePath] string callingFilePath = null, [CallerLineNumber] int callingFileLineNumber = 0)
	{
		Log.Information(FormatMessage("Message", message, callingMethod, Path.GetFileNameWithoutExtension(callingFilePath), callingFileLineNumber));
	}

	public void Warning(string message, [CallerMemberName] string callingMethod = null, [CallerFilePath] string callingFilePath = null, [CallerLineNumber] int callingFileLineNumber = 0)
	{
		Log.Warning(FormatMessage("Warning", message, callingMethod, Path.GetFileNameWithoutExtension(callingFilePath), callingFileLineNumber));
	}

	private static string FormatMessage(string level, string message, string method, string file, int line)
	{
		return $"{level}: '{message}'. Method: '{method}' at {file}:{line}";
	}

	private void Initialize(string diagnosticsFolder)
	{
		Log.Logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Observers(delegate(IObservable<LogEvent> e)
		{
			e.Subscribe(LogSubject.OnNext);
		}).WriteTo.Console(LogEventLevel.Verbose, "{Timestamp:dd:MM:yyyy HH:mm:ss} [{Level}] ({SourceContext}) {Message}{NewLine}{Exception}").WriteTo.File(Path.Combine(diagnosticsFolder, "diagnostics.txt"), LogEventLevel.Verbose, "{Timestamp:dd:MM:yyyy HH:mm:ss} [{Level}] ({SourceContext}) {Message}{NewLine}{Exception}", null, retainedFileCountLimit: 5, fileSizeLimitBytes: 2097152L, levelSwitch: null, buffered: false, shared: true, flushToDiskInterval: null, rollingInterval: RollingInterval.Infinite, rollOnFileSizeLimit: true).CreateLogger();
	}
}
