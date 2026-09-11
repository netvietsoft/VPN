using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VpnSDK.Common.Helpers;
using VpnSDK.Private.OpenVpn.Enums;
using VpnSDK.Private.OpenVpn.Extensions;
using VpnSDK.Private.OpenVpn.Helpers;

namespace VpnSDK.Private.OpenVpn.Driver;

public class DriverManager
{
	public delegate void TapDriverUpdated(TapDriver driver);

	private readonly string _driverInf;

	private readonly string _deviceName;

	private readonly string _driverSysfilePath;

	private readonly string _devconPath;

	private readonly ILogger _devconLog;

	private readonly OpenVpnTapAdapter _preferredTapAdapter;

	public TapDriver DetectedDriver;

	public bool IsInstalled => GetInstalledTapDriver() != TapDriver.NotInstalled;

	internal Version Version => FileHelper.GetVersion(_driverSysfilePath);

	public event TapDriverUpdated OnDriverUpdated;

	public DriverManager(string driverPath, OpenVpnTapAdapter preferredTapAdapter, string devconPath = null, ILoggerFactory loggerFactory = null)
	{
		LogProvider.SetLogFactory(loggerFactory);
		_devconLog = LogProvider.GetLogger("VpnSDK::Private::OpenVPN::TAP");
		_devconLog?.LogInformation($"DriverManager driverPath: {driverPath}, preferredTapAdapter: {preferredTapAdapter}, devconPath: {devconPath}");
		if (!Directory.Exists(driverPath))
		{
			throw new DirectoryNotFoundException("Directory \"" + driverPath + "\" does not exist.");
		}
		if (devconPath != null)
		{
			if (!devconPath.EndsWith("exe"))
			{
				throw new IOException("Invalid devcon executable provided.");
			}
			if (!File.Exists(devconPath))
			{
				throw new FileNotFoundException("Devcon executable provided does not exist.");
			}
		}
		else
		{
			devconPath = Directory.GetFiles(driverPath).FirstOrDefault((string x) => x.EndsWith("exe"));
			if (devconPath == null)
			{
				throw new FileNotFoundException("Unable to find devcon executable.");
			}
		}
		if (!FileVersionInfo.GetVersionInfo(devconPath).FileDescription.Contains("Windows Setup API"))
		{
			throw new InvalidOperationException("Devcon executable is not a valid Windows DDK executable.");
		}
		string[] files = Directory.GetFiles(driverPath);
		foreach (string text in files)
		{
			if (string.IsNullOrEmpty(_driverInf) && text.EndsWith(".inf"))
			{
				_driverInf = text;
			}
			else if (string.IsNullOrEmpty(_deviceName) && text.EndsWith(".sys"))
			{
				_driverSysfilePath = text;
				_deviceName = Path.GetFileNameWithoutExtension(text);
			}
		}
		if (_driverInf == null || _deviceName == null || _driverSysfilePath == null)
		{
			throw new InvalidOperationException("Driver files are missing.");
		}
		_devconPath = devconPath;
		_preferredTapAdapter = preferredTapAdapter;
		DetectedDriver = GetInstalledTapDriver();
		_devconLog?.LogInformation($"devconPath: {devconPath} and detectedDriver: {DetectedDriver}");
	}

	public async Task<DevconReturnCode> Install()
	{
		DevconReturnCode num = await PerformOperation("install", "\"" + Path.Combine(new string[1] { _driverInf.Replace("\\\\", "\\") }) + "\"", _deviceName).ConfigureAwait(continueOnCapturedContext: false);
		if (num == DevconReturnCode.Success)
		{
			DetectedDriver = GetInstalledTapDriver();
			OnDriverUpdated?.Invoke(DetectedDriver);
		}
		return num;
	}

	public async Task<DevconReturnCode> Remove()
	{
		DevconReturnCode num = await PerformOperation("remove", DetectedDriver.ToString()).ConfigureAwait(continueOnCapturedContext: false);
		if (num == DevconReturnCode.Success)
		{
			DetectedDriver = GetInstalledTapDriver();
			OnDriverUpdated?.Invoke(DetectedDriver);
		}
		return num;
	}

	private TapDriver GetInstalledTapDriver()
	{
		TapDriver tapDriver = TapDriver.NotInstalled;
		if (IsTapAdapterInstalled(TapDriver.tapwlvpn))
		{
			tapDriver = TapDriver.tapwlvpn;
		}
		if (_preferredTapAdapter == OpenVpnTapAdapter.NotSet && IsTapAdapterInstalled(TapDriver.tap0901))
		{
			tapDriver = TapDriver.tap0901;
		}
		if (tapDriver != DetectedDriver)
		{
			OnDriverUpdated?.Invoke(tapDriver);
		}
		return tapDriver;
	}

	private bool IsTapAdapterInstalled(TapDriver tapDriver)
	{
		bool num = NetworkInterfaceHelper.GetNetworkInterface(tapDriver.ToString()) != null;
		if (!num)
		{
			NetworkInterface[] allNetworkInterfacesSafely = NetworkInterfaceHelper.GetAllNetworkInterfacesSafely();
			string text = string.Join(",", allNetworkInterfacesSafely.Select((NetworkInterface o) => o.Description).ToArray());
			ILogger devconLog = _devconLog;
			if (devconLog == null)
			{
				return num;
			}
			devconLog.LogInformation("Current tapdriver: " + tapDriver.GetDescription() + " and available network adapters: " + text);
		}
		return num;
	}

	private Task<DevconReturnCode> PerformOperation(string operation, params string[] arguments)
	{
		TaskCompletionSource<DevconReturnCode> processTaskCompletionSource = new TaskCompletionSource<DevconReturnCode>(TaskCreationOptions.RunContinuationsAsynchronously);
		Process tapProcess = new Process
		{
			StartInfo = 
			{
				Arguments = operation + " " + string.Join(" ", arguments),
				CreateNoWindow = true,
				FileName = _devconPath,
				WorkingDirectory = Path.GetDirectoryName(_devconPath),
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			},
			EnableRaisingEvents = true
		};
		tapProcess.Exited += delegate
		{
			processTaskCompletionSource.SetResult((DevconReturnCode)tapProcess.ExitCode);
		};
		tapProcess.OutputDataReceived += delegate(object sender, DataReceivedEventArgs evt)
		{
			if (!string.IsNullOrEmpty(evt.Data))
			{
				_devconLog?.LogTrace(evt.Data);
			}
		};
		tapProcess.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs evt)
		{
			if (!string.IsNullOrEmpty(evt.Data))
			{
				_devconLog?.LogTrace(evt.Data);
			}
		};
		tapProcess.Start();
		tapProcess.BeginOutputReadLine();
		tapProcess.BeginErrorReadLine();
		return processTaskCompletionSource.Task;
	}
}
