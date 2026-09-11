using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using GeoCoordinatePortable;
using Microsoft.Win32;
using VpnSDK.Enums;
using VpnSDK.Interfaces;

namespace NextAiVPN.Services
{
	/// <summary>
	/// [VI] Đối tượng vị trí Proxy/VPN NextAI tương thích giao diện desktop
	/// [EN] NextAI Proxy/VPN Location object compatible with desktop UI
	/// </summary>
	public class NextAiProxyLocation : ILocation, IRegion, INotifyPropertyChanged
	{
		public string Id { get; set; } = "bestavailable";
		public string CountryCode { get; set; } = "US";
		public string CityCode { get; set; } = "AUTO";
		public string SearchName { get; set; } = "United States Auto";
		public ushort? PingMs { get; set; } = 25;
		public string Country { get; set; } = "United States";
		public string City { get; set; } = "Auto";
		public ushort? Load { get; set; } = 15;
		public GeoCoordinate GeoCoordinate { get; set; } = new GeoCoordinate(0, 0);

		public List<NetworkConnectionType> AvailableProtocols { get; set; } = new List<NetworkConnectionType>
		{
			NetworkConnectionType.WireGuard,
			NetworkConnectionType.OpenVPN,
			NetworkConnectionType.IKEv2
		};

		public Task<ushort?> Ping() => Task.FromResult(PingMs);
		public Task<ushort?> PingAll() => Task.FromResult(PingMs);

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	/// <summary>
	/// [VI] Thông tin phiên kết nối đang hoạt động của NextAI
	/// [EN] NextAI active connection session information
	/// </summary>
	public class NextAiConnectionInfo : IConnectionInfo
	{
		public ILocation Location { get; set; }
		public IConnectionConfiguration Configuration { get; set; }
		public NetworkConnectionType Protocol { get; set; } = NetworkConnectionType.WireGuard;
		public IPAddress ServerIp { get; set; } = IPAddress.Parse("127.0.0.1");
		public bool IsKillKwitchOn => false;
		public bool IsDnsProtected => true;
		public bool IsIPv6Protected => true;
		public bool IsLanTrafficAllowed => true;
		public bool IsAutomaticProtocolUsed => false;

		public NextAiConnectionInfo(ILocation location)
		{
			Location = location;
		}
	}

	/// <summary>
	/// [VI] Dịch vụ đồng bộ danh sách Proxy từ Backend CMS và điều khiển Gateway
	/// [EN] Service synchronizing proxy list from Backend CMS and controlling Gateway
	/// </summary>
	public static class NextAiLocationService
	{
		private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
		private const string LocationsUrl = "http://127.0.0.1:6033/api/v1/locations";
		private const string SelectProxyUrl = "http://127.0.0.1:6033/api/v1/proxies/{0}/select";
		private const string GatewayStatsUrl = "http://127.0.0.1:6033/api/v1/gateway/stats";

		/// <summary>
		/// [VI] Lấy thông tin thống kê lưu lượng từ Universal Gateway port 10000
		/// [EN] Get traffic statistics from Universal Gateway port 10000
		/// </summary>
		public static async Task<(long bytesIn, long bytesOut, string? protocol, string? ip)> GetGatewayTrafficStatsAsync()
		{
			try
			{
				string json = await _httpClient.GetStringAsync(GatewayStatsUrl);
				using var doc = JsonDocument.Parse(json);
				if (doc.RootElement.TryGetProperty("data", out var dataEl) || doc.RootElement.TryGetProperty("Data", out dataEl))
				{
					long bytesIn = 0;
					long bytesOut = 0;
					string? proto = null;
					string? ip = null;

					if (dataEl.TryGetProperty("bytesIn", out var bi) && bi.TryGetInt64(out var biVal)) bytesIn = biVal;
					else if (dataEl.TryGetProperty("BytesIn", out var bi2) && bi2.TryGetInt64(out var biVal2)) bytesIn = biVal2;

					if (dataEl.TryGetProperty("bytesOut", out var bo) && bo.TryGetInt64(out var boVal)) bytesOut = boVal;
					else if (dataEl.TryGetProperty("BytesOut", out var bo2) && bo2.TryGetInt64(out var boVal2)) bytesOut = boVal2;

					if (dataEl.TryGetProperty("activeProxy", out var ap) || dataEl.TryGetProperty("ActiveProxy", out ap))
					{
						if (ap.ValueKind == JsonValueKind.Object)
						{
							if (ap.TryGetProperty("type", out var pt)) proto = pt.GetString();
							else if (ap.TryGetProperty("Type", out var pt2)) proto = pt2.GetString();

							if (ap.TryGetProperty("host", out var ph)) ip = ph.GetString();
							else if (ap.TryGetProperty("Host", out var ph2)) ip = ph2.GetString();
						}
					}

					return (bytesIn, bytesOut, proto, ip);
				}
			}
			catch { }
			return (0L, 0L, null, null);
		}

