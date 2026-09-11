using System;
using System.Collections.Concurrent;
using System.IO;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace VpnSDK.Private.WFP.Utilities;

internal static class LogProvider
{
	private static readonly ConcurrentDictionary<string, Microsoft.Extensions.Logging.ILogger> _loggers = new ConcurrentDictionary<string, Microsoft.Extensions.Logging.ILogger>();

	private static ILoggerFactory _loggerFactory;

	private static readonly object _lock = new object();

	public static void SetLogFactory(ILoggerFactory loggerFactoryArg = null)
	{
		lock (_lock)
		{
			if (_loggerFactory == null && loggerFactoryArg != null)
			{
				_loggerFactory = loggerFactoryArg;
			}
		}
	}

	public static Microsoft.Extensions.Logging.ILogger GetLogger(string loggerName)
	{
		if (string.IsNullOrWhiteSpace(loggerName))
		{
			throw new ArgumentException("Logger name cannot be null or empty.", "loggerName");
		}
		if (_loggers.TryGetValue(loggerName, out var value))
		{
			return value;
		}
		lock (_lock)
		{
			if (_loggerFactory == null && !CreateLoggerFactory())
			{
				return null;
			}
			try
			{
				Microsoft.Extensions.Logging.ILogger logger = _loggerFactory.CreateLogger(loggerName);
				if (logger != null)
				{
					_loggers.TryAdd(loggerName, logger);
				}
				return logger;
			}
			catch
			{
				return null;
			}
		}
	}

	private static bool CreateLoggerFactory()
	{
		try
		{
			Logger logger = FileLoggerConfigurationExtensions.File(path: Path.Combine(AppContext.BaseDirectory, "wfpLog.txt"), sinkConfiguration: new LoggerConfiguration().WriteTo, restrictedToMinimumLevel: LogEventLevel.Verbose, outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] ({SourceContext}) {Message}{NewLine}{Exception}", formatProvider: null, fileSizeLimitBytes: 1073741824L, levelSwitch: null, buffered: false, shared: false, flushToDiskInterval: null, rollingInterval: RollingInterval.Infinite, rollOnFileSizeLimit: false, retainedFileCountLimit: 31).CreateLogger();
			SerilogLoggerProvider serilogProvider = new SerilogLoggerProvider(logger);
			_loggerFactory = LoggerFactory.Create(delegate(ILoggingBuilder builder)
			{
				builder.AddProvider(serilogProvider);
			});
			return true;
		}
		catch
		{
			return false;
		}
	}
}
