using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NextAiVPN.Common;
using NextAiVPN.Services.Persistence;
using VpnSDK.Interfaces;

namespace NextAiVPN.Services;

internal class FavoritesService : IFavoritesService
{
	private const string FavoritesListStr = "FavoritesList";

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IPreferencesRepository _preferencesRepository;

	private readonly IBugsnagService _bugsnagService;

	private readonly IAppLogger _logger;

	private readonly INotificationPresenter _notificationPresenter;

	public FavoritesService(IAppSettingsHelper appSettingsHelper, IPreferencesRepository preferencesRepository, IBugsnagService bugsnagService, IAppLogger logger, INotificationPresenter notificationPresenter)
	{
		_appSettingsHelper = appSettingsHelper ?? throw new ArgumentNullException("appSettingsHelper");
		_preferencesRepository = preferencesRepository ?? throw new ArgumentNullException("preferencesRepository");
		_bugsnagService = bugsnagService ?? throw new ArgumentNullException("bugsnagService");
		_logger = logger;
		_notificationPresenter = notificationPresenter ?? throw new ArgumentNullException("notificationPresenter");
	}

	/// <summary>
	/// [VI] Thêm vào hoặc xóa vị trí yêu thích khỏi danh sách (tự động đảo trạng thái toggle)
	/// [EN] Add or remove favorite location from list (auto toggle state)
	/// </summary>
	public void SaveFavorites(string location)
	{
		if (string.IsNullOrWhiteSpace(location)) return;
		string value = string.Empty;
		try
		{
			value = _appSettingsHelper.GetValue("FavoritesList") ?? string.Empty;
			string[] array = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			List<string> list = new List<string>();

			bool found = false;
			foreach (string item in array)
			{
				if (string.IsNullOrWhiteSpace(item)) continue;
				if (string.Equals(item, location, StringComparison.OrdinalIgnoreCase))
				{
					found = true;
				}
				else
				{
					list.Add(item);
				}
			}

			if (!found)
			{
				list.Add(location);
			}

			string newFavorites = list.Count > 0 ? string.Join(";", list) + ";" : string.Empty;
			_appSettingsHelper.SetValue("FavoritesList", newFavorites);
			_preferencesRepository.SaveSinglePreference("FavoritesList".ToLower(), newFavorites);
		}
		catch (Exception ex)
		{
			_appSettingsHelper.SetValue("FavoritesList", value);
			_notificationPresenter.ShowError(string.Format(CultureInfo.InvariantCulture, "Unable to save favorite location : {0}", ex.Message));
			_bugsnagService.Notify(ex);
		}
	}

	/// <summary>
	/// [VI] Kiểm tra xem vị trí hoặc quốc gia đã được lưu trong danh sách yêu thích chưa (không phân biệt chữ hoa/thường)
	/// [EN] Check if location or country is saved in favorites list (case-insensitive)
	/// </summary>
	public bool ContainFavorite(string location)
	{
		if (string.IsNullOrWhiteSpace(location)) return false;
		try
		{
			string favStr = _appSettingsHelper.GetValue("FavoritesList");
			if (string.IsNullOrEmpty(favStr)) return false;

			string[] array = favStr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string f in array)
			{
				if (string.Equals(f, location, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				if (f.Contains("_"))
				{
					string[] parts = f.Split('_');
					if (parts.Length >= 2)
					{
						if (string.Equals(parts[0], location, StringComparison.OrdinalIgnoreCase) ||
						    string.Equals(parts[1], location, StringComparison.OrdinalIgnoreCase))
						{
							return true;
						}
					}
				}
			}
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "ContainFavorite", "FavoritesService.cs", 95);
		}
		return false;
	}

	/// <summary>
	/// [VI] Kiểm tra xem quốc gia có bất kỳ thành phố nào nằm trong danh sách yêu thích không
	/// [EN] Check if country has any city in favorites list
	/// </summary>
	public bool HasFavorite(string location, SDKMonitor sdkObject)
	{
		if (string.IsNullOrEmpty(location)) return false;
		try
		{
			string favStr = _appSettingsHelper.GetValue("FavoritesList");
			if (string.IsNullOrEmpty(favStr)) return false;

			string[] array = favStr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length == 0) return false;

			if (sdkObject?.NextAiVpnSdkManager?.Locations != null)
			{
				IEnumerable<ILocation> enumerable = sdkObject.NextAiVpnSdkManager.Locations.Where((ILocation x) => string.Equals(x.CountryCode, location, StringComparison.OrdinalIgnoreCase));
				foreach (ILocation loc in enumerable)
				{
					foreach (string f in array)
					{
						if (string.Equals(f, loc.Id, StringComparison.OrdinalIgnoreCase) ||
						    string.Equals(f, loc.CountryCode + "_" + loc.Id, StringComparison.OrdinalIgnoreCase) ||
						    f.StartsWith(loc.CountryCode + "_", StringComparison.OrdinalIgnoreCase))
						{
							return true;
						}
					}
				}
			}
			else
			{
				return array.Any(f => f.StartsWith(location + "_", StringComparison.OrdinalIgnoreCase) || string.Equals(f, location, StringComparison.OrdinalIgnoreCase));
			}
		}
		catch (Exception ex)
		{
			_logger?.Error(ex.Message, "HasFavorite", "FavoritesService.cs", 130);
		}
		return false;
	}
}
