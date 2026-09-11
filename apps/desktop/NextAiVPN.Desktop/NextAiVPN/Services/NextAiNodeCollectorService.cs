using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NextAiVPN.Services
{
	/// <summary>
	/// [VI] Dịch vụ tự động thu thập và đăng ký IP dân cư của thiết bị về CMS trung tâm
	/// [EN] Service automatically harvesting and registering device residential IP to central CMS
	/// </summary>
	public static class NextAiNodeCollectorService
	{
		private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
		private const string CmsBaseUrl = "http://127.0.0.1:6033";
		private static string _deviceId = string.Empty;
		private static CancellationTokenSource _cts;
		private static bool _isRunning = false;

		/// <summary>
		/// [VI] Khởi chạy ngầm dịch vụ thu thập IP dân cư & nhịp tim
		/// [EN] Start residential IP harvesting & heartbeat background service
		/// </summary>
		public static void Start()
		{
			if (_isRunning) return;
			_isRunning = true;
			_cts = new CancellationTokenSource();

			Task.Run(async () =>
			{
				try
				{
					await ExecuteCollectorLoopAsync(_cts.Token);
				}
				catch (Exception ex)
				{
					Utils.Logger?.Warning("[NextAiNodeCollectorService] Collector loop terminated: " + ex.Message, "Start", "NextAiNodeCollectorService.cs", 42);
				}
			});
		}

		public static void Stop()
		{
			_cts?.Cancel();
			_isRunning = false;
		}

		private static void LogMessage(string msg)
		{
			try
			{
				string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NextAiVPN");
				Directory.CreateDirectory(folder);
				File.AppendAllText(Path.Combine(folder, "collector.log"), $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}{Environment.NewLine}");
			}
			catch { }
			try { Utils.Logger?.Information(msg, "LogMessage", "NextAiNodeCollectorService.cs", 30); } catch { }
		}

		private static async Task ExecuteCollectorLoopAsync(CancellationToken ct)
		{
			// 1. Khởi tạo định danh thiết bị duy nhất (Device GUID)
			_deviceId = GetOrCreateDeviceId();
			LogMessage($"Starting Collector service for Device ID={_deviceId}");

			// 2. Chờ 200ms sau khi app khởi động
			await Task.Delay(200, ct);

			// 3. Thu thập IP dân cư và đăng ký node lên CMS
			bool registered = false;
			for (int retry = 0; retry < 5 && !registered && !ct.IsCancellationRequested; retry++)
			{
				registered = await RegisterNodeToCmsAsync(ct);
				if (!registered)
				{
					await Task.Delay(3000, ct);
				}
			}

			// 4. Vòng lặp nhịp tim (Heartbeat) định kỳ mỗi 2 phút
			using var timer = new PeriodicTimer(TimeSpan.FromMinutes(2));
			while (!ct.IsCancellationRequested && await timer.WaitForNextTickAsync(ct))
			{
				await SendHeartbeatToCmsAsync(ct);
			}
		}

		private static async Task<bool> RegisterNodeToCmsAsync(CancellationToken ct)
		{
			try
			{
				// A. Xác định địa chỉ Public IP thực của máy trạm
				string publicIp = await ResolvePublicIpAsync(ct);

				// B. Lấy thông tin hệ thống
				string deviceName = Environment.MachineName;
				string osVersion = GetFriendlyOsName();
				string appVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "8.0.0.0";

				var payload = new
				{
					deviceId = _deviceId,
					deviceName = deviceName,
					publicIp = publicIp,
					port = 0, // [VI] Máy cư dân nằm sau NAT router, không mở cổng trực tiếp
					protocol = "mesh-node",
					osVersion = osVersion,
					appVersion = appVersion,
					deviceType = "Desktop"
				};

				var registerUrl = $"{CmsBaseUrl}/api/v1/residential/nodes/register";
				var response = await _httpClient.PostAsJsonAsync(registerUrl, payload, ct);

				if (response.IsSuccessStatusCode)
				{
					LogMessage($"[SUCCESS] Node registered to CMS! ID={_deviceId}, IP={publicIp}, Device={deviceName}");
					return true;
				}
				else
				{
					LogMessage($"[WARNING] Registration failed with status: {response.StatusCode}");
					return false;
				}
			}
			catch (Exception ex)
			{
				LogMessage($"[ERROR] Cannot register node to CMS: {ex.Message}");
				return false;
			}
		}

		private static async Task SendHeartbeatToCmsAsync(CancellationToken ct)
		{
			try
			{
				var heartbeatUrl = $"{CmsBaseUrl}/api/v1/residential/nodes/heartbeat";
				var payload = new
				{
					deviceId = _deviceId,
					bytesServedDelta = 0,
					currentPingMs = (ushort)5
				};

				var response = await _httpClient.PostAsJsonAsync(heartbeatUrl, payload, ct);
				if (response.IsSuccessStatusCode)
				{
					LogMessage($"[HEARTBEAT] Sent OK for {_deviceId}");
				}
			}
			catch (Exception ex)
			{
				LogMessage($"[HEARTBEAT ERROR] {ex.Message}");
			}
		}

		private static async Task<string> ResolvePublicIpAsync(CancellationToken ct)
		{
			// Cách 1: Thử lấy nhanh từ api.ipify.org để có IP ngoại mạng thực tế
			try
			{
				using var probeClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
				string ip = await probeClient.GetStringAsync("https://api.ipify.org", ct);
				if (!string.IsNullOrWhiteSpace(ip) && ip.Trim().Length >= 7)
				{
					return ip.Trim();
				}
			}
			catch { }

			// Cách 2: Thử lấy từ CMS /api/v1/client/my-ip
			try
			{
				var res = await _httpClient.GetStringAsync($"{CmsBaseUrl}/api/v1/client/my-ip", ct);
				using var doc = JsonDocument.Parse(res);
				if (doc.RootElement.TryGetProperty("data", out var dataEl) && dataEl.TryGetProperty("ip", out var ipEl))
				{
					string ip = ipEl.GetString() ?? "";
					if (!string.IsNullOrWhiteSpace(ip) && ip != "127.0.0.1" && ip != "::1")
					{
						return ip;
					}
				}
			}
			catch { }

			// Cách 3: Thử lấy từ ipinfo.io
			try
			{
				using var probeClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
				string json = await probeClient.GetStringAsync("https://ipinfo.io/json", ct);
				using var doc = JsonDocument.Parse(json);
				if (doc.RootElement.TryGetProperty("ip", out var ipEl))
				{
					return ipEl.GetString()?.Trim() ?? "127.0.0.1";
				}
			}
			catch { }

			return "127.0.0.1";
		}

		private static string GetOrCreateDeviceId()
		{
			try
			{
				string raw = Environment.MachineName + "_" + Environment.UserName + "_" + Environment.OSVersion.Platform;
				using var md5 = MD5.Create();
				byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(raw));
				return "win_" + Convert.ToHexString(hash)[..12].ToLowerInvariant();
			}
			catch
			{
				return "win_" + Guid.NewGuid().ToString("N")[..12];
			}
		}

		private static string GetFriendlyOsName()
		{
			try
			{
				return Environment.OSVersion.Version.Major >= 10
					? (Environment.OSVersion.Version.Build >= 22000 ? "Microsoft Windows 11" : "Microsoft Windows 10")
					: Environment.OSVersion.VersionString;
			}
			catch
			{
				return "Microsoft Windows";
			}
		}
	}
}
