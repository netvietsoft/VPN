using System.Collections.Generic;
using System.Linq;
using NextAiVPN.Common;
using VpnSDK.Interfaces;

namespace NextAiVPN.Services;

internal class FavoriteLocationsServices : IFavoriteLocationsServices
{
	private readonly SDKMonitor _skjObject;

	private readonly IAppSettingsHelper _appSettingsHelper;

	public FavoriteLocationsServices(SDKMonitor skjObject, IAppSettingsHelper appSettingsHelper)
	{
		_skjObject = skjObject;
		_appSettingsHelper = appSettingsHelper;
	}

	/// <summary>
	/// [VI] Lấy danh sách các ILocation yêu thích thực tế từ cấu hình
	/// [EN] Retrieve actual favorite ILocation items from configuration
	/// </summary>
	public List<ILocation> GetFavoriteLocations()
	{
		List<string> favoriteListFromConfig = GetFavoriteListFromConfig();
		List<ILocation> list = new List<ILocation>();
		if (_skjObject?.NextAiVpnSdkManager?.Locations == null) return list;

		var allLocations = _skjObject.NextAiVpnSdkManager.Locations;
		foreach (string item in favoriteListFromConfig)
		{
			if (string.IsNullOrWhiteSpace(item)) continue;
			string lookupId = item;
			if (item.Contains('_'))
			{
				lookupId = item.Substring(item.IndexOf('_') + 1);
			}

			foreach (ILocation item2 in allLocations)
			{
				if (string.Equals(item2.Id, lookupId, System.StringComparison.OrdinalIgnoreCase) ||
				    string.Equals(item2.Id, item, System.StringComparison.OrdinalIgnoreCase) ||
				    string.Equals(item2.CountryCode + "_" + item2.Id, item, System.StringComparison.OrdinalIgnoreCase))
				{
					if (!list.Any(x => x.Id == item2.Id))
					{
						list.Add(item2);
					}
				}
			}
		}
		return list.OrderBy((ILocation x) => x.Country).ThenBy((ILocation x) => x.City).ToList();
	}

	/// <summary>
	/// [VI] Lấy danh sách mã quốc gia yêu thích từ cấu hình (tối đa 5 mục)
	/// [EN] Get favorite country codes from config (max 5 items)
	/// </summary>
	public List<string> GetFavoriteListFromConfigOnlyCountries()
	{
		List<string> list = new List<string>();
		string value = _appSettingsHelper.GetValue("FavoritesList");
		if (!string.IsNullOrEmpty(value))
		{
			string[] favs = value.Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
			List<string> list2 = CheckDuplicates(favs);
			for (int i = 0; i < list2.Count; i++)
			{
				if (i < 5 && !string.IsNullOrEmpty(list2[i]))
				{
					list.Add(list2[i]);
				}
			}
		}
		return list;
	}

	/// <summary>
	/// [VI] Lấy danh sách chuỗi vị trí yêu thích từ cấu hình
	/// [EN] Get raw favorite location strings from config
	/// </summary>
	public List<string> GetFavoriteListFromConfig()
	{
		List<string> result = new List<string>();
		string value = _appSettingsHelper.GetValue("FavoritesList");
		if (!string.IsNullOrEmpty(value))
		{
			result = value.Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
		}
		return result;
	}

	private List<string> CheckDuplicates(IReadOnlyList<string> favs)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		if (_skjObject?.NextAiVpnSdkManager?.Locations == null) return list2;

		var allLocations = _skjObject.NextAiVpnSdkManager.Locations;
		for (int i = 0; i < favs.Count; i++)
		{
			string fav = favs[i];
			if (string.IsNullOrWhiteSpace(fav)) continue;
			string lookupId = fav;
			if (fav.Contains('_'))
			{
				lookupId = fav.Substring(fav.IndexOf('_') + 1);
			}

			foreach (ILocation item in allLocations)
			{
				if (string.Equals(item.Id, lookupId, System.StringComparison.OrdinalIgnoreCase) ||
				    string.Equals(item.Id, fav, System.StringComparison.OrdinalIgnoreCase))
				{
					if (!list.Contains(item.CountryCode))
					{
						list.Add(item.CountryCode);
						list2.Add(item.Id);
					}
				}
			}
		}
		return list2;
	}
}
