using Newtonsoft.Json;

namespace NextAiVPN.Entities;

public class CurrentPeriodPrice
{
	[JsonProperty("amount")]
	public string Amount { get; set; }

	[JsonProperty("currency")]
	public string Currency { get; set; }
}
