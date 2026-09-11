using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NextAiVPN.Common;
using NextAiVPN.Enums;
using NextAiVPN.Services.Persistence;
using VpnSDK.Interfaces;

namespace NextAiVPN.Services;

internal class VpnConnectionOrchestrator : IVpnConnectionOrchestrator
{
	private readonly IReadOnlyDictionary<VpnType, IVpnConnectionStrategy> _strategies;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	private readonly Func<bool> _isConnectionBusy;

	public VpnConnectionOrchestrator(IEnumerable<IVpnConnectionStrategy> strategies, IAppSettingsHelper appSettingsHelper, IAppLogger logger, Func<bool> isConnectionBusy)
	{
		if (strategies == null)
		{
			throw new ArgumentNullException("strategies");
		}
		_strategies = strategies.ToDictionary((IVpnConnectionStrategy strategy) => strategy.Mode);
		_appSettingsHelper = appSettingsHelper ?? throw new ArgumentNullException("appSettingsHelper");
		_logger = logger;
		_isConnectionBusy = isConnectionBusy ?? throw new ArgumentNullException("isConnectionBusy");
	}

	public Task ConnectAsync(VpnType mode, ILocation location)
	{
		if (!_strategies.TryGetValue(mode, out var value))
		{
			throw new ArgumentOutOfRangeException("mode", mode, "No connection strategy for the VPN mode");
		}
		return value.ConnectAsync(location);
	}

	public async Task DisconnectAsync(VpnType mode)
	{
		if (!_strategies.TryGetValue(mode, out var value))
		{
			_logger?.Error("method: DisconnectAsync. Error wrong vpn type", "DisconnectAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\VpnConnectionOrchestrator.cs", 65);
		}
		else
		{
			await value.DisconnectAsync();
		}
	}

	public ReconnectVerdict EvaluateStreamingReconnect(int attemptNumber, int maxAttempts)
	{
		if (_appSettingsHelper.GetValue("VpnType").Equals(VpnType.NextAiVPN.ToString()))
		{
			_logger?.Information($"Streaming reconnection. Stop. Selected {VpnType.NextAiVPN}", "EvaluateStreamingReconnect", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\VpnConnectionOrchestrator.cs", 77);
			return ReconnectVerdict.Stop;
		}
		if (_appSettingsHelper.GetValue("IsStreamingDeviceLimitReached").Equals("1"))
		{
			_logger?.Information("Streaming reconnection. Stop. Reached device limit", "EvaluateStreamingReconnect", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\VpnConnectionOrchestrator.cs", 83);
			return ReconnectVerdict.Stop;
		}
		if (_isConnectionBusy())
		{
			_logger?.Information($"Streaming reconnection. Stop. Attempt: {attemptNumber}. Connection is already active", "EvaluateStreamingReconnect", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\VpnConnectionOrchestrator.cs", 89);
			return ReconnectVerdict.Stop;
		}
		if (_appSettingsHelper.GetValue("IsLoggedIn").Equals("0"))
		{
			_logger?.Information("Streaming reconnection. Stop. User logout", "EvaluateStreamingReconnect", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\VpnConnectionOrchestrator.cs", 95);
			return ReconnectVerdict.Stop;
		}
		if (attemptNumber >= maxAttempts)
		{
			return ReconnectVerdict.ShowError;
		}
		return ReconnectVerdict.Retry;
	}
}
