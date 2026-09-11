using System;
using System.Globalization;
using Microsoft.Win32.TaskScheduler;
using NextAiVPN.Common;

namespace NextAiVPN.Services;

internal class StartUpService : IStartUpService
{
	private readonly IBugsnagService _bugsnagService;

	private readonly IAppLogger _logger;

	public StartUpService(IBugsnagService bugsnagService, IAppLogger logger)
	{
		_bugsnagService = bugsnagService;
		_logger = logger;
	}

	public void RemoveStartUp()
	{
		try
		{
			using TaskService taskService = new TaskService();
			if (IsStartUpTaskCreated())
			{
				taskService.RootFolder.DeleteTask("NextAiVPN_Boot");
			}
		}
		catch (Exception ex)
		{
			string msg = string.Format(CultureInfo.InvariantCulture, "[Error] - Path: {0}.{1}() - {2}", "StartUpService", "RemoveStartUp", ex.Message);
			LogError(msg);
		}
	}

	public void AddStartUp()
	{
		try
		{
			if (IsStartUpTaskCreated())
			{
				return;
			}
			using TaskService taskService = new TaskService();
			TaskDefinition taskDefinition = taskService.NewTask();
			taskDefinition.RegistrationInfo.Description = "Runs NextAiVPN on the boot";
			taskDefinition.Principal.RunLevel = TaskRunLevel.Highest;
			taskDefinition.Triggers.Add(new LogonTrigger());
			taskDefinition.Actions.Add(new ExecAction(VPNConstants.FilePath.NextAiVpnExeFilePath));
			taskService.RootFolder.RegisterTaskDefinition("NextAiVPN_Boot", taskDefinition);
		}
		catch (Exception ex) when (ex.Message.Contains("Access is denied") || ex is UnauthorizedAccessException)
		{
			_logger?.Warning("[StartUpService] Run as non-admin, startup task skipped: " + ex.Message, "AddStartUp", "StartUpService.cs", 56);
		}
		catch (Exception ex)
		{
			string msg = string.Format(CultureInfo.InvariantCulture, "[Error] - Path: {0}.{1}() - {2}", "StartUpService", "AddStartUp", ex.Message);
			LogError(msg);
		}
	}

	private void RunTask()
	{
		try
		{
			using TaskService taskService = new TaskService();
			taskService.FindTask("NextAiVPN_Boot")?.Run();
		}
		catch (Exception ex)
		{
			string msg = string.Format(CultureInfo.InvariantCulture, "[Error] - Path: {0}.{1}() - {2}", "StartUpService", "RunTask", ex.Message);
			LogError(msg);
		}
	}

	private bool IsStartUpTaskCreated()
	{
		try
		{
			using TaskService taskService = new TaskService();
			return taskService.FindTask("NextAiVPN_Boot") != null;
		}
		catch (Exception ex)
		{
			string msg = string.Format(CultureInfo.InvariantCulture, "[Error] - Path: {0}.{1}() - {2}", "StartUpService", "IsStartUpTaskCreated", ex.Message);
			LogError(msg);
			return false;
		}
	}

	private void LogError(string msg)
	{
		_logger?.Error(msg, "LogError", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\StartUpService.cs", 125);
		_bugsnagService.Notify(msg);
	}
}
