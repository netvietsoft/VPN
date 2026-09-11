using System;
using System.IO;
using NextAiVPN.Common;

namespace NextAiVPN.Services.Persistence;

internal class MsiFileCleaner : IMsiFileCleaner
{
	private readonly IAppLogger _logger;

	public MsiFileCleaner(IAppLogger logger)
	{
		_logger = logger;
	}

	public void CleanUp()
	{
		try
		{
			if (File.Exists(VPNConstants.FilePath.NextAiVpnInstallFilePath))
			{
				File.Delete(VPNConstants.FilePath.NextAiVpnInstallFilePath);
			}
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "CleanUp", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\MsiFileCleaner.cs", 38);
		}
	}
}
