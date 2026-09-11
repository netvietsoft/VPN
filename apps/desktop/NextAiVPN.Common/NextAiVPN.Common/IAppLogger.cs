using System;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using Serilog.Events;

namespace NextAiVPN.Common;

public interface IAppLogger
{
	ReplaySubject<LogEvent> LogSubject { get; set; }

	void Error(string message, [CallerMemberName] string callingMethod = null, [CallerFilePath] string callingFilePath = null, [CallerLineNumber] int callingFileLineNumber = 0);

	void Error(Exception exception, [CallerMemberName] string callingMethod = null, [CallerFilePath] string callingFilePath = null, [CallerLineNumber] int callingFileLineNumber = 0);

	void Information(string message, [CallerMemberName] string callingMethod = null, [CallerFilePath] string callingFilePath = null, [CallerLineNumber] int callingFileLineNumber = 0);

	void Warning(string message, [CallerMemberName] string callingMethod = null, [CallerFilePath] string callingFilePath = null, [CallerLineNumber] int callingFileLineNumber = 0);
}
