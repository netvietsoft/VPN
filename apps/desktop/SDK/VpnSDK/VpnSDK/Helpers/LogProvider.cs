using System;
using System.Collections.Concurrent;
using System.IO;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace VpnSDK.Helpers;

internal static class LogProvider
{
	private static readonly ConcurrentDictionary<string, Microsoft.Extensions.Logging.ILogger> _loggers = new ConcurrentDictionary<string, Microsoft.Extensions.Logging.ILogger>();

	private static readonly object _lock = new object();

	private static ILoggerFactory _loggerFactory;

	public static ILoggerFactory LoggerFactoryInstance
	{
		get
		{
			lock (_lock)
			{
				if (_loggerFactory == null)
				{
					CreateLoggerFactory();
				}
				return _loggerFactory;
			}
		}
	}

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

	public static ILogger<T> GetLogger<T>()
	{
		lock (_lock)
		{
			if (_loggerFactory == null)
			{
				CreateLoggerFactory();
			}
			return _loggerFactory.CreateLogger<T>();
		}
	}

	private static bool CreateLoggerFactory()
	{
		try
		{
			SerilogLoggerProvider serilogProvider;
			if (Log.Logger != null)
			{
				serilogProvider = new SerilogLoggerProvider(Log.Logger);
			}
			else
			{
				string path = Path.Combine(AppContext.BaseDirectory, "vpnSDKLog.txt");
				Logger logger = new LoggerConfiguration().WriteTo.File(path, LogEventLevel.Verbose, "{Timestamp:HH:mm:ss} [{Level}] ({SourceContext}) {Message}{NewLine}{Exception}", null, 1073741824L, null, buffered: false, shared: false, null, RollingInterval.Infinite, rollOnFileSizeLimit: false, 31).CreateLogger();
				serilogProvider = new SerilogLoggerProvider(logger);
			}
			_loggerFactory = LoggerFactory.Create(delegate(ILoggingBuilder builder)
			{
				builder.SetMinimumLevel(LogLevel.Trace).AddProvider(serilogProvider);
			});
			return true;
		}
		catch
		{
			return false;
		}
	}
}