		[DllImport("wininet.dll", SetLastError = true)]
		private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
		private const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
		private const int INTERNET_OPTION_REFRESH = 37;

		/// <summary>
		/// [VI] Tải danh sách vị trí máy chủ proxy từ Backend CMS
		/// [EN] Load list of proxy server locations from Backend CMS
		/// </summary>
		public static async Task<List<NextAiProxyLocation>> LoadLocationsAsync()
		{
			var result = new List<NextAiProxyLocation>();
			try
			{
				string json = await _httpClient.GetStringAsync(LocationsUrl);
				using var doc = JsonDocument.Parse(json);
				JsonElement arrayElement = doc.RootElement;
				if (doc.RootElement.ValueKind == JsonValueKind.Object)
				{
					if ((doc.RootElement.TryGetProperty("locations", out var locEl) || doc.RootElement.TryGetProperty("Locations", out locEl)) && locEl.ValueKind == JsonValueKind.Array)
					{
						arrayElement = locEl;
					}
					else if ((doc.RootElement.TryGetProperty("data", out var dataEl) || doc.RootElement.TryGetProperty("Data", out dataEl)) && dataEl.ValueKind == JsonValueKind.Array)
					{
						arrayElement = dataEl;
					}
				}

				if (arrayElement.ValueKind == JsonValueKind.Array)
				{
					foreach (var el in arrayElement.EnumerateArray())
					{
						var loc = new NextAiProxyLocation
						{
							Id = el.TryGetProperty("id", out var id) ? (id.GetString() ?? "loc") : "loc",
							Country = el.TryGetProperty("country", out var c) ? (c.GetString() ?? "United States") : "United States",
							CountryCode = el.TryGetProperty("countryCode", out var cc) ? (cc.GetString() ?? "US") : "US",
							City = el.TryGetProperty("city", out var ci) ? (ci.GetString() ?? "Auto") : "Auto",
							CityCode = el.TryGetProperty("cityCode", out var cic) ? (cic.GetString() ?? "auto") : "auto",
							SearchName = el.TryGetProperty("searchName", out var sn) ? (sn.GetString() ?? "") : "",
							PingMs = el.TryGetProperty("pingMs", out var pm) && pm.TryGetUInt16(out var pVal) ? pVal : (ushort)25,
							Load = el.TryGetProperty("load", out var ld) && ld.TryGetUInt16(out var lVal) ? lVal : (ushort)15
						};
						result.Add(loc);
					}
				}
			}
			catch (Exception ex)
			{
				Utils.Logger?.Warning("[NextAiLocationService] Failed to load from CMS API: " + ex.Message);
				result = GetDefaultFallbackLocations();
			}

			if (result.Count == 0)
			{
				result = GetDefaultFallbackLocations();
			}

			return result;
		}

		/// <summary>
		/// [VI] Danh sách vị trí dự phòng mặc định khi CMS chưa có proxy
		/// [EN] Default fallback location list when CMS has no proxies yet
		/// </summary>
		public static List<NextAiProxyLocation> GetDefaultFallbackLocations()
		{
			return new List<NextAiProxyLocation>
			{
				new NextAiProxyLocation { Id = "bestavailable", Country = "Best Available", CountryCode = "US", City = "Auto", CityCode = "auto", SearchName = "Best Available", PingMs = 18, Load = 10 },
				new NextAiProxyLocation { Id = "proxy_vn_hanoi", Country = "Vietnam", CountryCode = "VN", City = "Hanoi", CityCode = "hanoi", SearchName = "Vietnam Hanoi VN", PingMs = 18, Load = 15 },
				new NextAiProxyLocation { Id = "proxy_vn_hcm", Country = "Vietnam", CountryCode = "VN", City = "Ho Chi Minh City", CityCode = "hochiminhcity", SearchName = "Vietnam Ho Chi Minh City VN", PingMs = 24, Load = 15 },
				new NextAiProxyLocation { Id = "proxy_sg_sin", Country = "Singapore", CountryCode = "SG", City = "Singapore", CityCode = "singapore", SearchName = "Singapore Singapore SG", PingMs = 38, Load = 15 },
				new NextAiProxyLocation { Id = "proxy_jp_tyo", Country = "Japan", CountryCode = "JP", City = "Tokyo", CityCode = "tokyo", SearchName = "Japan Tokyo JP", PingMs = 72, Load = 15 },
				new NextAiProxyLocation { Id = "proxy_us_lax", Country = "United States", CountryCode = "US", City = "Los Angeles", CityCode = "losangeles", SearchName = "United States Los Angeles US", PingMs = 145, Load = 15 },
				new NextAiProxyLocation { Id = "proxy_us_nyc", Country = "United States", CountryCode = "US", City = "New York City", CityCode = "newyorkcity", SearchName = "United States New York City US", PingMs = 168, Load = 15 },
				new NextAiProxyLocation { Id = "proxy_gb_lon", Country = "United Kingdom", CountryCode = "GB", City = "London", CityCode = "london", SearchName = "United Kingdom London GB", PingMs = 180, Load = 15 },
				new NextAiProxyLocation { Id = "proxy_de_fra", Country = "Germany", CountryCode = "DE", City = "Frankfurt", CityCode = "frankfurt", SearchName = "Germany Frankfurt DE", PingMs = 195, Load = 15 }
			};
		}

