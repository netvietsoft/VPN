using System;
using System.Collections.Generic;
using VpnSDK.Interfaces;
using VpnSDK.Internal.Helpers;
using VpnSDK.Private.API.DTO;

namespace VpnSDK.Internal.Dtos;

internal class BrandingInfoProxy : BindableBase, IBrandingInfo
{
	private BrandingInfo _brandingInfo;

	public string AppName => _brandingInfo?.AppName;

	public DateTime? LastUpdatedAt => _brandingInfo?.LastUpdatedAt;

	public Dictionary<string, string> Urls => _brandingInfo?.Urls;

	public Dictionary<string, string> Colors => _brandingInfo?.Colors;

	public BrandingInfoProxy(BrandingInfo brandingInfo)
	{
		_brandingInfo = brandingInfo;
	}

	public void Update(BrandingInfo brandingInfo)
	{
		bool flag = false;
		if (LastUpdatedAt.HasValue && brandingInfo != null)
		{
			_ = brandingInfo.LastUpdatedAt;
			if (!LastUpdatedAt.Equals(brandingInfo.LastUpdatedAt))
			{
				flag = true;
			}
		}
		_brandingInfo = brandingInfo;
		if (flag)
		{
			OnPropertyChanged("AppName");
			OnPropertyChanged("LastUpdatedAt");
			OnPropertyChanged("Urls");
			OnPropertyChanged("Colors");
		}
	}
}
