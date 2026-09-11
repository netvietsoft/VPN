using Newtonsoft.Json;

namespace NextAiVPN.Entities;

public class Product
{
	[JsonProperty("id")]
	public int Id { get; set; }

	[JsonProperty("slug")]
	public string Slug { get; set; }
}
