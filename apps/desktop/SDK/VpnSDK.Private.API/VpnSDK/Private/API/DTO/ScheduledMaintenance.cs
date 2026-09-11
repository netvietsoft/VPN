using System;
using Newtonsoft.Json;

namespace VpnSDK.Private.API.DTO;

public class ScheduledMaintenance
{
	[JsonProperty("starts_at")]
	public long StartsAt { get; set; }

	[JsonProperty("ends_at")]
	public long EndsAt { get; set; }

	[JsonProperty("warning_window_min")]
	public int WarningWindowMin { get; set; }

	public DateTime StartTime => DateTimeOffset.FromUnixTimeSeconds(StartsAt).UtcDateTime;

	public DateTime EndTime => DateTimeOffset.FromUnixTimeSeconds(EndsAt).UtcDateTime;
}
