using Newtonsoft.Json;

namespace NextAiVPN.Entities;

internal class DiagnosticsFileUploadResponse
{
	[JsonProperty("fileId")]
	public string FileId { get; set; }

	[JsonProperty("success")]
	public int Success { get; set; }
}