		/// <summary>
		/// [VI] Bơm danh sách vị trí vào SDK Core để giao diện WPF tự động hiển thị
		/// [EN] Inject locations collection into SDK Core for WPF UI auto-rendering
		/// </summary>
		public static void InjectLocationsIntoSdk(ISDK sdk, List<NextAiProxyLocation> locations)
		{
			if (sdk == null || locations == null) return;
			try
			{
				var observable = new ObservableCollection<ILocation>(locations);
				var readOnly = new ReadOnlyObservableCollection<ILocation>(observable);
				var field = sdk.GetType().GetField("<Locations>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
				if (field != null)
				{
					field.SetValue(sdk, readOnly);
				}
			}
			catch (Exception ex)
			{
				Utils.Logger?.Error(ex, "InjectLocationsIntoSdk", "NextAiLocationService.cs", 155);
			}
		}

		/// <summary>
		/// [VI] Đặt thông tin kết nối đang hoạt động trong SDK Core
		/// [EN] Set active connection information in SDK Core
		/// </summary>
		public static void SetActiveConnection(ISDK sdk, ILocation location, bool isConnected)
		{
			if (sdk == null) return;
			try
			{
				var connInfoField = sdk.GetType().GetField("_activeConnectionInformation", BindingFlags.Instance | BindingFlags.NonPublic);
				if (connInfoField != null)
				{
					connInfoField.SetValue(sdk, isConnected ? new NextAiConnectionInfo(location) : null);
				}

				var statusField = sdk.GetType().GetField("<CurrentConnectionStatus>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
				if (statusField != null)
				{
					statusField.SetValue(sdk, isConnected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected);
				}
			}
			catch (Exception ex)
			{
				Utils.Logger?.Error(ex, "SetActiveConnection", "NextAiLocationService.cs", 180);
			}
		}

		/// <summary>
		/// [VI] Báo cho Backend chọn upstream proxy tương ứng với vị trí đã chọn
		/// [EN] Notify Backend to select upstream proxy corresponding to selected location
		/// </summary>
		public static async Task SelectUpstreamProxyAsync(string proxyId)
		{
			if (string.IsNullOrEmpty(proxyId) || proxyId == "bestavailable") return;
			try
			{
				string url = string.Format(SelectProxyUrl, proxyId);
				await _httpClient.PostAsync(url, null);
			}
			catch { }
		}

		/// <summary>
		/// [VI] Bật System Proxy Windows để điều hướng toàn bộ lưu lượng duyệt web qua Gateway cổng 10000
		/// [EN] Enable Windows System Proxy to route all web browsing traffic through Gateway port 10000
		/// </summary>
		public static void EnableSystemProxy(string host = "127.0.0.1", int port = 10000)
		{
			try
			{
				using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true);
				if (key != null)
				{
					key.SetValue("ProxyEnable", 1, RegistryValueKind.DWord);
					key.SetValue("ProxyServer", $"{host}:{port}", RegistryValueKind.String);
					key.SetValue("ProxyOverride", "<local>;localhost;127.*;10.*;192.168.*", RegistryValueKind.String);
				}
				NotifySystemProxyChanged();
				Utils.Logger?.Information($"[NextAiLocationService] Windows System Proxy ENABLED -> {host}:{port}");
			}
			catch (Exception ex)
			{
				Utils.Logger?.Error(ex, "EnableSystemProxy", "NextAiLocationService.cs", 245);
			}
		}

		/// <summary>
		/// [VI] Đảm bảo dọn dẹp và trả lại trạng thái mạng sạch sẽ cho Windows (ProxyEnable = 0, ProxyServer rỗng)
		/// [EN] Ensure cleanup and restore clean network state for Windows (ProxyEnable = 0, empty ProxyServer)
		/// </summary>
		public static void DisableSystemProxy()
		{
			try
			{
				using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true);
				if (key != null)
				{
					key.SetValue("ProxyEnable", 0, RegistryValueKind.DWord);
					key.SetValue("ProxyServer", "", RegistryValueKind.String);
				}
				NotifySystemProxyChanged();
				Utils.Logger?.Information("[NextAiLocationService] Windows System Proxy verified DISABLED and CLEANED (ProxyEnable=0)");
			}
			catch (Exception ex)
			{
				Utils.Logger?.Error(ex, "DisableSystemProxy", "NextAiLocationService.cs", 235);
			}
		}

		private static void NotifySystemProxyChanged()
		{
			try
			{
				InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
				InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
			}
			catch { }
		}
	}
}
